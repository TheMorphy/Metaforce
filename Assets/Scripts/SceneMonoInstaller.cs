using UnityEngine;
using Zenject;

public class SceneMonoInstaller : MonoInstaller
{
    [Header("Default Configs")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private EnemyTypeConfig enemyTypeConfig;
    [SerializeField] private EnemySpawnConfig enemySpawnConfig;

    public override void InstallBindings()
    {
        Container.Bind<GameEvents>().AsSingle();

        Container.BindInstance(playerConfig).AsSingle();
        Container.BindInstance(enemyTypeConfig).AsSingle();
        Container.BindInstance(enemySpawnConfig).AsSingle();
    }
}