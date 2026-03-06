using UnityEngine;
using UnityEngine.UI;
using R3;
using DG.Tweening;

public sealed class EnemyHpBarView : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    
    [Header("Hp Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private float tweenDuration = 0.2f;

    private readonly CompositeDisposable cd = new();
    private Tween currentTween;

    private void Awake()
    {
        if (!health)
            health = GetComponentInParent<EnemyHealth>();
    }

    private void Start()
    {
        slider.maxValue = health.MaxHp;
        slider.value = health.Hp.CurrentValue;

        health.Hp
            .Subscribe(OnHpChanged)
            .AddTo(cd);
    }

    private void OnHpChanged(int hp)
    {
        currentTween?.Kill();
        currentTween = slider.DOValue(hp, tweenDuration);
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
        cd.Dispose();
    }
}