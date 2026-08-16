using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Grid;
using Grid.Services;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Windows
{
    public class ErrorWindow : MonoBehaviour
    {
        [SerializeField] GameObject ErrorWindowPrefab;

        private int minError = 5;
        private int maxError = 11;

        private List<MainWindow> activeErrorWindows = new();
        private List<UniTask> tasks = new();

        private void Start()
        {
            activeErrorWindows?.Clear();
            
            int objToSpawn = Random.Range(minError, maxError);
            
            var ct = this.GetCancellationTokenOnDestroy();

            for (int i = 0; i < objToSpawn; i++)
            {
                var obj = Instantiate(ErrorWindowPrefab, this.transform);
                var mainWindow = obj.GetComponent<MainWindow>();
                ApplyRandomWorldPosition(obj);
                tasks.Add(WaitForObjInactive(obj, ct));
            }
            WaitForAll(ct).Forget();
        }

        private async UniTask WaitForObjInactive(GameObject obj, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => 
                    obj == null || 
                    !obj.activeInHierarchy,
                cancellationToken: ct
            );
        }

        private async UniTask WaitForAll(CancellationToken ct)
        {
            try
            {
                await UniTask.WhenAll(tasks);
                if (!ct.IsCancellationRequested)
                {
                    GameStateService.G?.OnEventEnded.OnNext(Unit.Default);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        private void ApplyRandomWorldPosition(GameObject obj)
        {
            RectTransform objRect = obj.GetComponent<RectTransform>();
            RectTransform rootCanvas = GridService.G.RootCanvasTransform;
            RectTransform parentRect = (RectTransform)transform;
            
            if (objRect.parent != parentRect)
            {
                objRect.SetParent(parentRect, false);
            }
            
            Vector2 randomCanvasPos = GridService.G.GetSpawnPositionForFolderWindow();
            
            if (rootCanvas != null)
            {
                Vector3 worldSpawnPos = rootCanvas.TransformPoint(randomCanvasPos);
                objRect.position = worldSpawnPos;
            }
            else
            {
                objRect.anchoredPosition = randomCanvasPos;
            }
    
            obj.gameObject.SetActive(true);
        }
    }
}