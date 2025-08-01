namespace Program_Execution.States
{
    public class BootstrapState : IState
    {
        private const string Bootstrapper = "Bootstrapper";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
        }

        
        public void Enter() //все нужные вещи выполняются на входе в состояние или на выходе из него.
        {
            RegisterServices(); //тут будем регистрировать сервисы
            _sceneLoader.Load(Bootstrapper, EnterLoadLevel);
        }   

        private void EnterLoadLevel()
        {
            _stateMachine.Enter<SceneLoadState>(); //по идее это класс-состояние
        }


        public void Exit()
        {
            
        }
        
        private void RegisterServices()
        {
            //тут регаем сервисы которые нам нужны.
            //Аналитика
        }
    }
}