// LevelLoadRequest.cs

using System;
using Runtime.Bootstrap.States;

namespace Runtime.Infrastructure
{
    public sealed class LevelLoadRequest
    {
        public string SceneName { get; }
        public bool ShowLoadingScreen { get; }
        public Type NextState { get; }

        public LevelLoadRequest(string sceneName, bool showLoadingScreen, Type nextState = null)
        {
            SceneName = sceneName;
            ShowLoadingScreen = showLoadingScreen;
            NextState = nextState ?? typeof(GameplayState);
        }
    }
}