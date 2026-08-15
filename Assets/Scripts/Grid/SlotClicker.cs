using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Grid
{
    public class SlotClicker : MonoBehaviour
    {
        [SerializeField] private Button _button;
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.OnClickAsObservable().Subscribe(_ => OnButtonClick()).AddTo(this);
        }
        
        public void OnButtonClick()
        {
            GridService.G?.MouseManager.CleanLastIcon();
        }
    }
}