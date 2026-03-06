using System;
using UnityEngine;
using Zenject;
using R3;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Patrol route")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Options")]
    [SerializeField] private bool randomizeStartIndex = true;

    [SerializeField] private EnemySpawnConfig overrideSpawnConfig;

    private EnemySpawnConfig defaultSpawnConfig;
    private DiContainer container;

    private EnemySpawnConfig Config => overrideSpawnConfig != null
        ? overrideSpawnConfig
        : defaultSpawnConfig;

    [Inject]
    public void Construct(EnemySpawnConfig spawnConfig, DiContainer diContainer)
    {
        defaultSpawnConfig = spawnConfig;
        container = diContainer;
    }

    private void Start()
    {
        int count = Config.MaxEnemiesOnLevel;

        for (int i = 0; i < count; i++)
            SpawnEnemy(i);
    }

    private void SpawnEnemy(int index)
    {
        Transform spawnPoint = spawnPoints[index % spawnPoints.Length];

        GameObject enemy = container.InstantiatePrefab(
            Config.EnemyType.EnemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            null);

        EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
        if (patrol != null)
        {
            int startIndex = randomizeStartIndex
                ? UnityEngine.Random.Range(0, patrolRoute.Count)
                : index % patrolRoute.Count;

            patrol.Init(patrolRoute, startIndex);
        }

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (health != null)
        {
            health.Died.Subscribe(_ =>
            {
                ScheduleRespawn();
            }).AddTo(this);
        }
    }

    private void ScheduleRespawn()
    {
        Observable.Timer(TimeSpan.FromSeconds(Config.RespawnTimeSeconds))
            .Subscribe(_ =>
            {
                SpawnEnemy(UnityEngine.Random.Range(0, spawnPoints.Length));
            })
            .AddTo(this);
    }
}