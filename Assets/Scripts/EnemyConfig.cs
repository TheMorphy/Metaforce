using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Enemy Config", fileName = "EnemyConfig")]
public sealed class EnemyConfig : ScriptableObject
{
    [Min(1)] public int HP = 3;

    [Min(0f)] public float RespawnTimeSeconds = 2.0f;

    [Min(0)] public int MaxEnemiesOnLevel = 10;

    [Min(0f)] public float MoveSpeed = 2.0f;
}