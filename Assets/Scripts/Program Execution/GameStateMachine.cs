using System;
using System.Collections.Generic;
using Program_Execution.States;

namespace Program_Execution
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IState> _states;
        private IState _currentState;

        public GameStateMachine()
        {
            _states = new Dictionary<Type, IState>()
            {
                [typeof(BootstrapState)] = new BootstrapState(this),
            };
        }
        
        public void Enter<TState>() where TState : IState //ограничиваем Tстейты теми, которые есть в интерфейсе. Это просто шаблон, чтобы его запустить нужно будет например stateMachine.Enter<CombatState>();
        {
            _currentState?.Exit();
            IState state = _states[typeof(TState)];
            _currentState = state;
            state.Enter();
        }
    }
}