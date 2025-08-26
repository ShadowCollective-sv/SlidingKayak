using UnityEngine;

namespace Runtime.Scene
{
    public class ChangeSceneButton : MonoBehaviour
    {
        private LevelLoadManager _levelLoadManager;
        
        public void ChangeScene(string sceneName)
        {
            GameManager.instance._levelLoadManager.LoadScene(sceneName);
            _levelLoadManager.LoadScene(sceneName); //зачем этот метод делать статичным?
        }
    }
}