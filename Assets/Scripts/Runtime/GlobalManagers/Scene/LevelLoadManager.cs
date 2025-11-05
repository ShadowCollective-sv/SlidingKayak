using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Runtime.Scene
{
    public class LevelLoadManager : MonoBehaviour
    {
        [SerializeField] private GameObject sceneLoadComponentsPrefabRef;
        
        public static LevelLoadManager Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async void ExpandedLoadScene(string sceneName) 
        {
            GameObject loadingScreen = Instantiate(sceneLoadComponentsPrefabRef); //Тут у нас сразу появляется экран загрузки при инстанцировании, отдельный Canvas не нужен
            Image progressBar = loadingScreen.GetComponentInChildren<Image>();

            if (progressBar == null)
            {
                Debug.LogError("Компонент Image не найден в префабе загрузочного экрана.");
                Destroy(loadingScreen);
                return;
            }
 
            loadingScreen.SetActive(true); 
            
            AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);  //можно было бы написать еще var sceneLoadOperation
            sceneLoadOperation.allowSceneActivation = false; 
            

            do
            {
                await Task.Delay(100);
                progressBar.fillAmount = sceneLoadOperation.progress / 0.9f; //нормализация, значение 0,9 берется из особенностей работы AsyncOperation.progress
                Debug.Log($"Scene loading progress: {sceneLoadOperation.progress:P0}");
                
            } while (sceneLoadOperation.progress < 0.9f);

            sceneLoadOperation.allowSceneActivation = true;
            
            loadingScreen.SetActive(false);
            Destroy(loadingScreen);
        }
    }
}