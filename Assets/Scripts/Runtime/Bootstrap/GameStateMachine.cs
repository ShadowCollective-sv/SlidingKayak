// GameStateMachine.cs (фрагменты)

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Bootstrap;
using Runtime.Bootstrap.States;

public sealed class GameStateMachine
{
    private readonly Dictionary<Type, IGameState> _states = new();
    private IGameState _current;
    private CancellationTokenSource _cts;
    private bool _transitionInProgress;

    public void Register<T>(T state) where T : IGameState => _states[typeof(T)] = state;

    public UniTask Enter<T>(object payload = null) where T : IGameState
        => Enter(typeof(T), payload);

    public async UniTask Enter(Type stateType, object payload = null)
    {
        if (_transitionInProgress) return;
        _transitionInProgress = true;

        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            if (_current != null)
                await _current.Exit();

            if (!_states.TryGetValue(stateType, out var next))
                throw new InvalidOperationException($"State {stateType.Name} not registered");

            _current = next;
            await _current.Enter(payload, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // штатная отмена
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
            if (stateType != typeof(MainMenuState) && _states.ContainsKey(typeof(MainMenuState)))
                await Enter(typeof(MainMenuState));
        }
        finally
        {
            _transitionInProgress = false;
        }
    }

    public void CancelCurrent() => _cts?.Cancel();
}