// PauseState.cs — пауза с возвратом

using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Infrastructure;
using UnityEngine;

namespace Runtime.Bootstrap.States
{
    public sealed class PauseState : BaseState
    {
        private PauseMenuView _view;

        public PauseState(GameStateMachine m) : base(m) { }

        public override UniTask Enter(object payload, CancellationToken token)
        {
            Time.timeScale = 0f;
            _view = Object.FindFirstObjectByType<PauseMenuView>(FindObjectsInactive.Include);
            if (_view != null)
            {
                _view.ResumeClicked += OnResume;
                _view.QuitToMenuClicked += OnQuitToMenu;
                _view.Show();
            }
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            Time.timeScale = 1f;
            if (_view != null)
            {
                _view.ResumeClicked -= OnResume;
                _view.QuitToMenuClicked -= OnQuitToMenu;
                _view.Hide();
            }
            return UniTask.CompletedTask;
        }

        private async void OnResume() => await Machine.Enter<GameplayState>();

        private async void OnQuitToMenu()
        {
            var req = new LevelLoadRequest(sceneName: "Menu", showLoadingScreen: true, nextState: typeof(MainMenuState));
            await Machine.Enter<LoadLevelState>(req);
        }
    }

// простой view для паузы
    public sealed class PauseMenuView : MonoBehaviour
    {
        public event System.Action ResumeClicked;
        public event System.Action QuitToMenuClicked;
        [SerializeField] private CanvasGroup _canvas;

        public void Show() { _canvas.alpha = 1; _canvas.blocksRaycasts = true; _canvas.interactable = true; }
        public void Hide() { _canvas.alpha = 0; _canvas.blocksRaycasts = false; _canvas.interactable = false; }

        public void OnResumeButton() => ResumeClicked?.Invoke();
        public void OnQuitToMenuButton() => QuitToMenuClicked?.Invoke();
    }
}