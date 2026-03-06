using UnityEngine;
using Zenject;

public sealed class AttackRadiusVisual : MonoBehaviour
{
    [SerializeField] private Transform circleQuad;
    [SerializeField] private float yOffset;
    [SerializeField] private float yScale;

    private PlayerConfig config;

    [Inject]
    public void Construct(PlayerConfig playerConfig) => config = playerConfig;

    private void Start()
    {
        if (circleQuad == null) return;

        float diameter = config.AttackRadius * 2f;
        circleQuad.localScale = new Vector3(diameter, yScale, diameter);

        Vector3 localPosition = circleQuad.localPosition;
        circleQuad.localPosition = new Vector3(localPosition.x, yOffset, localPosition.z);
    }
}