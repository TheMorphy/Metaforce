using UnityEngine;
using Zenject;

public class SceneMonoInstaller : MonoInstaller
{
    [Header("Default Configs")]
    [SerializeField] private PlayerConfig playerConfig;

    public override void InstallBindings()
    {
        Container.Bind<GameEvents>().AsSingle();
        Container.Bind<KillCounter>().AsSingle();

        Container.BindInstance(playerConfig).AsSingle();
    }
}