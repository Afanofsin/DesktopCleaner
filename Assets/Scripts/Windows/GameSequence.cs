using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class GameSequence : SerializedMonoBehaviour
{
    [OdinSerialize] private GameObject _mainMenu;
    [OdinSerialize] private GameObject _gamePrefab;
    [OdinSerialize] private Image _blackScreen;
    
    public bool IsRunning { get; private set; }

    [OdinSerialize] private GameObject _gameRoot;

    private GameObject _current;

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
        
        _blackScreen.color = Color.black;

        Sequence.Create()
            .ChainDelay(1f)
            .Chain(FadeOutBlackScreen());
    }

    public void StartNewGame()
    {
        IsRunning = false;
        
        Sequence.Create()
            .Chain(FadeInBlackScreen())
            .ChainCallback(this, static manager =>
            {
                manager.ReplaceGame().Forget();
                manager._mainMenu.gameObject.SetActive(false);
            })
            .ChainDelay(1f)
            .Chain(FadeOutBlackScreen())
            .ChainDelay(1f)
            .ChainCallback(this, static manager => manager.IsRunning = true);
    }

    public void ResumeGame()
    {
        if(_current == null) return;
        
        Sequence.Create()
            .Chain(FadeInBlackScreen())
            .ChainCallback(this, static manager =>
            {
                manager._mainMenu.gameObject.SetActive(false);
                manager._current.gameObject.SetActive(true);
            })
            .ChainDelay(1f)
            .Chain(FadeOutBlackScreen())
            .ChainDelay(1f)
            .ChainCallback(this, static manager => manager.IsRunning = true);
    }

    public void ExitGame()
    {
        IsRunning = false;
        
        Sequence.Create()
            .Chain(FadeInBlackScreen())
            .ChainCallback(this, static manager =>
            {
                manager._mainMenu.gameObject.SetActive(true);
                manager._current?.gameObject.SetActive(false);
            })
            .ChainDelay(1f)
            .Chain(FadeOutBlackScreen());
    }

    private async UniTask ReplaceGame()
    {
        if (_current != null)
        {
            GameObject oldGame = _current;
            _current = null;

            oldGame.SetActive(false);
            Destroy(oldGame);

            await UniTask.NextFrame();
        }

        _current = Instantiate(_gamePrefab, _gameRoot.transform, false);
        _current.transform.SetAsFirstSibling();
    }

    private Tween FadeInBlackScreen()
    {
        _blackScreen.enabled = true;
        return Tween.Alpha(_blackScreen, 1f, 0.75f);
    }

    private Sequence FadeOutBlackScreen()
    {
        _blackScreen.enabled = true;

        return Sequence.Create()
            .Chain(Tween.Alpha(_blackScreen, 0f, 0.75f))
            .ChainCallback(this, sequence => sequence._blackScreen.enabled = false);
    }
}