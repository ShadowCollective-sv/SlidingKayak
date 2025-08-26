using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader
{
    private readonly LoadingScreen _loading;
    private string _activeContentScene; // имя текущей "контент-сцены"

    public SimpleSceneLoader(LoadingScreen loading)
    {
        _loading = loading;
    }

    public async UniTask LoadContentScene(string sceneName)
    {
        // 0) UI: показать загрузку
        _loading?.Show(0f);

        // 1) Выгрузка предыдущей контент-сцены (если есть)
        if (!string.IsNullOrEmpty(_activeContentScene) && _activeContentScene != sceneName)
        {
            var unloadOp = SceneManager.UnloadSceneAsync(_activeContentScene);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone)
                {
                    // Двигаем прогресс плавно до ~0.3
                    _loading?.SetProgress(Mathf.Lerp(_loadingProgress, 0.3f, 0.25f));
                    await UniTask.Yield();
                }
            }
            _activeContentScene = null;

            // Чуть подчистим память
            await Resources.UnloadUnusedAssets();
        }

        // 2) Загрузка новой сцены аддитивно
        var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
        {
            // progress идёт от 0 до ~0.9, добьём сами
            float p = Mathf.Clamp01(loadOp.progress / 0.9f);
            _loading?.SetProgress(Mathf.Lerp(0.3f, 0.95f, p));
            await UniTask.Yield();
        }

        // 3) Сделать её активной
        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        _activeContentScene = sceneName;

        // 4) Финальный штришок до 1.0 (инициализации на сцене)
        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield();
            float v = 0.95f + 0.05f * ((i + 1) / 3f);
            _loading?.SetProgress(v);
        }

        // 5) UI: спрятать загрузку
        _loading?.Hide();
    }

    private float _loadingProgress = 0f; // чисто для плавности если нужно где-то хранить
}
