using System;
using UnityEngine;
using Zenject;
using R3;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn Config")]
    [SerializeField] private EnemySpawnConfig enemySpawnConfig;
    
    [Header("Spawn points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Patrol route")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Options")]
    [SerializeField] private bool randomizeStartIndex = true;

    private DiContainer container;
    
    private DisposableBag disposables;

    [Inject]
    public void Construct(DiContainer diContainer)
    {
        container = diContainer;
    }
    
    private void OnDestroy()
    {
        disposables.Dispose();
    }
    
    private void Start()
    {
        if (Validate()) return;

        int count = Mathf.Max(0, enemySpawnConfig.MaxEnemiesOnLevel);

        for (int i = 0; i < count; i++)
            SpawnEnemy(i);
    }
    
    private bool Validate()
    {
        if (container == null)
        {
            Debug.LogError("EnemySpawner: DiContainer is missing.", this);
            return true;
        }
        
        if (enemySpawnConfig == null)
        {
            Debug.LogError("EnemySpawner: spawn config is missing.", this);
            return true;
        }

        if (enemySpawnConfig.EnemyType == null || enemySpawnConfig.EnemyType.EnemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemy type or prefab is missing.", this);
            return true;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: spawnPoints is empty.", this);
            return true;
        }

        return false;
    }

    private void SpawnEnemy(int index)
    {
        Transform spawnPoint = GetSpawnPoint(index);
        if (spawnPoint == null)
            return;

        GameObject enemy = CreateEnemy(spawnPoint);
        if (enemy == null)
            return;

        SetupPatrol(enemy, index);
        SetupHealth(enemy);
    }

    private Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        int spawnIndex = index % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            Debug.LogWarning($"EnemySpawner: spawn point at index {spawnIndex} is null.", this);
            return null;
        }

        return spawnPoint;
    }

    private GameObject CreateEnemy(Transform spawnPoint)
    {
        return container.InstantiatePrefab(
            enemySpawnConfig.EnemyType.EnemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            null);
    }

    private void SetupPatrol(GameObject enemy, int index)
    {
        EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();
        if (patrol == null)
            return;

        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        int startIndex = randomizeStartIndex
            ? UnityEngine.Random.Range(0, patrolRoute.Count)
            : index % patrolRoute.Count;

        patrol.Init(enemySpawnConfig.EnemyType, patrolRoute, startIndex);
    }

    private void SetupHealth(GameObject enemy)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health == null)
        {
            Debug.LogError("EnemySpawner: spawned enemy has no EnemyHealth component.", enemy);
            return;
        }

        health.Init(enemySpawnConfig.EnemyType);

        health.Died
            .Subscribe(_ => ScheduleRespawn())
            .AddTo(ref disposables);
    }

    private void ScheduleRespawn()
    {
        if (enemySpawnConfig == null)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Observable.Timer(TimeSpan.FromSeconds(enemySpawnConfig.RespawnTimeSeconds))
            .Subscribe(_ => SpawnEnemy(UnityEngine.Random.Range(0, spawnPoints.Length)))
            .AddTo(ref disposables);
    }
}