// ILoadingScreen.cs
namespace Runtime.Infrastructure.Services
{
    public interface ILoadingScreen
    {
        void Show();
        void Hide();
        void SetProgress(float value01);
    }
}