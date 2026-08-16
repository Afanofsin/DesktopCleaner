using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.Serialization;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaptchaTextGame : CaptchaGame
{
    [OdinSerialize] private Dictionary<string, Sprite> _texts;
    [OdinSerialize] private Image _captchaImage;
    [OdinSerialize] private TMP_InputField _captchaInput;

    private int _lastIndex = -1;
    private string _selectedString;
    
    private void Awake()
    {
        PointsGoal = 2;

        _captchaInput.OnEndEditAsObservable().Subscribe(ValidateInput);
    }

    public override void StartGame()
    {
        base.StartGame();
        StartLevel();
        
        _captchaInput.gameObject.SetActive(true);
    }

    private void ValidateInput(string text)
    {
        if (text.IsNullOrWhitespace() || text.Length <= 4)
        {
            return;
        }
        
        _captchaInput.text = string.Empty;
        
        text = text.ToLowerInvariant();
        
        if (text != _selectedString)
        {
            OnStageFailRx.OnNext(Unit.Default);
            StartLevel();
        }
        else
        {
            OnStageWinRx.OnNext(Unit.Default);
            
            if (CurrentPointsRx.Value++ >= PointsGoal - 1)
            {
                WinGame();
            }
            else
            {
                StartLevel();
            }
        }
    }

    public override void WinGame()
    {
        base.WinGame();
        _captchaInput.gameObject.SetActive(false);
    }

    private void StartLevel()
    {
        int index;

        do
        {
            index = Random.Range(0, _texts.Count);
        }
        while (_texts.Count > 1 && index == _lastIndex);

        _lastIndex = index;
        
        var kvp = _texts.ElementAt(index);
        
        _selectedString = kvp.Key;
        _captchaImage.sprite = kvp.Value;
    }
}