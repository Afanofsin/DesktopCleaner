using System;
using System.Collections.Generic;
using Grid;
using R3.Triggers;
using UnityEngine;

public class IconGrid : MonoBehaviour
{
    [SerializeField] private GameObject iconSlotPrefab;
    [SerializeField] private IconView testIconPrefab;
    [SerializeField] private RectTransform canvasRoot;
    [SerializeField] private int slotsAmount;

    [SerializeField] private int slotToSpawn = 0;

    public Transform CanvasRoot => canvasRoot;

    public HashSet<GameObject> AllIconSlots = new();
    
    private Dictionary<int, GameObject> _iconSlotsByID = new();
    public Dictionary<int, GameObject> IconSlotsByID => _iconSlotsByID;
    private Dictionary<GameObject, IconView> _iconOccupations = new();
    public Dictionary<GameObject, IconView> IconOccupations => _iconOccupations;

    //private IconMouseManager _iconMouseManager;

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
        GridService.G?.RegisterGrid(this);
    }

    void Awake()
    {
        //_iconMouseManager = new IconMouseManager(this);
        
        int id = 0;
        for (int i = 0; i < slotsAmount; i++)
        {
            var obj = Instantiate(iconSlotPrefab, transform);
            _iconSlotsByID.Add(id, obj);
            AllIconSlots.Add(obj);
            
            id++;
        }
        
        // SpawnIconAtSlot();
        // slotToSpawn = 1;
        // SpawnIconAtSlot();
        // slotToSpawn = 2;
        // SpawnIconAtSlot();
        // slotToSpawn = 8;
        // SpawnIconAtSlot();
    }
    
    
    [ContextMenu("Spawn Icon")]
    public void SpawnIconAtSlot()
    {
        SpawnIconAtSlot(slotToSpawn, testIconPrefab);
    }

    public void SpawnIconAtSlot(int slot, IconView view)
    {
        var slotObj = _iconSlotsByID[slot];
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
        _iconOccupations[lastIconSlot] = null;
    }

    private void MoveIconOnAnotherGrid(IconGrid grid, IconView movedView, GameObject droppedOn, GameObject lastIconSlot)
    {
        grid.IconOccupations[droppedOn] = movedView;
        _iconOccupations[lastIconSlot] = null;

        // grid.AttachIconToManager(movedView);
    }

    private void SwapIcons(GameObject firstSlot, GameObject secondSlot)
    {
        (_iconOccupations[firstSlot], _iconOccupations[secondSlot]) = (_iconOccupations[secondSlot], _iconOccupations[firstSlot]);
    }
    
    private void SwapIconsBetweenGrids(IconGrid grid, IconView movedView, IconView swappedView, GameObject firstSlot, GameObject secondSlot)
    {
        this._iconOccupations[firstSlot] = swappedView;
        grid.IconOccupations[secondSlot] = movedView;
        
        // this.AttachIconToManager(swappedView);
        // grid.AttachIconToManager(movedView);

    }

    // public void AttachIconToManager(IconView movedView)
    // {
    //     _iconMouseManager.SubscribeButton(movedView);
    // }
}
