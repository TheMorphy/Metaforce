using UnityEngine;
using Zenject;
using R3;

public sealed class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyTypeConfig type;

    private GameEvents gameEvents;

    private readonly Subject<Unit> died = new();
    public Observable<Unit> Died => died;

    private readonly ReactiveProperty<int> hp = new(0);
    public ReadOnlyReactiveProperty<int> Hp => hp;

    public int MaxHp { get; private set; }

    private bool dead;
    private bool initialized;

    [Inject]
    public void Construct(GameEvents gameEvents)
    {
        this.gameEvents = gameEvents;
    }

    public void Init(EnemyTypeConfig config)
    {
        type = config;
        MaxHp = type != null ? type.Hp : 1;
        hp.Value = MaxHp;
        dead = false;
        initialized = true;
    }

    private void Awake()
    {
        if (!initialized)
        {
            MaxHp = type != null ? type.Hp : 1;
            hp.Value = MaxHp;
        }
    }

    public void ApplyDamage(int amount)
    {
        if (dead || amount <= 0)
            return;

        int newHp = Mathf.Max(0, hp.Value - amount);
        hp.Value = newHp;

        if (newHp == 0)
            Die();
    }

    private void Die()
    {
        if (dead) return;

        dead = true;

        gameEvents?.EnemyKilled.OnNext(Unit.Default);

        died.OnNext(Unit.Default);
        died.OnCompleted();

        Destroy(gameObject);
    }
}