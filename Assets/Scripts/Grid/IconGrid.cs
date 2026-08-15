using System.Collections.Generic;
using Grid;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class IconGrid : SerializedMonoBehaviour
{
    [SerializeField] private GameObject iconSlotPrefab;
    [SerializeField] private RectTransform canvasRoot;
    [SerializeField] private int slotsAmount;
    [SerializeField] private Transform rootForIcons;
    
    [OdinSerialize] private Dictionary<int, IconView> iconPlacement;
    [SerializeField] private bool shouldBeRandomlyPopulated = true;

    [SerializeField] [PropertyRange(0, "slotsAmount")]
    private int slotsToPopulate;

    public Transform CanvasRoot => canvasRoot;
    public int IconCount => IconOccupations.Count;
    
    public HashSet<GameObject> AllIconSlots = new();
    private Dictionary<int, GameObject> _iconSlotsByID = new();
    public Dictionary<int, GameObject> IconSlotsByID => _iconSlotsByID;
    private Dictionary<GameObject, IconView> _iconOccupations = new();
    public Dictionary<GameObject, IconView> IconOccupations => _iconOccupations;
    
    private bool isInit = false;

    private void OnEnable()
    {
        GridService.G?.AddToActiveGrid(this);
        if (canvasRoot == null)
        {
            canvasRoot = GridService.G?.RootCanvasTransform;
        }
    }

    private void OnDisable()
    {
        GridService.G?.RemoveFromActiveGrid(this);
    }

    public void Initialize()
    {
        Debug.Log("Init");
        GridService.G?.RegisterGrid(this);
        GridService.G?.AddToActiveGrid(this);
        if (rootForIcons == null) rootForIcons = transform;
        int id = 0;
        for (int i = 0; i < slotsAmount; i++)
        {
            var obj = Instantiate(iconSlotPrefab, rootForIcons);
            _iconSlotsByID.Add(id, obj);
            AllIconSlots.Add(obj);
            
            id++;
        }

        foreach (var kvp in iconPlacement)
        {
            SpawnIconAtSlot(kvp.Key,kvp.Value);
        }

        if (shouldBeRandomlyPopulated)
        {
            int slotN = 0;
            for (int i = 0; i < slotsToPopulate; i++)
            {
                for (; slotN < slotsAmount; slotN++)
                {
                    IconView view = GridService.G?.RandomIconProvider.GetIcon(this, true);
                    SpawnIconAtSlot(slotN, view);
                }
            }
        }

        isInit = true;
    }

    void Start()
    {
        //Debug.Log("Awakening");
        if (isInit) return;
        Initialize();
    }

    public GameObject AddIcon(IconView view)
    {
        var unoccupiedSlots = new Dictionary<int, GameObject>();
        int lowestFreeSlot = int.MaxValue;
        
        foreach (var (id, slotObj) in _iconSlotsByID)
        {
            if(!_iconOccupations.TryGetValue(slotObj, out var icon) || icon == null)
            {
                if (id < lowestFreeSlot) lowestFreeSlot = id;
                unoccupiedSlots.Add(id, slotObj);
            }
        }

        if (unoccupiedSlots.Count == 0) return null;

        var obj = _iconSlotsByID[lowestFreeSlot];
        _iconOccupations[obj] = view;
        return obj;
    }

    public void RemoveIcon(GameObject lastIconSlot)
    {
        _iconOccupations.Remove(lastIconSlot);
    }

    public void SpawnIconAtSlot(int slot, IconView view)
    {
        var slotObj = _iconSlotsByID[slot];

        _iconOccupations.TryGetValue(slotObj, out var dictView);
        if (dictView != null) return;
        
        var obj = Instantiate(view, slotObj.transform);
        var iconView = obj.GetComponent<IconView>();
        
        iconView.Setup();
        GridService.G.MouseManager.SubscribeButton(iconView);
        
        _iconOccupations[slotObj] = iconView;
    }
    
    public bool TryMoveIcon(IconView movedView, GameObject droppedOn, GameObject lastIconSlot, out IconView swappedView)
    {
        swappedView = null;
        bool found = false;
        foreach (var kvp in _iconSlotsByID)
        {
            if (kvp.Value == droppedOn)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            var grid = GridService.G.SearchForActiveGridWithSlot(droppedOn);
            if (grid == null) return false;

            grid.IconOccupations.TryGetValue(droppedOn, out swappedView);
            if (swappedView == null)
            {
                MoveIconOnAnotherGrid(grid, movedView, droppedOn, lastIconSlot);
            }
            else
            {
                SwapIconsBetweenGrids(grid, movedView, swappedView, lastIconSlot, droppedOn);
            }
        }
        else
        {
            _iconOccupations.TryGetValue(droppedOn, out swappedView);
            if (swappedView == null)
            {
                MoveIcon(movedView, droppedOn, lastIconSlot);
            }
            else
            {
                SwapIcons(lastIconSlot, droppedOn );
            }
        }
        
        return true;
    }

    private void MoveIcon(IconView movedView, GameObject droppedOn, GameObject lastIconSlot)
    {
        _iconOccupations[droppedOn] = movedView;
        _iconOccupations.Remove(lastIconSlot);
    }

    private void MoveIconOnAnotherGrid(IconGrid grid, IconView movedView, GameObject droppedOn, GameObject lastIconSlot)
    {
        grid.IconOccupations[droppedOn] = movedView;
        _iconOccupations.Remove(lastIconSlot);
    }

    private void SwapIcons(GameObject firstSlot, GameObject secondSlot)
    {
        (_iconOccupations[firstSlot], _iconOccupations[secondSlot]) = (_iconOccupations[secondSlot], _iconOccupations[firstSlot]);
    }
    
    private void SwapIconsBetweenGrids(IconGrid grid, IconView movedView, IconView swappedView, GameObject firstSlot, GameObject secondSlot)
    {
        this._iconOccupations[firstSlot] = swappedView;
        grid.IconOccupations[secondSlot] = movedView;
    }
}
