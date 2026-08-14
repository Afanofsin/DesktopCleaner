using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class GridService : MonoBehaviour
    {
        public static GridService G;

        private List<IconGrid> _totalGridList = new();
        private List<IconGrid> _activeGridList = new();
        public List<IconGrid> ActiveGrids => _activeGridList;
        public List<IconGrid> TotalGridList => _totalGridList;
        
        public IconMouseManager MouseManager { get; private set; }
        
        public IconView lastClickedIcon;
        public IconView lastDraggedItem;
        public GameObject lastIconSlot;

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
    }
}