using System;
using System.Collections.Generic;
using System.Linq;
using Grid;
using Grid.Services;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Windows
{
    public class PatternMatchingGame : MonoBehaviour
    {
        [SerializeField] private MainWindow window;
        [SerializeField] private GameObject deskTopPatternMatch;
        [SerializeField] private GameObject folderGame;

        private List<IconGrid> totalGridList = new();
        private List<IconView> totalIcons = new();
        
        private void Awake()
        {
            window.GetComponentInChildren<MainWindow>();
        }

        public void SelectTask()
        {
            totalGridList.Clear();
            totalIcons.Clear();
            totalGridList = GridService.G?.TotalGridList;
            totalIcons = GameStateService.G?.TotalIconList;
            
            int coinFlip = Random.Range(0, 2);
            if(coinFlip == 0)
            {
                SelectPatternForDesktop();
            }
            else
            {
                SelectFolderTask();
            }
        }

        // 18 * 7
        private void SelectPatternForDesktop()
        {
            var grid = totalGridList[0];
            
            IconType typeToMatch = (IconType)Random.Range(0, 5);
            int countTotalTypes = totalIcons.Count(x => x.IconType == typeToMatch);
            int rand = Random.Range(0, 2);
            switch (rand)
            {
                case 0:
                    if (countTotalTypes < 5) goto case 1;
                    StarPattern();
                    break;
                case 1:
                    if (countTotalTypes < 3) break;
                    LinePattern();
                    break;
            }
                
        }

        private void SelectFolderTask()
        {
            
        }

        private void StarPattern()
        {
            
        }

        private void LinePattern()
        {
            
        }
        
        
    }
}