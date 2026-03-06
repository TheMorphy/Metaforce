using R3;

public sealed class GameEvents
{
    public readonly Subject<Unit> EnemyKilled = new();
}