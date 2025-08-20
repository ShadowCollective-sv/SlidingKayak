// LoadLevelState.cs
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Runtime.Bootstrap;
using Runtime.Infrastructure;

public sealed class LoadLevelState : BaseState
{
    private readonly SceneLoader _loader;
    public LoadLevelState(GameStateMachine m, SceneLoader loader) : base(m) => _loader = loader;

    public override async UniTask Enter(object payload, CancellationToken token)
    {
        var req = payload as LevelLoadRequest ?? new LevelLoadRequest("Level_01", true);

        var progress = Progress.Create<float>(_ => { /* телеметрия, лог */ });

        await _loader.LoadContentScene(
            req.SceneName,
            req.ShowLoadingScreen,
            progress,
            token,
            timeout: TimeSpan.FromSeconds(90));

        await Machine.Enter(req.NextState); // <-- больше не хардкодим GameplayState
    }
}

//Нет Exit состоияния