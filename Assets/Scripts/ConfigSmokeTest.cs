using UnityEngine;
using Zenject;

public sealed class ConfigSmokeTest : MonoBehaviour
{
    [Inject] private PlayerConfig _player;
    [Inject] private EnemyConfig _enemy;

    private void Start()
    {
        Debug.Log($"Player: speed={_player.MoveSpeed}, dmg={_player.Damage}");
        Debug.Log($"Enemy: hp={_enemy.HP}, respawn={_enemy.RespawnTimeSeconds}, max={_enemy.MaxEnemiesOnLevel}");
    }
}