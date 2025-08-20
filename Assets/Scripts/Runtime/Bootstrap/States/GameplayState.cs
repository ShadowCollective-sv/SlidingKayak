// GameplayState.cs — базовая петля, вход/выход

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Bootstrap.States
{
    public sealed class GameplayState : BaseState
    {
        public GameplayState(GameStateMachine m) : base(m) { }

        private CancellationTokenSource _inputCts;

        public override async UniTask Enter(object payload, CancellationToken token)
        {
            // Пример: подписка на input в фоне
            _inputCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _ = ListenForPause(_inputCts.Token);

            // Инициализация геймплея (спавн систем, HUD и т.п.)
            await UniTask.Yield(token);
            Debug.Log("[Gameplay] Entered");
        }

        public override UniTask Exit()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid ListenForPause(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    await Machine.Enter<PauseState>();
                    return;
                }
            }
        }
    }
}