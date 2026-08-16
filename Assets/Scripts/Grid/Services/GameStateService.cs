using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;
using Unit = R3.Unit;

namespace Grid.Services
{
    public class GameStateService : SerializedMonoBehaviour
    {
        public static GameStateService G;

        [OdinSerialize] private List<GameObject> Events;
        
        [SerializeField] private float timeSubFromIconBinned = 2f;
        
        [HideInInspector] public Subject<IconView> OnIconBinned = new();
        [HideInInspector] public Subject<Unit> OnEventEnded = new();
        [HideInInspector] public Subject<Unit> OnGameEnded = new();
        
        private Dictionary<IconType, int> iconsCountByType = new();
        public Dictionary<IconType, int> IconsCountByType => iconsCountByType;
        private List<IconView> totalIconList = new();
        public List<IconView> TotalIconList => totalIconList;
        
        private ReactiveProperty<int> totalIcons = new(0);
        private ReactiveProperty<int> binnedIcons = new(0);
        public ReadOnlyReactiveProperty<int> BinnedIcons => binnedIcons;
        public ReadOnlyReactiveProperty<int> TotalIcons => totalIcons;

        private bool isEventActive = false;
        private bool isGameWon = false;
        [SerializeField] private float timeTillNextEvent = 8;
        [SerializeField] private float minTimeTillNextEvent = 6;
        private float currentTimeTillNextEvent = 30;

        private ReactiveProperty<float> totalTimeTimer = new(0f);
        public ReadOnlyReactiveProperty<float> TotalTime => totalTimeTimer;

        private GameObject _currentEvent; 

        public void Awake()
        {
            if (G == null)
            {
                G = this;
                isGameWon = false;
                isEventActive = false;
                totalIcons = new(0);
                binnedIcons = new(0);
                totalTimeTimer = new(0f);
                OnIconBinned.Subscribe(view => IconBinned(view)).AddTo(this);
                OnEventEnded.Subscribe(_ => FlipEvent()).AddTo(this);
                OnGameEnded.Subscribe(_ => GameWon()).AddTo(this);
                return;
            }
            Destroy(this);
        }

        private void OnDestroy()
        {
            if (G == this)
                G = null;
        }

        private void FlipEvent()
        {
            GameObject.Destroy(_currentEvent.gameObject);
            isEventActive = false;
        }
        
        private void GameWon() => isGameWon = true;

        public void IconBinned(IconView view)
        {
            binnedIcons.Value++;
            
            iconsCountByType[view.IconType]--;
            totalIconList.Remove(view);

            currentTimeTillNextEvent -= timeSubFromIconBinned;
            
            CheckIfGameEnd();
        }

        private void CheckIfGameEnd()
        {
            if (binnedIcons.Value == totalIcons.Value)
            {
                OnGameEnded.OnNext(Unit.Default);
            }
        }

        public void Initialize(List<IconGrid> totalGridList)
        {
            foreach (var grid in totalGridList)
            {
                totalIcons.Value += grid.IconCount;

                foreach (var icon in grid.IconOccupations.Values)
                {
                    if(!iconsCountByType.TryAdd(icon.IconType, 1)) iconsCountByType[icon.IconType]++;

                    totalIconList.Add(icon);
                }
            }

            totalIcons.Value -= 1; // Eliminate Bin;
            Debug.LogWarning($"{totalIcons} total icons were loaded");
            
            currentTimeTillNextEvent = timeTillNextEvent;
            
            GameLoop().Forget();
            TimeLoop().Forget();
        }

        private async UniTaskVoid GameLoop()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            while (!ct.IsCancellationRequested)
            {
                if(!GameSequence.Instance.IsRunning) return;
                
                Debug.Log($"{currentTimeTillNextEvent}");
                currentTimeTillNextEvent -= Time.deltaTime;

                if (isGameWon) return;
                
                if (isEventActive && currentTimeTillNextEvent <= minTimeTillNextEvent)
                {
                    currentTimeTillNextEvent = minTimeTillNextEvent;
                    await UniTask.WaitUntil(() => !isEventActive, cancellationToken: ct);
                }
                
                if (currentTimeTillNextEvent <= 0f && !isEventActive)
                {
                    currentTimeTillNextEvent = timeTillNextEvent;
                    isEventActive = true;
                    SelectEvent();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);
            }
        }

        private async UniTaskVoid TimeLoop()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            while (!ct.IsCancellationRequested)
            {
                if (!GameSequence.Instance.IsRunning) return;
                
                if (isGameWon) return;
                totalTimeTimer.Value += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);
            }
        }
        
        private void SelectEvent()
        {
            GameObject selectedEvent = Events[Random.Range(0, Events.Count)];
            _currentEvent = Instantiate(selectedEvent, GridService.G?.RootCanvasTransform);
            _currentEvent.SetActive(true);
        }
        
    }

    public enum IconType
    {
        Default,
        Image,
        Application,
        Document,
        Folder
    }
}
