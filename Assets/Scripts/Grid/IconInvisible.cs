using Grid;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IconInvisible : Graphic
{
    [SerializeField] private Button _button;
    
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }

    protected override void Awake()
    {
        base.Awake();
        _button = GetComponent<Button>();
        //if (_button == null) return;
        //_button.OnClickAsObservable().Subscribe(_ => OnButtonClick()).AddTo(this);
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
    }
    
    public void OnButtonClick()
    {
        GridService.G?.MouseManager.CleanLastIcon();
    }
}
