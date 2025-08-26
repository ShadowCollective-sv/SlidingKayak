using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private Slider progressBar;

    public void Show(float startValue = 0f)
    {
        if (canvas != null)
        {
            canvas.alpha = 1f;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
        }
        SetProgress(startValue);
    }

    public void Hide()
    {
        if (canvas != null)
        {
            canvas.alpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }
    }

    public void SetProgress(float value01)
    {
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(value01);
    }
}