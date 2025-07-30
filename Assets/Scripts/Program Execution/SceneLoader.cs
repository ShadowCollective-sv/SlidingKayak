using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Program_Execution
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) => _coroutineRunner = coroutineRunner;

        public void Load(string name, Action onLoaded = null) => _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));

        private IEnumerator LoadScene(string sceneName, Action onLoaded = null) //опциональный callback
        {
            //+ тут нужна ассинхронная загрузка, сцена + экран загрузки
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(sceneName); // нам нужно дождаться ее выполнения
            while (!waitNextScene.isDone)
                yield return null;
        }
    }
}


//waitNextScene.completed += _ => onLoaded?.Invoke(); // в этом случае мы будем знать только о начале и конце операции.
//waitNextScene.isDone = true; //а в этом случае опрос происходит постоянно. Это мы делаем через корутину