using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class IconMouseManager
    {
        private IconGrid _grid;
        private GameObject _lastClickedIcon;
        private float _doubleClickTimeframe = 0.2f;
        
        private float _timeOfLastClick = 0;
        private IconView lastClickedIcon;
        private IconView lastDraggedItem;
        private GameObject lastIconSlot;
        

        public IconMouseManager(IconGrid grid)
        {
            _grid = grid;
        }

        public void SubscribeButton(IconView view)
        {
            view.ClickButton.OnPointerClickAsObservable().Subscribe(OnPointerClick).AddTo(view);
            
            view.ClickButton.OnPointerEnterAsObservable().Subscribe(OnPointerEnter).AddTo(view);
            view.ClickButton.OnPointerExitAsObservable().Subscribe(eventData => OnPointerExit(view, eventData)).AddTo(view);
            
            view.ClickButton.OnBeginDragAsObservable().Subscribe(eventData => OnBeginDrag(view, eventData)).AddTo(view);
            view.ClickButton.OnDragAsObservable().Subscribe(eventData => OnDrag(view, eventData)).AddTo(view);
            view.ClickButton.OnEndDragAsObservable().Subscribe(eventData => OnEndDrag(view, eventData)).AddTo(view);
        }
        
        public void OnBeginDrag(IconView eventView, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            foreach (var kvp in _grid.IconOccupations)
            {
                if (kvp.Value == eventView)
                {
                    lastIconSlot = kvp.Key;
                    break;
                }
            }

            if (lastIconSlot == null)
            {
                Debug.LogError("Last Icon Slot is null");
                return;
            }

            eventView.transform.SetParent(_grid.CanvasRoot);
            eventView.MakeViewDraggable();
        }

        public void OnDrag(IconView eventView, PointerEventData eventData)
        {
            eventView.transform.position = eventData.position;
        }

        public void OnEndDrag(IconView eventView, PointerEventData eventData)
        {
            GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
            if (!_grid.TryMoveIcon(eventView, droppedOn, lastIconSlot))
            {
                eventView.gameObject.transform.SetParent(lastIconSlot.transform);
                RectTransform rect = eventView.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                eventView.CancelDraggableView();
            }
            else
            {
                eventView.gameObject.transform.SetParent(droppedOn.transform);
                RectTransform rect = eventView.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                lastIconSlot = null;
                eventView.CancelDraggableView();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IconView view;
            if (lastClickedIcon?.gameObject == eventData.pointerClick)
            {
                Debug.Log("Clicked the same button");
                if (_timeOfLastClick > Time.time - _doubleClickTimeframe)
                {
                    _timeOfLastClick = Time.time;
                    DoubleClickHandler(eventData);
                    return;
                }
                _timeOfLastClick = Time.time;
                view = lastClickedIcon;
            }
            else
            {
                Debug.Log("Clicked");
                CleanLastIcon();
            
                var obj = eventData.pointerClick;
                view = obj.GetComponent<IconView>();
                lastClickedIcon = view;
            }
            
            view.ChangeBackdropAlpha(1f);
        }

        public void DoubleClickHandler(PointerEventData eventData)
        {
            Debug.Log("DoubleClick");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var obj = eventData.pointerEnter;
            
            if (lastClickedIcon?.gameObject == obj) return;
            
            var view = obj.GetComponent<IconView>();
            view.ChangeBackdropAlpha(0.1f);
            view.ActivateBackdrop();
        }
        
        public void OnPointerExit(IconView eventView, PointerEventData eventData)
        {
            if (lastClickedIcon == eventView) return;
            
            eventView.ChangeBackdropAlpha(0f);
            eventView.DeactivateBackdrop();
        }

        public void CleanLastIcon()
        {
            lastClickedIcon?.ChangeBackdropAlpha(0f);
            lastClickedIcon?.DeactivateBackdrop();
            lastClickedIcon = null;
        }
    }
}