using Runtime.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HauntedHouses
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private string newGameSceneName;
        
        private void Start()
        {
            //твин анимации облаков
            Debug.Log("что-то");
        }
        
        public void StartNewGame()
        {
            //SceneManager.LoadScene(newGameSceneName);
            LevelLoadManager.Instance.ExpandedLoadScene(newGameSceneName);
        }

    }
}
