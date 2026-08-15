using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class WallpaperClicker : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            GridService.G?.MouseManager.CleanLastIcon();
        }
    }
}