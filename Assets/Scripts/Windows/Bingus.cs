using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;

public class Bingus : SerializedMonoBehaviour
{
    [OdinSerialize] public Button WinButton { get; private set; }
    [OdinSerialize] public Button LoseButton { get; private set; }
}