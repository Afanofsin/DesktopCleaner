using System.Collections.Generic;
using System.Linq;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class IconMouseManager
    {
        private GameObject _lastClickedIcon;
        private float _doubleClickTimeframe = 0.2f;
        
        private float _timeOfLastClick = 0;
        private IconView lastClickedIcon;
        private IconView lastDraggedItem;
        private GameObject lastIconSlot;
        private IconGrid _currentGrid;
        

        public IconMouseManager()
        {
            lastClickedIcon = GridService.G.lastClickedIcon;
            lastDraggedItem = GridService.G.lastDraggedItem;
            lastIconSlot = GridService.G.lastIconSlot;
        }

        public void SubscribeButton(IconView view)
        {
            view.ClickButton.OnPointerClickAsObservable().Subscribe(OnPointerClick).AddTo(view);
            
            view.ClickButton.OnPointerEnterAsObservable().Subscribe(OnPointerEnter).AddTo(view);
            view.ClickButton.OnPointerExitAsObservable().Subscribe(eventData => OnPointerExit(view, eventData)).AddTo(view);
            
            view.ClickButton.OnBeginDragAsObservable().Subscribe(eventData => OnBeginDrag(view,eventData)).AddTo(view);
            view.ClickButton.OnDragAsObservable().Subscribe(eventData => OnDrag(view, eventData)).AddTo(view);
            view.ClickButton.OnEndDragAsObservable().Subscribe(eventData => OnEndDrag(view,eventData)).AddTo(view);
        }
        
        public void OnBeginDrag(IconView eventView, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            CleanLastIcon();
            lastIconSlot = eventView.gameObject.transform.parent.gameObject;
            _currentGrid = GridService.G.SearchForActiveGridWithSlot(lastIconSlot);
            
            // foreach (var kvp in grid.IconOccupations)
            // {
            //     if (kvp.Value == eventView)
            //     {
            //         lastIconSlot = kvp.Key;
            //         break;
            //     }
            // }

            if (lastIconSlot == null || _currentGrid == null)
            {
                Debug.LogError("Last Icon Slot or Grid is null");
                return;
            }

            eventView.transform.SetParent(_currentGrid.CanvasRoot);
            eventView.MakeViewDraggable();
        }

        public void OnDrag(IconView eventView, PointerEventData eventData)
        {
            //eventView.transform.position = eventData.position;
            
            RectTransform rect = eventView.GetComponent<RectTransform>();
            
            Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : GridService.G.Camera;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, cam, out Vector3 worldPoint))
            {
                rect.position = worldPoint;
            }
        }

        public void OnEndDrag(IconView eventView, PointerEventData eventData)
        {
            GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;

            if (droppedOn == null)
            {
                CancelDrag(eventView);
                return;
            }
            
            IconView hitIcon = droppedOn.GetComponent<IconView>();
            if (hitIcon != null && hitIcon != eventView)
            {
                droppedOn = hitIcon.transform.parent.gameObject;
            }
            
            if (!_currentGrid.TryMoveIcon(eventView, droppedOn, lastIconSlot, out var swappedIcon))
            {
                CancelDrag(eventView);
            }
            else
            {
                eventView.gameObject.transform.SetParent(droppedOn.transform);
                RectTransform rect = eventView.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;

                if (swappedIcon != null)
                {
                    swappedIcon.gameObject.transform.SetParent(lastIconSlot.transform);
                    rect = swappedIcon.GetComponent<RectTransform>();
                    rect.anchoredPosition = Vector2.zero;
                }

                lastIconSlot = null;
                
                eventView.CancelDraggableView();
            }
  
            eventView.ChangeBackdropAlpha(0.1f);
            eventView.ActivateBackdrop();
        }

        private void CancelDrag(IconView eventView)
        {
            eventView.gameObject.transform.SetParent(lastIconSlot.transform);
            RectTransform rect = eventView.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            eventView.CancelDraggableView();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            IconView view;
            if (lastClickedIcon?.gameObject == eventData.pointerClick)
            {
                Debug.Log("Clicked the same button");
                view = lastClickedIcon;
                if (_timeOfLastClick > Time.time - _doubleClickTimeframe)
                {
                    _timeOfLastClick = Time.time;
                    DoubleClickHandler(view, eventData);
                    return;
                }
                _timeOfLastClick = Time.time;
            }
            else
            {
                Debug.Log("Clicked");
                CleanLastIcon();
            
                var obj = eventData.pointerClick;
                view = obj.GetComponent<IconView>();
                
                lastClickedIcon = view;
                _timeOfLastClick = Time.time;
            }
            
            view?.ChangeBackdropAlpha(1f);
        }

        public void DoubleClickHandler(IconView view, PointerEventData eventData)
        {
            Debug.Log("DoubleClick");
            view?.OnDoubleClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var obj = eventData.pointerEnter;
            
            if (lastClickedIcon?.gameObject == obj) return;
            
            var view = obj.GetComponent<IconView>();

            if (view == null) return;
            
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