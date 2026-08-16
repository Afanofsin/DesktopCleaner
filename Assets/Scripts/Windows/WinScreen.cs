using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;

public class WinScreen : SerializedMonoBehaviour
{
    [OdinSerialize] private Button _okButton;

    private void Awake() => _okButton.OnClickAsObservable().Subscribe(_ => GameSequence.Instance.GameWon());
}