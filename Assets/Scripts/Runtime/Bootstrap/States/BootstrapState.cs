// BootstrapState.cs — инициализация сервисов/конфига

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Bootstrap.States
{
    public sealed class BootstrapState : BaseState
    {
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(GameStateMachine m, SceneLoader sceneLoader) : base(m)
        {
            _sceneLoader = sceneLoader;
        }

        public override async UniTask Enter(object payload, CancellationToken token)
        {
            // Пример: асинхронная загрузка конфига/локали/сейва
            await UniTask.WhenAll(
                InitializeConfig(token),
                InitializeSave(token),
                InitializeAudio(token)
            );

            // Load the MainMenu scene before transitioning to MainMenuState
            await _sceneLoader.LoadContentScene("MainMenu", true, token: token); // Assuming "MainMenu" is your scene name

            await Machine.Enter<MainMenuState>();
        }

        private async UniTask InitializeConfig(CancellationToken token)
        {
            await UniTask.DelayFrame(1, cancellationToken: token);
            Debug.Log("[Bootstrap] Config ready");
        }

        private async UniTask InitializeSave(CancellationToken token)
        {
            await UniTask.DelayFrame(1, cancellationToken: token);
            Debug.Log("[Bootstrap] Save ready");
        }

        private async UniTask InitializeAudio(CancellationToken token)
        {
            await UniTask.DelayFrame(1, cancellationToken: token);
            Debug.Log("[Bootstrap] Audio ready");
        }
    }
}