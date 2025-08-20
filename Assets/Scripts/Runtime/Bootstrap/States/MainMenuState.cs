// MainMenuState.cs — показ меню и переход по событию "Play"

using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Infrastructure;
using UnityEngine;

namespace Runtime.Bootstrap.States
{
    public sealed class MainMenuState : BaseState
    {
        public MainMenuState(GameStateMachine m) : base(m) { }

        private MainMenuView _view;

        public override UniTask Enter(object payload, CancellationToken token)
        {
            // Найти/создать UI меню (в Persistent сцене), подписаться на события
            _view = Object.FindFirstObjectByType<MainMenuView>(FindObjectsInactive.Include);
            if (_view != null)
                _view.PlayClicked += OnPlayClicked;

            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            if (_view != null)
                _view.PlayClicked -= OnPlayClicked;
            return UniTask.CompletedTask;
        }

        private async void OnPlayClicked()
        {
            var request = new LevelLoadRequest(sceneName: "Level_01", showLoadingScreen: true);
            await Machine.Enter<LoadLevelState>(request);
        }
    }

// пример простого вью
    public sealed class MainMenuView : MonoBehaviour
    {
        public event System.Action PlayClicked;
        public void OnPlayButton() => PlayClicked?.Invoke();
    }
}