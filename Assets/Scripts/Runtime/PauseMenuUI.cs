using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    // Эту функцию мы повесим на кнопку "Resume" (Продолжить).
    public void OnResumeButton()
    {
        // Просим "Босса" снять игру с паузы.
        GameManager.instance.TogglePause();
    }

    // Эту функцию мы повесим на кнопку "Quit to Menu" (Выйти в меню).
    public void OnQuitButton()
    {
        // Просим "Босса" вернуть нас в главное меню.
        GameManager.instance.GoToMainMenu();
    }
}