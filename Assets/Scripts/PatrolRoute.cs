using UnityEngine;

public sealed class PatrolRoute : MonoBehaviour
{
    [SerializeField] private Transform[] points;

    public int Count => points?.Length ?? 0;
    public Transform GetPoint(int index) => points[index];

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;

        for (int i = 0; i < points.Length; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Length];
            if (a == null || b == null) continue;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(a.position, 0.1f);
            Gizmos.DrawLine(a.position, b.position);
        }
    }
#endif
}