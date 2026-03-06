using UnityEngine;
using Zenject;

public sealed class AttackRadiusVisual : MonoBehaviour
{
    [SerializeField] private Transform circleQuad;
    [SerializeField] private float yOffset = 0.02f;

    private PlayerConfig config;

    [Inject]
    public void Construct(PlayerConfig playerConfig) => config = playerConfig;

    private void Start()
    {
        if (circleQuad == null) return;

        float diameter = config.AttackRadius * 2f;
        circleQuad.localScale = new Vector3(diameter, 0.01f, diameter);

        var p = circleQuad.localPosition;
        circleQuad.localPosition = new Vector3(p.x, yOffset, p.z);
    }
}