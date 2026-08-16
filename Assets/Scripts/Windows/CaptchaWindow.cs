using PrimeTween;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class CaptchaWindow : MainWindow
{
    [SerializeField] private Image muzhikImage;
    private Sequence _muzhikSequence;
    protected override bool IsDraggable => false;

    private const float StartPos = -1200f;
    private const float EndPos = -80f;
    
    protected override void Awake()
    {
        base.Awake();
        muzhikImage.transform.localPosition = new Vector3(0f, StartPos, 0f);
        muzhikImage.enabled = false;
    }

    protected override void OnCloseButtonClick(Unit _)
    {
        _muzhikSequence.Complete();
        
        muzhikImage.transform.localPosition = new Vector3(0, StartPos, 0f);
        muzhikImage.color = Color.white;
        muzhikImage.enabled = true;

        _muzhikSequence = Sequence.Create()
            .Chain(Tween.LocalPositionY(muzhikImage.transform, EndPos, 0.65f, Ease.OutBounce))
            .ChainDelay(1.25f)
            .Chain(Tween.Alpha(muzhikImage, 0f, 0.666f))
            .OnComplete(this, static window => window.muzhikImage.enabled = false);
    }
}