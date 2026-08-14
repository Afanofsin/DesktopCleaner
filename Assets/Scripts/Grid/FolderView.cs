using UnityEngine;

namespace Grid
{
    public class FolderView : IconView
    {
        [SerializeField] private IconGrid folderPrefab;
        
        public override void Setup()
        {
            base.Setup();
            folderPrefab.Initialize();
        }
    }
}