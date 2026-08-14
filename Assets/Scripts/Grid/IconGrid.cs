using System.Collections.Generic;
using Grid;
using R3.Triggers;
using UnityEngine;

public class IconGrid : MonoBehaviour
{
    [SerializeField] private GameObject iconSlotPrefab;
    [SerializeField] private IconView testIconPrefab;
    [SerializeField] private Transform canvasRoot;
    [SerializeField] private int slotsAmount;

    [SerializeField] private int slotToSpawn = 0;

    public Transform CanvasRoot => canvasRoot;
    
    private Dictionary<int, GameObject> _iconSlots = new();
    public Dictionary<int, GameObject> IconSlots => _iconSlots;
    private Dictionary<GameObject, IconView> _iconOccupations = new();
    public Dictionary<GameObject, IconView> IconOccupations => _iconOccupations;

    private IconMouseManager _iconMouseManager;

    void Awake()
    {
        _iconMouseManager = new IconMouseManager(this);
        
        int id = 0;
        for (int i = 0; i < slotsAmount; i++)
        {
            var obj = Instantiate(iconSlotPrefab, transform);
            _iconSlots.Add(id, obj);
            
            id++;
        }
        
        SpawnIconAtSlot();
        slotToSpawn = 1;
        SpawnIconAtSlot();
        slotToSpawn = 2;
        SpawnIconAtSlot();
        slotToSpawn = 8;
        SpawnIconAtSlot();
    }
    
    
    [ContextMenu("Spawn Icon")]
    public void SpawnIconAtSlot()
    {
        var slotObj = _iconSlots[slotToSpawn];
        var obj = Instantiate(testIconPrefab, slotObj.transform);
        var iconView = obj.GetComponent<IconView>();
        
        iconView.Setup();
        _iconMouseManager.SubscribeButton(iconView);
        
        _iconOccupations[slotObj] = iconView;
    }

    public bool TryMoveIcon(IconView movedView, GameObject droppedOn, GameObject lastIconSlot)
    {
        bool found = false;
        foreach (var kvp in _iconSlots)
        {
            if (kvp.Value == droppedOn)
            {
                found = true;
                break;
            }
        }
        if(!found) return false;

        _iconOccupations.TryGetValue(droppedOn, out IconView dictView);

        if (dictView == null)
        {
            _iconOccupations[droppedOn] = movedView;
            _iconOccupations[lastIconSlot] = null;
        }
        else
        {
            SwapIcons(movedView, dictView, lastIconSlot,  droppedOn);
        }
        
        return true;
    }

    public void SwapIcons(IconView firstView, IconView secondView, GameObject firstSlot, GameObject secondSlot)
    {
        (_iconOccupations[firstSlot], _iconOccupations[secondSlot]) = (_iconOccupations[secondSlot], _iconOccupations[firstSlot]);
    }
}
