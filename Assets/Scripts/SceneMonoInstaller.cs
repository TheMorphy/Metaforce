using UnityEngine;
using Zenject;

public class SceneMonoInstaller : MonoInstaller
{
    [Header("Configs")]
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private EnemyConfig enemyConfig;

    public override void InstallBindings()
    {
        Container.Bind<GameEvents>().AsSingle();

        Container.BindInstance(playerConfig).AsSingle();
        Container.BindInstance(enemyConfig).AsSingle();
    }
}