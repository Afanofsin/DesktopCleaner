using System;
using System.Collections.Generic;
using System.Linq;
using Grid.Services;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Captcha : SerializedMonoBehaviour
{
    [OdinSerialize] private List<CaptchaGame> _games = new();
    [OdinSerialize] private Button _startButton;

    [OdinSerialize] private Image _progressBar;
    [OdinSerialize] private Image _numberImage;
    [OdinSerialize] private List<Sprite> _numberSprites;
    [OdinSerialize] private TextMeshProUGUI _pointsText;
    [OdinSerialize] private AudioSource _audioSource;
    [OdinSerialize] private AudioClip _winClip;
    [OdinSerialize] private AudioClip _loseClip;
    [OdinSerialize] private TextMeshProUGUI _taskText;
    [OdinSerialize] private AudioClip _numberClip;
    [OdinSerialize] private CanvasGroup _canvasGroup;

    [OdinSerialize] private GameObject _progressHolder;
    
    private readonly CompositeDisposable _gameSubs = new();
    
    private CaptchaGame _current;

    private float _timeLeft;
    private Material _progressBarMaterial;

    private static readonly Vector3 StartScale = new (1.44f, 1.44f, 1.44f);
    private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");

    private void Awake()
    {
        _progressBarMaterial = Instantiate(_progressBar.material);
        _progressBar.material = _progressBarMaterial;
        _taskText.SetText("ENTER TEXT HERE");

        _progressHolder.gameObject.SetActive(false);
        _numberImage.enabled = false;
        
        _startButton
            .OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.25f))
            .Subscribe(_ => StartSequence())
            .AddTo(this);

       Tween.Scale(
            _startButton.transform,
            endValue: Vector3.one * 1.15f,
            duration: 0.33f,
            ease: Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo
        );
    }

    private void OnEnable()
    {
        _startButton.gameObject.SetActive(true);
        _current?.gameObject.SetActive(false);
        _canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (_current == null || !_current.IsRunning)
        {
            _progressHolder.gameObject.SetActive(false);
            return;
        }

        _progressHolder.gameObject.SetActive(true);
        _timeLeft -= Time.deltaTime;
        
        _progressBarMaterial.SetFloat(FillAmountId, Mathf.Clamp01(_timeLeft / _current.StageTime));

        if (_timeLeft <= 0)
        {
            _current.FailGame();
        }
    }
    
    private void RollStage()
    {
        _current?.gameObject.SetActive(false);

        List<CaptchaGame> available;

        if (_current != null && _games.Count > 1)
        {
            available = _games.Where(game => game != _current).ToList();
        }
        else
        {
            available = _games;
        }
        
        var index = Random.Range(0, available.Count);
        
        _current = available[index];
        _current.gameObject.SetActive(true);
        SubscribeToStage(_current);

        _timeLeft = _current.StageTime;
        _taskText.SetText(_current.TaskText);
        _current.StartGame();
    }

    private void SubscribeToStage(CaptchaGame game)
    {
        _gameSubs.Clear();
        
        game.OnGameWin.Subscribe(_ => WinGame()).AddTo(_gameSubs);
        game.OnGameFail.Subscribe(_ => FailGame()).AddTo(_gameSubs);
        
        game.OnStageWin.Subscribe(_ => WinStage()).AddTo(_gameSubs);
        game.OnStageFail.Subscribe(_ => FailStage()).AddTo(_gameSubs);
        
        game.CurrentPoints.Subscribe(SetupCurrentPointsText).AddTo(_gameSubs);
    }

    private void SetupCurrentPointsText(int value) => _pointsText.SetText("{0}/{1}", value, _current.PointsGoal);
    
    private void StartSequence()
    {
        _startButton.gameObject.SetActive(false);
        _taskText.SetText("ENTER TEXT HERE");
        
        _numberImage.transform.localScale = StartScale;
        _numberImage.enabled = true;

        _numberImage.sprite = _numberSprites[2];
        
        Sequence.Create()
            .ChainCallback(this, static window => window._audioSource.PlayOneShot(window._numberClip))
            .Chain(Tween.Scale(_numberImage.transform, 0.8f, 1f))
            .ChainCallback(this, static window => window._numberImage.sprite = window._numberSprites[1])
            .ChainCallback(this, static window => window._numberImage.transform.localScale = StartScale)
            .ChainCallback(this, static window => window._audioSource.PlayOneShot(window._numberClip))
            .Chain(Tween.Scale(_numberImage.transform, 0.8f, 1f))
            .ChainCallback(this, static window => window._numberImage.sprite = window._numberSprites[0])
            .ChainCallback(this, static window => window._numberImage.transform.localScale = StartScale)
            .ChainCallback(this, static window => window._audioSource.PlayOneShot(window._numberClip))
            .Chain(Tween.Scale(_numberImage.transform, 0.8f, 1f))
            .ChainCallback(this, static window => window.RollStage())
            .ChainCallback(this, static window => window._numberImage.enabled = false);
    }
    
    private void WinGame()
    {
        _audioSource.PlayOneShot(_winClip);
        Tween.Delay(this, _winClip.length + 0.015f, static window =>
        {
            
            window.gameObject.SetActive(false);
        });

        Sequence.Create()
            .ChainDelay(2.5f)
            .Chain(Tween.Alpha(_canvasGroup, 0f, 0.33f))
            .ChainDelay(2.17f)
            .ChainCallback(this, static window => 
            {
                GameStateService.G?.OnEventEnded.OnNext(Unit.Default);
                window.gameObject.SetActive(false);
            });
    }
    
    private void FailGame()
    {
        _audioSource.PlayOneShot(_loseClip);
        Sequence.Create()
            .ChainDelay(_loseClip.length + 0.015f)
            .ChainCallback(this, static window => window._startButton.gameObject.SetActive(true))
            .ChainCallback(this, static window => window._current.gameObject.SetActive(false));
    }

    private void WinStage() => _audioSource.PlayOneShot(_winClip);
    private void FailStage() => _audioSource.PlayOneShot(_loseClip);
    
    private void OnDestroy()
    {
        _gameSubs.Dispose();
        Destroy(_progressBarMaterial);
    }
}