using System;
using System.Collections.Generic;
using System.Linq;
using Grid.IconData;
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
        [SerializeField] private Camera camera;
        
        private List<IconGrid> _totalGridList = new();
        private List<IconGrid> _activeGridList = new();
        public List<IconGrid> ActiveGrids => _activeGridList;
        public List<IconGrid> TotalGridList => _totalGridList;
        public Camera Camera => camera;
        public RectTransform  RootCanvasTransform => rootCanvasTransform;
        
        public IconMouseManager MouseManager { get; private set; }
        
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

        private void Start()
        {
            var grid = _activeGridList.FirstOrDefault();
            if (grid == null) return;

            foreach (var kvp in iconPlacement)
            {
                grid.SpawnIconAtSlot(kvp.Key, kvp.Value);
            }
        }

        public void RegisterGrid(IconGrid grid)
        {
            _totalGridList.Add(grid);
        }

        public void AddToActiveGrid(IconGrid grid)
        {
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
    }
}