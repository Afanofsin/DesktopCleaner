using System;
using Grid.Services;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class CaptchaGame : SerializedMonoBehaviour
{
    [OdinSerialize] public float StageTime { get; private set; }
    [OdinSerialize] public string TaskText { get; private set; }
    
    public bool IsRunning { get; private set; }
    
    protected readonly Subject<Unit> OnGameFailRx = new();
    public Observable<Unit> OnGameFail => OnGameFailRx;
    
    protected readonly Subject<Unit> OnStageFailRx = new();
    public Observable<Unit> OnStageFail => OnStageFailRx;
    
    protected readonly Subject<Unit> OnStageWinRx = new();
    public Observable<Unit> OnStageWin => OnStageWinRx;

    protected readonly Subject<Unit> OnGameWinRx = new();
    public Observable<Unit> OnGameWin => OnGameWinRx;
    
    protected readonly ReactiveProperty<int> CurrentPointsRx = new (0);
    public ReadOnlyReactiveProperty<int> CurrentPoints => CurrentPointsRx;
    
    public int PointsGoal { get; protected set; }
    
    public virtual void StartGame()
    {
        IsRunning = true;
        CurrentPointsRx.Value = 0;
    }

    public virtual void FailStage()
    {
        OnStageFailRx.OnNext(Unit.Default);
    }

    public virtual void FailGame()
    {
        IsRunning = false;
        OnGameFailRx.OnNext(Unit.Default);
    }

    public virtual void WinGame()
    {
        IsRunning = false;
        
        OnGameWinRx.OnNext(Unit.Default);
    }

    protected void OnDestroy()
    {
        OnGameFailRx.Dispose();
        OnStageFailRx.Dispose();
        OnStageWinRx.Dispose();
        OnGameWinRx.Dispose();
        CurrentPointsRx.Dispose();
    }
}
