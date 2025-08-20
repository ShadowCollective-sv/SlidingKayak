using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Program_Execution;

namespace Runtime.Bootstrap
{
    public abstract class BaseState : IGameState
    {
        protected readonly GameStateMachine Machine;
        
        protected BaseState(GameStateMachine machine) => Machine = machine;
        
        public virtual UniTask Enter([CanBeNull] object payload, CancellationToken token) => UniTask.CompletedTask;
        public virtual UniTask Exit() => UniTask.CompletedTask;
    }
}