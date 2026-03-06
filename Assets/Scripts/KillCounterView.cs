using TMPro;
using UnityEngine;
using Zenject;
using R3;

public sealed class KillCounterView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private readonly CompositeDisposable cd = new();

    [Inject]
    public void Construct(KillCounter counter)
    {
        counter.Kills
            .Subscribe(UpdateText)
            .AddTo(cd);
    }

    private void UpdateText(int kills)
    {
        text.text = kills.ToString();
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}