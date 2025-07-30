namespace Program_Execution.States
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _stateMachine;

        public BootstrapState(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        
        public void Enter() //все нужные вещи выполняются на входе в состояние или на выходе из него.
        {
            RegisterServices(); //тут будем регистрировать сервисы
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