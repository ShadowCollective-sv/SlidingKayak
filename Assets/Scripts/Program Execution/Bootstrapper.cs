using Program_Execution.States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Program_Execution
{
    public class Bootstrapper : MonoBehaviour, ICoroutineRunner
    {
        private Game _game;

        private void Awake()
        {
            _game = new Game();
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(gameObject); //DontDestroyOnLoad(this);
            //SceneManager.LoadScene("MainMenu");
        }
    }
}