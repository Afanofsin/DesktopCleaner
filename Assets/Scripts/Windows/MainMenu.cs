using System;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;

public class MainMenu : SerializedMonoBehaviour
{
    [OdinSerialize] private Button _newGameButton;
    [OdinSerialize] private Button _resumeButton;

    private void Awake()
    {
        _resumeButton.enabled = false;

        _newGameButton.OnClickAsObservable().Subscribe(_ => StartNewGame().Forget()).AddTo(this);
        _resumeButton.OnClickAsObservable().Subscribe(_ => ResumeGame()).AddTo(this);
    }

    private async UniTaskVoid StartNewGame()
    {
        GameSequence.Instance.StartNewGame();
        await UniTask.Delay(TimeSpan.FromSeconds(1.25f));
        _resumeButton.enabled = true;
    }

    private void ResumeGame() => GameSequence.Instance.ResumeGame();
}