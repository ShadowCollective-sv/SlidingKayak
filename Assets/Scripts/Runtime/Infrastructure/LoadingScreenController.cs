// LoadingScreenController.cs

using Runtime.Infrastructure.Services;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingScreenController : MonoBehaviour, ILoadingScreen
{
    [SerializeField] private CanvasGroup _canvas;
    [SerializeField] private Slider _progress;

    public void Show()
    {
        _canvas.alpha = 1f;
        _canvas.interactable = true;
        _canvas.blocksRaycasts = true;
        SetProgress(0f);
    }

    public void Hide()
    {
        _canvas.alpha = 0f;
        _canvas.interactable = false;
        _canvas.blocksRaycasts = false;
    }

    public void SetProgress(float value01)
    {
        if (_progress != null)
            _progress.value = Mathf.Clamp01(value01);
    }
}

//С этими Show Hide не будет такого, что ассет висит в памяти, хотя он уже не нужен?