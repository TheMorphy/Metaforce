using System;
using UnityEngine;
using Zenject;
using R3;

public sealed class PlayerAutoAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Animator animator;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyMask;

    private PlayerConfig config;

    private readonly CompositeDisposable cd = new();
    private IDisposable attackLoop;
    
    private Transform currentTarget;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    [Inject]
    public void Construct(PlayerConfig playerConfig)
    {
        config = playerConfig;
    }

    private void Start()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();

        animator.SetFloat(AttackSpeedHash, config.SecondsPerAttack * 2);
        
        movement.IsMoving
            .Subscribe(isMoving => animator.SetFloat(SpeedHash, isMoving ? 1f : 0f))
            .AddTo(cd);

        movement.IsMoving
            .DistinctUntilChanged()
            .Subscribe(isMoving =>
            {
                if (isMoving) StopAttackLoop();
                else StartAttackLoop();
            })
            .AddTo(cd);
    }
    
    private void Update()
    {
        if (movement && !movement.IsMoving.CurrentValue)
        {
            currentTarget = FindNearestTarget();
            RotateTowardsTarget(currentTarget);
        }
    }

    private void StartAttackLoop()
    {
        StopAttackLoop();

        attackLoop = Observable
            .Interval(TimeSpan.FromSeconds(config.SecondsPerAttack))
            .Subscribe(_ => TryShootAtCurrentTarget());
    }

    private void StopAttackLoop()
    {
        attackLoop?.Dispose();
        attackLoop = null;
    }

    private void TryShootAtCurrentTarget()
    {
        if (currentTarget == null) return;

        Vector3 origin = muzzle != null ? muzzle.position : (transform.position + Vector3.up * 1.5f);

        Vector3 targetPoint = currentTarget.position + Vector3.up * 1.0f;
        Vector3 dir = (targetPoint - origin).normalized;

        float distToTarget = Vector3.Distance(origin, targetPoint);
        if (distToTarget > config.AttackRadius + 0.5f)
            return;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, config.AttackRadius + 5f, enemyMask))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                animator.SetTrigger(ShootHash);
                dmg.ApplyDamage(config.Damage);
            }
        }
    }

    private Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, config.AttackRadius, enemyMask);
        if (hits == null || hits.Length == 0) return null;

        Transform nearest = null;
        float best = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var t in hits)
        {
            float d = (t.transform.position - p).sqrMagnitude;
            if (d < best)
            {
                best = d;
                nearest = t.transform;
            }
        }
        return nearest;
    }
    
    private void RotateTowardsTarget(Transform target)
    {
        if (!target) return;

        Vector3 to = target.position - transform.position;
        to.y = 0f;

        if (to.sqrMagnitude < 0.01f) return;

        Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * config.RotationSpeed);
    }

    private void OnDestroy()
    {
        StopAttackLoop();
        cd.Dispose();
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, config != null ? config.AttackRadius : 2f);
    }
#endif
}