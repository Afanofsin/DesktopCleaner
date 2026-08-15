using System;
using UnityEngine;
using R3;

namespace Grid
{
    public class FolderView : IconView
    {
        [SerializeField] private IconGrid folderPrefab;
        [SerializeField] private MainWindow folderWindow;
        
        public bool isFolderActive => folderPrefab.gameObject.activeSelf;
        public bool isEmpty => folderPrefab.IconOccupations.Count == 0;
        
        public override void Setup()
        {
            base.Setup();
            folderPrefab.Initialize();
            
            GridService.G?.OnFolderWindowOpened.Subscribe(_ => ProhibitDragging()).AddTo(this);
            GridService.G?.OnFolderWindowClosed.Subscribe(_ => AllowDragging()).AddTo(this);
        }

        public GameObject PutInFolder(IconView view)
        {
            return folderPrefab.AddIcon(view);
        }

        public override void OnDoubleClick()
        {
            base.OnDoubleClick();

            //if (!isFolderActive)
            if(!folderWindow.IsVisible)
            {
                ApplyRandomWorldPosition();
                folderWindow.Show();
                GridService.G?.OnFolderWindowOpened.OnNext(Unit.Default);
            }
            else
            {
                folderWindow.Hide();
                //folderPrefab.gameObject.SetActive(false);
                GridService.G?.OnFolderWindowClosed.OnNext(Unit.Default);
            }
            
        }

        private void ProhibitDragging()
        {
            isDraggingAllowed = false;
        }

        private void AllowDragging()
        {
            isDraggingAllowed = true;
        } 

        private void ApplyRandomWorldPosition()
        {
            RectTransform folderRect = folderPrefab.GetComponent<RectTransform>();
            RectTransform rootCanvas = GridService.G.RootCanvasTransform;
            
            if (folderRect.parent != rootCanvas)
            {
                folderRect.SetParent(rootCanvas, false);
            }
            
            Vector2 randomCanvasPos = GridService.G.GetSpawnPositionForFolderWindow();
            folderRect.anchoredPosition = randomCanvasPos;
            folderPrefab.gameObject.SetActive(true);
        }
    }
}