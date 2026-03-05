using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Enemy Type Config", fileName = "EnemyTypeConfig")]
public sealed class EnemyTypeConfig : ScriptableObject
{
    public GameObject EnemyPrefab;
    
    [Min(1)] public int Hp;

    [Min(0f)] public float MoveSpeed;
    [Min(0f)] public float RotationSpeed;
}