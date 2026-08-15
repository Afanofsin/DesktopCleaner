using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class MainWindow : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    protected DisposableBag Bag;
    
    protected virtual void Awake() => closeButton.OnClickAsObservable() .Subscribe(OnCloseButtonClick).AddTo(ref Bag);

    protected virtual void OnCloseButtonClick(Unit _) => Hide();
    
    protected void Hide() => gameObject.SetActive(false);
    protected void Show() => gameObject.SetActive(true);
    
    protected virtual void OnDestroy() => Bag.Dispose();
}