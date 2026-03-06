using System;
using UnityEngine;
using Zenject;
using R3;

[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerAutoAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Animator animator;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyMask;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float attackAnimationSpeedMultiplier = 2f;

    private PlayerConfig config;

    private readonly CompositeDisposable disposables = new();
    private readonly Collider[] targetBuffer = new Collider[32];

    private IDisposable attackLoop;
    private Transform currentTarget;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    private const float DefaultMuzzleHeight = 1.5f;
    private const float TargetAimHeight = 1f;
    private const float MinShotDistance = 0.001f;
    private const float TargetRangeTolerance = 0.5f;
    private const float RaycastDistancePadding = 0.1f;
    private const float MinRotateSqrMagnitude = 0.01f;

    [Inject]
    public void Construct(PlayerConfig playerConfig)
    {
        config = playerConfig;
    }

    private void Awake()
    {
        if (!ValidateAndCacheRefs())
        {
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enabled)
            return;

        animator.SetFloat(AttackSpeedHash, config.SecondsPerAttack * attackAnimationSpeedMultiplier);

        movement.IsMoving
            .DistinctUntilChanged()
            .Subscribe(OnMovementStateChanged)
            .AddTo(disposables);

        movement.IsMoving
            .Subscribe(isMoving => animator.SetFloat(SpeedHash, isMoving ? 1f : 0f))
            .AddTo(disposables);
    }

    private bool ValidateAndCacheRefs()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (movement == null)
        {
            Debug.LogError("PlayerAutoAttack: PlayerMovement is missing.", this);
            return false;
        }

        if (animator == null)
        {
            Debug.LogError("PlayerAutoAttack: Animator is missing.", this);
            return false;
        }

        if (config == null)
        {
            Debug.LogError("PlayerAutoAttack: PlayerConfig is missing.", this);
            return false;
        }

        if (config.SecondsPerAttack <= 0f)
        {
            Debug.LogError("PlayerAutoAttack: SecondsPerAttack must be > 0.", this);
            return false;
        }

        if (config.AttackRadius < 0f)
        {
            Debug.LogError("PlayerAutoAttack: AttackRadius must be >= 0.", this);
            return false;
        }

        return true;
    }

    private void Update()
    {
        if (!enabled || movement.IsMoving.CurrentValue)
            return;

        RefreshCurrentTarget();
        RotateTowardsTarget(currentTarget);
    }

    private void OnMovementStateChanged(bool isMoving)
    {
        if (isMoving)
        {
            StopAttackLoop();
            currentTarget = null;
            return;
        }

        StartAttackLoop();
    }

    private void StartAttackLoop()
    {
        StopAttackLoop();

        TryShootAtCurrentTarget();

        attackLoop = Observable
            .Interval(TimeSpan.FromSeconds(config.SecondsPerAttack))
            .Subscribe(_ => TryShootAtCurrentTarget());
    }

    private void StopAttackLoop()
    {
        attackLoop?.Dispose();
        attackLoop = null;
    }

    private void RefreshCurrentTarget()
    {
        if (IsTargetValid(currentTarget))
            return;

        currentTarget = FindNearestTarget();
    }

    private bool IsTargetValid(Transform target)
    {
        if (!target)
            return false;

        Vector3 from = transform.position;
        Vector3 to = target.position;
        from.y = 0f;
        to.y = 0f;

        float maxDistance = config.AttackRadius + TargetRangeTolerance;
        return (to - from).sqrMagnitude <= maxDistance * maxDistance;
    }

    private void TryShootAtCurrentTarget()
    {
        if (!TryGetShotData(out IDamageable damageable))
            return;

        animator.SetTrigger(ShootHash);
        damageable.ApplyDamage(config.Damage);
    }

    private bool TryGetShotData(out IDamageable damageable)
    {
        damageable = null;

        RefreshCurrentTarget();
        if (currentTarget == null)
            return false;

        Vector3 origin = GetShotOrigin();
        Vector3 targetPoint = GetTargetAimPoint(currentTarget);
        Vector3 shotVector = targetPoint - origin;

        float distanceToTarget = shotVector.magnitude;
        if (distanceToTarget <= MinShotDistance)
            return false;

        if (distanceToTarget > config.AttackRadius + TargetRangeTolerance)
        {
            currentTarget = null;
            return false;
        }

        Vector3 direction = shotVector / distanceToTarget;

        if (!Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                distanceToTarget + RaycastDistancePadding,
                enemyMask))
        {
            return false;
        }

        if (!IsHitMatchingCurrentTarget(hit.collider.transform))
            return false;

        damageable = hit.collider.GetComponent<IDamageable>()
                     ?? hit.collider.GetComponentInParent<IDamageable>();

        return damageable != null;
    }

    private Vector3 GetShotOrigin()
    {
        return muzzle != null
            ? muzzle.position
            : transform.position + Vector3.up * DefaultMuzzleHeight;
    }

    private static Vector3 GetTargetAimPoint(Transform target)
    {
        return target.position + Vector3.up * TargetAimHeight;
    }

    private bool IsHitMatchingCurrentTarget(Transform hitTransform)
    {
        if (currentTarget == null || hitTransform == null)
            return false;

        return hitTransform == currentTarget
               || hitTransform.IsChildOf(currentTarget)
               || currentTarget.IsChildOf(hitTransform);
    }

    private Transform FindNearestTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            config.AttackRadius,
            targetBuffer,
            enemyMask);

        if (hitCount <= 0)
            return null;

        Transform nearestTarget = null;
        float bestSqrDistance = float.MaxValue;
        Vector3 selfPosition = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = targetBuffer[i];
            targetBuffer[i] = null;

            if (!hit)
                continue;

            IDamageable damageable = hit.GetComponent<IDamageable>()
                                    ?? hit.GetComponentInParent<IDamageable>();

            if (damageable is not Component damageableComponent)
                continue;

            Transform candidateTarget = damageableComponent.transform;

            Vector3 from = selfPosition;
            Vector3 to = candidateTarget.position;
            from.y = 0f;
            to.y = 0f;

            float sqrDistance = (to - from).sqrMagnitude;
            if (sqrDistance >= bestSqrDistance)
                continue;

            bestSqrDistance = sqrDistance;
            nearestTarget = candidateTarget;
        }

        return nearestTarget;
    }

    private void RotateTowardsTarget(Transform target)
    {
        if (!target)
            return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < MinRotateSqrMagnitude)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * config.RotationSpeed);
    }

    private void OnDestroy()
    {
        StopAttackLoop();
        disposables.Dispose();
    }
}