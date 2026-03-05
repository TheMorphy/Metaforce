using R3;

public sealed class GameEvents
{
    public readonly Subject<Unit> EnemyKilled = new();
    public readonly Subject<Unit> EnemySpawned = new();
}