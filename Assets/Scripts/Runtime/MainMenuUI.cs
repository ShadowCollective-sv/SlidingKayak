using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // Эту функцию мы повесим на кнопку "Play" в инспекторе.
    public void OnPlayButton()
    {
        // Просто просим "Босса" начать игру.
        GameManager.instance.StartGame();
    }
}