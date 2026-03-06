using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Player Config", fileName = "PlayerConfig")]
public sealed class PlayerConfig : ScriptableObject
{
    [Min(0f)] public float MoveSpeed;
    [Min(0f)] public float RotationSpeed;

    [Min(0.01f)] public float SecondsPerAttack;

    [Min(1)] public int Damage;

    [Min(0f)] public float AttackRadius;
}