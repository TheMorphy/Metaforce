using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public sealed class EnemyPatrol : MonoBehaviour
{
    private PatrolRoute route;
    [SerializeField, Min(0f)] private float arriveDistance;
    [SerializeField, Min(0f)] private float waitSecondsAtPoint;

    private CharacterController controller;
    private EnemyTypeConfig typeConfig;

    private int index;
    private float waitTimer;

    [Inject]
    public void Construct(EnemyTypeConfig enemyTypeConfig)
    {
        typeConfig = enemyTypeConfig;
    }
    
    public void Init(PatrolRoute patrolRoute, int startIndex)
    {
        route = patrolRoute;
        index = Mathf.Clamp(startIndex, 0, patrolRoute.Count - 1);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!route || route.Count < 2) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 target = route.GetPoint(index).position;
        target.y = transform.position.y;

        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;

        if (dist <= arriveDistance)
        {
            index = (index + 1) % route.Count;
            waitTimer = waitSecondsAtPoint;
            return;
        }

        Vector3 dir = toTarget / dist;
        controller.Move(dir * (typeConfig.MoveSpeed * Time.deltaTime));

        if (!(dir.sqrMagnitude > 0.0001f)) return;
        
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * typeConfig.RotationSpeed);
    }
}