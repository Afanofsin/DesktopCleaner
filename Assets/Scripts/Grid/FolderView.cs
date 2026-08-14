using UnityEngine;

namespace Grid
{
    public class FolderView : IconView
    {
        [SerializeField] private IconGrid folderPrefab;
        
        bool isFolderActive => folderPrefab.gameObject.activeSelf;
        
        public override void Setup()
        {
            base.Setup();
            folderPrefab.Initialize();
        }

        public override void OnDoubleClick()
        {
            base.OnDoubleClick();

            if (!isFolderActive)
            {
                ApplyRandomWorldPosition();
            }
            else
            {
                folderPrefab.gameObject.SetActive(false);
            }
            
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