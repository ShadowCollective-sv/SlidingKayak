// SceneLoader.cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using Runtime.Infrastructure.Services;

public sealed class SceneLoader
{
    private string _activeContentScene;
    private readonly ILoadingScreen _loading;

    public SceneLoader(ILoadingScreen loading) => _loading = loading;

    public async UniTask<Scene> LoadContentScene(
        string sceneName,
        bool showLoadingScreen,
        IProgress<float> progress = null,
        CancellationToken token = default,
        TimeSpan? timeout = null)
    {
        if (showLoadingScreen)
            _loading?.Show();

        // 1) Выгрузка прошлой контент-сцены
        if (!string.IsNullOrEmpty(_activeContentScene))
        {
            var prev = SceneManager.GetSceneByName(_activeContentScene);
            if (prev.IsValid() && prev.isLoaded)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(_activeContentScene);
                if (unloadOp != null)
                    await unloadOp.ToUniTask(cancellationToken: token);
            }

            _activeContentScene = null;
            await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: token);
        }

        // 2) Загрузка новой сцены аддитивно
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;

        // Линкованный токен + таймаут
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (timeout.HasValue)
        {
            // CancelAfter(TimeSpan) не везде доступен — используем миллисекунды
            linkedCts.CancelAfter((int)timeout.Value.TotalMilliseconds);
        }

        await loadOp.ToUniTask(
            Progress.Create<float>(p =>
            {
                float v = Mathf.Clamp01(p) * 0.95f;
                progress?.Report(v);
                _loading?.SetProgress(v);
            }),
            cancellationToken: linkedCts.Token
        );

        // 3) Сделать сцену активной
        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        _activeContentScene = sceneName;

        // 4) Добиваем прогресс до 1.0 «на инициализацию»
        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            float v = 0.95f + 0.05f * ((i + 1) / 3f);
            progress?.Report(v);
            _loading?.SetProgress(v);
        }

        if (showLoadingScreen)
            _loading?.Hide();

        return scene;
    }
}
