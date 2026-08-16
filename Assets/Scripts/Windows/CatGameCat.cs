using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;

public class CatGameCat : SerializedMonoBehaviour
{
    [OdinSerialize] public bool IsBad { get; private set; } 
    public Button button;
}