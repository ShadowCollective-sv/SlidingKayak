using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Runtime.Scene
{
    public class LevelLoadManager : MonoBehaviour
    {
        [SerializeField] private GameObject _loaderCanvas;
        [SerializeField] private Image _progressBar;

        public async void LoadScene(string sceneName) {
            var scene = SceneManager.LoadSceneAsync(sceneName);
            scene.allowSceneActivation = false; //грузим, но не активируем сразу

            _loaderCanvas.SetActive(true); //Активирует объект
            
            //проверка насколько загружена (параллельная) сцена и изменение шкалы прогресса от этого

            do
            {
                await Task.Delay(100); //Задержка, чтобы загрузка не пролетала мгновенно
                
                _progressBar.fillAmount = scene.progress;
            } while (scene.progress < 0.9f);

            scene.allowSceneActivation = true; //теперь активируем сцену которую мы грузили.
            _loaderCanvas.SetActive(false);//а экран загрузки наоборот отключим
            
        }
    }
}