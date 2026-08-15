using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Grid.IconData;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grid
{
    public class GridService : SerializedMonoBehaviour
    {
        public static GridService G;

        [OdinSerialize] private Dictionary<int, IconView> iconPlacement;

        [SerializeField] private RectTransform upperLeftBoundryForFolders;
        [SerializeField] private RectTransform lowerRightBoundryForFolders;
        [SerializeField] private RectTransform rootCanvasTransform;
        [SerializeField] private Camera cam;
        [SerializeField] private RandomIconProvider randomIconProvider;
        
        private List<IconGrid> _totalGridList = new();
        private List<IconGrid> _activeGridList = new();
        public List<IconGrid> ActiveGrids => _activeGridList;
        public List<IconGrid> TotalGridList => _totalGridList;
        public Camera Camera => cam;
        public RectTransform  RootCanvasTransform => rootCanvasTransform;
        public RandomIconProvider RandomIconProvider => randomIconProvider;
        
        [HideInInspector] public IconMouseManager MouseManager { get; private set; }

        [HideInInspector] public Subject<Unit> OnFolderWindowOpened = new();
        [HideInInspector] public Subject<Unit> OnFolderWindowClosed = new();
        
        [HideInInspector] public IconView lastClickedIcon;
        [HideInInspector] public IconView lastDraggedItem;
        [HideInInspector] public GameObject lastIconSlot;

        private void Awake()
        {
            if (G == null)
            {
                G = this;
                MouseManager = new IconMouseManager();
                return;
            }
            Destroy(gameObject);
        }

        private async UniTaskVoid Start()
        {
            await UniTask.DelayFrame(2);
            await UniTask.WaitUntil(() => randomIconProvider.isInitialized == true,
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        public void RegisterGrid(IconGrid grid)
        {
            _totalGridList.Add(grid);
        }

        public void AddToActiveGrid(IconGrid grid)
        {
            if (_activeGridList.Contains(grid)) return;
            _activeGridList.Add(grid);
        }
        
        public void RemoveFromActiveGrid(IconGrid grid)
        {
            _activeGridList.Remove(grid);
        }

        public IconGrid SearchForActiveGridWithSlot(GameObject slot)
        {
            foreach (var grid in _activeGridList)
            {
                if (grid.AllIconSlots.Contains(slot))
                {
                    return grid;
                }
            }

            return null;
        }

        public Vector3 GetSpawnPositionForFolderWindow()
        {
            if (upperLeftBoundryForFolders == null || lowerRightBoundryForFolders == null)
            {
                Debug.LogError("Boundaries are not assigned!");
                return Vector2.zero;
            }

            Vector2 posA = upperLeftBoundryForFolders.anchoredPosition;
            Vector2 posB = lowerRightBoundryForFolders.anchoredPosition;

            float minX = Mathf.Min(posA.x, posB.x);
            float maxX = Mathf.Max(posA.x, posB.x);
            float minY = Mathf.Min(posA.y, posB.y);
            float maxY = Mathf.Max(posA.y, posB.y);

            return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
        
        public (IconGrid targetGrid, GameObject closestSlot) GetClosestSlotAtPosition
        (Vector2 screenPosition, float maxSnapDistance = 150f)
        {
            IconGrid targetGrid = null;
            for (int i = _activeGridList.Count - 1; i >= 0; i--)
            {
                var grid = _activeGridList[i];
                if (grid == null || !grid.gameObject.activeInHierarchy) continue;

                RectTransform gridRect = grid.GetComponent<RectTransform>();
                if (gridRect != null && RectTransformUtility.RectangleContainsScreenPoint(gridRect, screenPosition, cam))
                {
                    targetGrid = grid;
                    break; // Found the top-most window
                }
            }
            
            // Fallback
            if (targetGrid == null && _activeGridList.Count > 0)
            {
                targetGrid = _activeGridList[0];
            }

            if (targetGrid == null) return (null, null);

            // 2. Find the closest slot in the selected target grid
            GameObject closestSlot = null;
            float closestDistSqr = float.MaxValue;
            float maxDistSqr = maxSnapDistance * maxSnapDistance;

            foreach (var slot in targetGrid.AllIconSlots)
            {
                if (slot == null || !slot.activeInHierarchy) continue;

                RectTransform slotRect = slot.GetComponent<RectTransform>();
                Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(cam, slotRect.position);

                float distSqr = (slotScreenPos - screenPosition).sqrMagnitude;
                if (distSqr < closestDistSqr && distSqr <= maxDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestSlot = slot;
                }
            }

            return (targetGrid, closestSlot);
        }
    }
}