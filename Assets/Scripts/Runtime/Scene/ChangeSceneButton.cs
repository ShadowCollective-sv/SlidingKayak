using UnityEngine;

namespace Runtime.Scene
{
    public class ChangeSceneButton : MonoBehaviour
    {
        public void ChangeScene(string sceneName)
        {
            GameManager.instance._levelLoadManager.LoadScene(sceneName);
        }
    }
}