using System.Collections.Generic;
using R3;
using Sirenix.Serialization;
using UnityEngine;

public class CaptchaBingusGame : CaptchaGame
{
    [OdinSerialize] private List<Bingus> _binguses;

    private Bingus _current;
    private int _lastBingusIndex = -1;

    private void Awake()
    {
        PointsGoal = 2;
        
        foreach (var bingus in _binguses)
        {
            bingus.WinButton.OnClickAsObservable().Subscribe(_ => CheckWin()).AddTo(this);
            bingus.LoseButton.OnClickAsObservable().Subscribe(_ => CheckLose()).AddTo(this);
            
            bingus.gameObject.SetActive(false);
        }
    }

    public override void StartGame()
    {
        base.StartGame();
        StartLevel();
    }

    private void CheckWin()
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

    private void CheckLose()
    {
        OnStageFailRx.OnNext(Unit.Default);
        StartLevel();
    }
    
    private void StartLevel()
    {
        _current?.gameObject.SetActive(false);
        
        int index;

        do
        {
            index = Random.Range(0, _binguses.Count);
        }
        while (_binguses.Count > 1 && index == _lastBingusIndex);

        _lastBingusIndex = index;
        
        _current = _binguses[index];
        _current.gameObject.SetActive(true);
    }
}