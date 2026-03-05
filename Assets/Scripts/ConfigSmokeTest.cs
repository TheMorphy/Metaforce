using UnityEngine;
using Zenject;

public sealed class ConfigSmokeTest : MonoBehaviour
{
    [Inject] private PlayerConfig player;
    [Inject] private EnemyConfig enemy;

    private void Start()
    {
        Debug.Log($"Player: speed={player.MoveSpeed}, dmg={player.Damage}");
        Debug.Log($"Enemy: hp={enemy.Hp}, respawn={enemy.RespawnTimeSeconds}, max={enemy.MaxEnemiesOnLevel}");
    }
}