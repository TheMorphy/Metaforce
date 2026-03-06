using System;
using R3;
using Zenject;

public sealed class KillCounter : IDisposable
{
    private readonly ReactiveProperty<int> kills = new(0);
    public ReadOnlyReactiveProperty<int> Kills => kills;

    private readonly CompositeDisposable cd = new();

    [Inject]
    public KillCounter(GameEvents events)
    {
        events.EnemyKilled
            .Subscribe(_ => kills.Value++)
            .AddTo(cd);
    }

    public void Dispose()
    {
        cd.Dispose();
    }
}