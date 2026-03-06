using System;
using R3;
using Zenject;

public sealed class KillCounter : IDisposable
{
    private readonly ReactiveProperty<int> kills = new(0);
    public ReadOnlyReactiveProperty<int> Kills => kills;

    private readonly CompositeDisposable compositeDisposable = new();

    [Inject]
    public KillCounter(GameEvents gameEvents)
    {
        gameEvents.EnemyKilled
            .Subscribe(_ => kills.Value++)
            .AddTo(compositeDisposable);
    }

    public void Dispose()
    {
        compositeDisposable.Dispose();
    }
}