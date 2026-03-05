using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Enemy Spawn Config", fileName = "EnemySpawnConfig")]
public class EnemySpawnConfig : ScriptableObject
{
    [Header("What to spawn")]
    public EnemyTypeConfig EnemyType;

    [Header("Spawn rules")]
    [Min(0f)] public float RespawnTimeSeconds;
    [Min(0)] public int MaxEnemiesOnLevel;
}
