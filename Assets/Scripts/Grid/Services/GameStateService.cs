using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Grid.Services
{
    public class GameStateService : MonoBehaviour
    {
        public static GameStateService G;
        
        [HideInInspector] public Subject<Unit> OnIconBinned = new();

        private int totalIcons = 0;
        private int binnedIcons = 0;
        public int BinnedIcons => binnedIcons;
        public int TotalIcons => totalIcons;

        public void Awake()
        {
            if (G == null)
            {
                G = this;
                OnIconBinned.Subscribe(_ => IconBinned()).AddTo(this);
                return;
            }
            Destroy(this);
        }

        private void OnDestroy()
        {
            if (G == this)
                G = null;
        }

        public void IconBinned()
        {
            binnedIcons++;
            CheckIfGameEnd();
        }

        private void CheckIfGameEnd()
        {
            if (binnedIcons == totalIcons)
            {
                Debug.Log("$$$$$$$$$$$$$$$WWWWWWWWWWWWWWWww$$$$$$$$$$$$$$$$$$$");
            }
        }

        public void InitializeTotalCount(List<IconGrid> totalGridList)
        {
            foreach (var grid in totalGridList)
            {
                totalIcons += grid.IconCount;
            }

            totalIcons -= 1; // Eliminate Bin;
            Debug.LogWarning($"{totalIcons} total icons were loaded");
        }
    }
}
