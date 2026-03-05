using UnityEngine;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Patrol route")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Options")]
    [SerializeField] private bool randomizeStartIndex = true;

    [Header("Если нужно создать новый спавнер с другим типом врагов (сейчас Zenject конфиг)")]
    [SerializeField] private EnemySpawnConfig overrideSpawnConfig;
    private EnemySpawnConfig enemySpawnConfig;
    
    private EnemySpawnConfig EffectiveConfig => overrideSpawnConfig != null
        ? overrideSpawnConfig
        : enemySpawnConfig;

    private DiContainer container;

    [Inject]
    public void Construct(EnemySpawnConfig enemySpawnConfig, DiContainer diContainer)
    {
        this.enemySpawnConfig = enemySpawnConfig;
        container = diContainer;
    }

    private void Start()
    {
        SpawnInitial();
    }

    private void SpawnInitial()
    {
        EnemySpawnConfig config = EffectiveConfig;
        
        if (config.EnemyType == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab is null");
            return;
        }
        
        if (config.EnemyType.EnemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemy prefab is null");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: spawnPoints are not set");
            return;
        }
        
        if (patrolRoute == null)
        {
            Debug.LogError("EnemySpawner: no patrol route set, trying to find one...");
            patrolRoute = FindFirstObjectByType< PatrolRoute>();
        }

        int count = Mathf.Max(0, enemySpawnConfig.MaxEnemiesOnLevel);

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

            GameObject enemyInstance = container.InstantiatePrefab(config.EnemyType.EnemyPrefab, spawnPoint.position, spawnPoint.rotation, null);

            var patrol = enemyInstance.GetComponent<EnemyPatrol>();
            
            if (patrol == null)
            {
                Debug.LogWarning("EnemySpawner: spawned enemy has no EnemyPatrol");
                continue;
            }

            PatrolRoute route = ChooseRouteFor(enemyInstance, patrolRoute);

            int startIndex = ChooseStartIndex(route, i);

            patrol.Init(route, startIndex);
        }
    }

    /// <summary>
    /// Точка расширения: в будущем можно выбирать маршрут индивидуально для каждого врага.
    /// Сейчас: всегда defaultRoute.
    /// </summary>
    protected virtual PatrolRoute ChooseRouteFor(GameObject enemyInstance, PatrolRoute fallback)
    {
        return fallback;
    }

    /// <summary>
    /// Точка расширения: политика выбора стартовой точки.
    /// Сейчас: рандом или детерминированно по индексу.
    /// </summary>
    protected virtual int ChooseStartIndex(PatrolRoute route, int spawnOrderIndex)
    {
        if (route == null || route.Count == 0) return 0;

        return randomizeStartIndex
            ? Random.Range(0, route.Count)
            : (spawnOrderIndex % route.Count);
    }
}