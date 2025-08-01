using System;
using UnityEngine.SceneManagement;

namespace Program_Execution.States
{
    public class SceneLoadState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public SceneLoadState(GameStateMachine stateMachine, SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            throw  new NotImplementedException();
        }

        public void Enter()
        {
            _sceneLoader.Load("MainMenu");
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}