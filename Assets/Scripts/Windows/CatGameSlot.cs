using Sirenix.OdinInspector;

public class CatGameSlot : SerializedMonoBehaviour
{
    public CatGameCat CurrentCat { get; private set; }
    
    public void AssignCat(CatGameCat cat)
    {
        cat.transform.SetParent(transform, false);
        cat.gameObject.SetActive(true);
        CurrentCat = cat;
    }
}