// Bootstrapper.cs — точка входа в Persistent сцене

using Runtime.Bootstrap.States;
using Runtime.Infrastructure;
using Runtime.Infrastructure.Services;
using Scriptable_Object_Templates.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Bootstrap
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private LoadingScreenController _loadingScreen;
        [SerializeField] private ScenesConfig _scenesConfig;

        private GameStateMachine _machine;

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Регистрация сервисов
            ServiceLocator.Register<ILoadingScreen>(_loadingScreen);
            var sceneLoader = new SceneLoader(_loadingScreen);
            ServiceLocator.Register(sceneLoader);

            // Тут же регистрируйте Save/Config/Audio/etc.
            // ServiceLocator.Register<ISaveService>(new SaveService(...));

            // StateMachine + стейты
            _machine = new GameStateMachine();

            _machine.Register(new BootstrapState(_machine));
            _machine.Register(new MainMenuState(_machine));
            _machine.Register(new LoadLevelState(_machine, sceneLoader));
            _machine.Register(new GameplayState(_machine));
            _machine.Register(new PauseState(_machine));

            await _machine.Enter<BootstrapState>();
        }
    }
}