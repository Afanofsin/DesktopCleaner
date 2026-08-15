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
        
        private void OnBeginDrag(IconView eventView, PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!eventView.isDraggingAllowed) return;
            
            CleanLastIcon();
            lastIconSlot = eventView.gameObject.transform.parent.gameObject;
            _currentGrid = GridService.G.SearchForActiveGridWithSlot(lastIconSlot);

            if (lastIconSlot == null || _currentGrid == null)
            {
                Debug.LogError("Last Icon Slot or Grid is null");
                return;
            }

            eventView.transform.SetParent(_currentGrid.CanvasRoot);
            eventView.MakeViewDraggable();
        }

        private void OnDrag(IconView eventView, PointerEventData eventData)
        {
            //eventView.transform.position = eventData.position;
            if (!eventView.isDraggingAllowed) return;
            
            RectTransform rect = eventView.GetComponent<RectTransform>();
            
            Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : GridService.G.Camera;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, cam, out Vector3 worldPoint))
            {
                rect.position = worldPoint;
            }
        }

        private void OnEndDrag(IconView eventView, PointerEventData eventData)
        {
            if (!eventView.isDraggingAllowed) return;
            // GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
            //
            // if (droppedOn == null)
            // {
            //     CancelDrag(eventView);
            //     return;
            // }
            //
            // IconView hitIcon = droppedOn.GetComponent<IconView>();
            // if (hitIcon != null && hitIcon != eventView)
            // {
            //     droppedOn = hitIcon.transform.parent.gameObject;
            // }
            // //
            // else
            // {
            //     var targetGrid = GridService.G?.SearchForActiveGridWithSlot(droppedOn);
            //     if (targetGrid != null && targetGrid.IconOccupations.TryGetValue(droppedOn, out var existingIcon))
            //     {
            //         hitIcon = existingIcon;
            //     }
            // }
            
            var (targetGrid, droppedOn) = GridService.G.GetClosestSlotAtPosition(eventData.position, maxSnapDistance: 200f);
        
            if (droppedOn == null || targetGrid == null)
            {
                CancelDrag(eventView);
                return;
            }

            // 2. Check if the resolved slot already has an icon
            targetGrid.IconOccupations.TryGetValue(droppedOn, out var hitIcon);
            
            if (hitIcon is FolderView folder)
            {
                var slotToMoveTo = folder.PutInFolder(eventView);
                if (slotToMoveTo == null) CancelDrag(eventView);
                else
                {
                    SnapToSlotTransform(eventView, slotToMoveTo.transform);
                    
                    _currentGrid.RemoveIcon(lastIconSlot);
                    
                    lastIconSlot = null;
                
                    eventView.CancelDraggableView();
                }

                return;
            }
            
            if (!_currentGrid.TryMoveIcon(eventView, droppedOn, lastIconSlot, out var swappedIcon))
            {
                CancelDrag(eventView);
            }
            else
            {
                SnapToSlotTransform(eventView, droppedOn.transform);
            
                if (swappedIcon != null)
                {
                    SnapToSlotTransform(eventView, lastIconSlot.transform);
                }
            
                lastIconSlot = null;
                
                eventView.CancelDraggableView();
                eventView.ChangeBackdropAlpha(0.1f);
                eventView.ActivateBackdrop();
            }    
                
            //
            // if (!_currentGrid.TryMoveIcon(eventView, droppedOn, lastIconSlot, out var swappedIcon))
            // {
            //     CancelDrag(eventView);
            // }
            // //else if (swappedIcon is BinView)
            // else if (swappedIcon is FolderView folder)
            // {
            //     var slotToMoveTo = folder.PutInFolder(eventView);
            //     if (slotToMoveTo == null) CancelDrag(eventView);
            //     else
            //     {
            //         eventView.gameObject.transform.SetParent(slotToMoveTo.transform);
            //         RectTransform rect = eventView.GetComponent<RectTransform>();
            //         rect.anchoredPosition = Vector2.zero;
            //         
            //         _currentGrid.RemoveIcon(lastIconSlot);
            //         
            //         lastIconSlot = null;
            //     
            //         eventView.CancelDraggableView();
            //     }
            // }
            // else
            // {
            //     eventView.gameObject.transform.SetParent(droppedOn.transform);
            //     RectTransform rect = eventView.GetComponent<RectTransform>();
            //     rect.anchoredPosition = Vector2.zero;
            //
            //     if (swappedIcon != null)
            //     {
            //         swappedIcon.gameObject.transform.SetParent(lastIconSlot.transform);
            //         rect = swappedIcon.GetComponent<RectTransform>();
            //         rect.anchoredPosition = Vector2.zero;
            //     }
            //
            //     lastIconSlot = null;
            //     
            //     eventView.CancelDraggableView();
            //     eventView.ChangeBackdropAlpha(0.1f);
            //     eventView.ActivateBackdrop();
            // }
        }
        
        private void SnapToSlotTransform(IconView view, Transform slotTransform)
        {
            view.transform.SetParent(slotTransform, false);
    
            RectTransform rect = view.GetComponent<RectTransform>();
    
            // Ensure stretch-stretch anchors
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
    
            // Reset Left, Bottom, Right, Top (0, 0, 0, 0)
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
    
            // Reset scale and Z-depth
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        private void CancelDrag(IconView eventView)
        {
            // eventView.gameObject.transform.SetParent(lastIconSlot.transform);
            // RectTransform rect = eventView.GetComponent<RectTransform>();
            // rect.anchoredPosition = Vector2.zero;
            SnapToSlotTransform(eventView, lastIconSlot.transform);
            eventView.CancelDraggableView();
        }

        private void OnPointerClick(PointerEventData eventData)
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

        private void DoubleClickHandler(IconView view, PointerEventData eventData)
        {
            Debug.Log("DoubleClick");
            view?.OnDoubleClick();
        }

        private void OnPointerEnter(PointerEventData eventData)
        {
            var obj = eventData.pointerEnter;
            
            if (lastClickedIcon?.gameObject == obj) return;
            
            var view = obj.GetComponent<IconView>();

            if (view == null) return;
            
            view.ChangeBackdropAlpha(0.1f);
            view.ActivateBackdrop();
        }
        
        private void OnPointerExit(IconView eventView, PointerEventData eventData)
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