using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class GameSequence : SerializedMonoBehaviour
{
    [OdinSerialize] private GameObject _gamePrefab;
    [OdinSerialize] private Image _blackScreen;
    
    public static GameSequence Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(this); 
    }

    private Tween FadeInBlackScreen()
    {
        _blackScreen.enabled = true;
        return Tween.Alpha(_blackScreen, 1f, 1.25f);
    }

    private Sequence FadeOutBlackScreen()
    {
        _blackScreen.enabled = true;
        
        return Sequence.Create()
            .Chain(Tween.Alpha(_blackScreen, 0f, 1.25f))
            .ChainCallback(this, sequence => sequence._blackScreen.enabled = false);
    }

    public void StartNewGame()
    {
        _blackScreen.enabled = true;
    }

    public void ResumeGame()
    {
        
    }
}

public class MainMenu : SerializedMonoBehaviour
{
    [OdinSerialize] private Button _newGameButton;
    [OdinSerialize] private Button _resumeButton;

    private void Awake()
    {
        _resumeButton.enabled = false;

        _newGameButton.OnClickAsObservable().Subscribe(_ => StartNewGame().Forget()).AddTo(this);
        _newGameButton.OnClickAsObservable().Subscribe(_ => ResumeGame()).AddTo(this);
    }

    private async UniTaskVoid StartNewGame()
    {
        GameSequence.Instance.StartNewGame();
        await UniTask.Delay(TimeSpan.FromSeconds(1.25f));
        _resumeButton.enabled = true;
    }

    private void ResumeGame() => GameSequence.Instance.ResumeGame();
}