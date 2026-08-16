using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grid
{
    public class RandomIconProvider : SerializedMonoBehaviour
    {
        [SerializeField] private string pathToIcons;
        [SerializeField] private string pathToFolders;
        [SerializeField] private string pathToSpecial;

        [ShowInInspector] private List<GameObject> icons = new();
        [ShowInInspector] private List<GameObject> folders = new();
        [ShowInInspector] private List<GameObject> special= new() ;
        
        public Dictionary<IconGrid, HashSet<GameObject>> UniqueIconsProvided = new();
        
        public bool isInitialized = false;
        
        private void Awake()
        {
            icons = Resources.LoadAll<GameObject>(pathToIcons).ToList();
            folders = Resources.LoadAll<GameObject>(pathToFolders).ToList();
            special = Resources.LoadAll<GameObject>(pathToSpecial).ToList();

            isInitialized = true;
        }

        public IconView GetIcon(IconGrid grid, bool isUnique = false)
        {
            List<GameObject> availableIcons = new List<GameObject>(icons);

            if (availableIcons.Count == 0) return null;
            
            if (isUnique && grid != null)
            {
                if (!UniqueIconsProvided.TryGetValue(grid, out var usedIcons))
                {
                    usedIcons = new HashSet<GameObject>();
                    UniqueIconsProvided[grid] = usedIcons;
                }
                
                availableIcons = icons.Where(x => !usedIcons.Contains(x)).ToList();

                if (availableIcons.Count == 0)
                {
                    Debug.LogWarning("Ran out of unique Icons");
                    availableIcons = new List<GameObject>(icons);
                    if (availableIcons.Count == 0) return null;
                }
            }
            
            GameObject view = availableIcons[Random.Range(0, availableIcons.Count)];

            if (grid != null)
            {
                UniqueIconsProvided.TryAdd(grid, new HashSet<GameObject>());
                UniqueIconsProvided[grid].Add(view);
            }
            return view.GetComponent<IconView>();
        }
        
        public IconView GetFolder(IconGrid grid, bool isUnique = false)
        {
            List<GameObject> availableFolders = new List<GameObject>(folders);

            if (availableFolders.Count == 0) return null;
            
            if (isUnique && grid != null)
            {
                if (!UniqueIconsProvided.TryGetValue(grid, out var usedFolders))
                {
                    usedFolders = new HashSet<GameObject>();
                    UniqueIconsProvided[grid] = usedFolders;
                }
                
                availableFolders = icons.Where(x => !usedFolders.Contains(x)).ToList();

                if (availableFolders.Count == 0)
                {
                    Debug.LogWarning("Ran out of unique Folders");
                    availableFolders = new List<GameObject>(folders);
                    if (availableFolders.Count == 0) return null;
                }
            }
            
            GameObject view = availableFolders[Random.Range(0, availableFolders.Count)];

            if (grid != null)
            {
                UniqueIconsProvided[grid].Add(view);
            }
            return view.GetComponent<IconView>();
        }
        
        public IconView GetSpecial(IconGrid grid, bool isUnique = false)
        {
            List<GameObject> availableSpecial = new List<GameObject>(special);

            if (availableSpecial.Count == 0) return null;
            
            if (isUnique && grid != null)
            {
                if (!UniqueIconsProvided.TryGetValue(grid, out var usedSpecial))
                {
                    usedSpecial = new HashSet<GameObject>();
                    UniqueIconsProvided[grid] = usedSpecial;
                }
                
                availableSpecial = icons.Where(x => !usedSpecial.Contains(x)).ToList();

                if (availableSpecial.Count == 0)
                {
                    Debug.LogWarning("Ran out of unique Icons");
                    availableSpecial = new List<GameObject>(special);
                    if (availableSpecial.Count == 0) return null;
                }
            }
            
            GameObject view = availableSpecial[Random.Range(0, availableSpecial.Count)];

            if (grid != null)
            {
                UniqueIconsProvided[grid].Add(view);
            }
            return view.GetComponent<IconView>();
        }
    }
}