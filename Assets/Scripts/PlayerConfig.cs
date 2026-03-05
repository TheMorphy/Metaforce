using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Player Config", fileName = "PlayerConfig")]
public sealed class PlayerConfig : ScriptableObject
{
    [Min(0f)] public float MoveSpeed = 5f;

    [Min(0.01f)] public float SecondsPerAttack = 0.5f;

    [Min(1)] public int Damage = 1;

    [Min(0f)] public float AttackRadius = 2.0f;
}