using System.Threading;
using Cysharp.Threading.Tasks;

namespace Runtime.Bootstrap
{
    public interface IGameState
    {
        // Вход в состояние. token отменяет длинные операции (загрузка/инициализация).
        UniTask Enter(object payload, CancellationToken token);
    
        // Выход из состояния (чистка подписок, UI и пр.)
        UniTask Exit();
    }
}