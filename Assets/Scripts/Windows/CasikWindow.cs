using Grid.Services;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class CasikWindow : MainWindow
{
    [SerializeField] private Button button;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip mainClip;    
    [SerializeField] private AudioSource audioSource;
    
    
    protected override void Awake()
    {
        base.Awake();
        audioSource.clip = mainClip;
        audioSource.Play();
        button.OnClickAsObservable().Subscribe(OnButtonClick).AddTo(ref Bag);
    }

    private void OnButtonClick(Unit _) => audioSource.PlayOneShot(clickClip);

    protected override void OnCloseButtonClick(Unit _)
    {
        GameStateService.G?.OnEventEnded.OnNext(Unit.Default);
        base.OnCloseButtonClick(_);
    }
}