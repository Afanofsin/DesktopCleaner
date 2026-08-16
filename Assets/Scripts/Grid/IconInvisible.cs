using Grid;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IconInvisible : Graphic
{
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
    }
    
}
