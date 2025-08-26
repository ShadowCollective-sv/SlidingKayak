public enum AfterLoad
{
    GoToGameplay,
    GoToMainMenu
}

public class LevelLoadRequest
{
    public string SceneName;
    public AfterLoad After;

    public LevelLoadRequest(string sceneName, AfterLoad after)
    {
        SceneName = sceneName;
        After = after;
    }
}