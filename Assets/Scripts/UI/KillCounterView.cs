using TMPro;
using UnityEngine;
using Zenject;
using R3;

public sealed class KillCounterView : MonoBehaviour
{
    [SerializeField] private TMP_Text killCounterText;

    private readonly CompositeDisposable compositeDisposable = new();

    [Inject]
    public void Construct(KillCounter counter)
    {
        counter.Kills
            .Subscribe(UpdateText)
            .AddTo(compositeDisposable);
    }

    private void UpdateText(int kills)
    {
        if (killCounterText == null)
        {
            Debug.LogError("KillCounterView: kill counter text is null");
            return;
        }
        
        killCounterText.text = kills.ToString();
    }

    private void OnDestroy()
    {
        compositeDisposable.Dispose();
    }
}