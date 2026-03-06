using UnityEngine;
using UnityEngine.UI;
using R3;
using DG.Tweening;

public sealed class EnemyHpBarView : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    
    [Header("Hp Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private float tweenDuration;

    private readonly CompositeDisposable compositeDisposable = new();
    private Tween currentTween;

    private void Awake()
    {
        if (!health)
            health = GetComponentInParent<EnemyHealth>();

        if (health == null)
        {
            Debug.LogError("EnemyHpBarView: EnemyHealth was not found.", this);
            enabled = false;
            return;
        }

        if (slider == null)
        {
            Debug.LogError("EnemyHpBarView: Slider reference is missing.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        slider.maxValue = health.MaxHp;
        slider.value = health.Hp.CurrentValue;

        health.Hp
            .Subscribe(OnHpChanged)
            .AddTo(compositeDisposable);
    }

    private void OnHpChanged(int hp)
    {
        currentTween?.Kill();
        currentTween = slider.DOValue(hp, tweenDuration);
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
        compositeDisposable.Dispose();
    }
}