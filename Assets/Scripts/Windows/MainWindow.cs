using System;
using System.Collections.Generic;
using Grid;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject backdropImage;
    [SerializeField] private GameObject fillImage;
    [SerializeField] private GameObject iconRoot;
    [SerializeField] private IconInvisible draggableTop;
    [SerializeField] private CanvasGroup canvasGroup;

    private List<Image> rayCastReceivers = new();
    
    protected DisposableBag Bag;

    private RectTransform _rect;
    private Vector3 _dragOffset;

    protected virtual void Awake()
    {
        _rect = GetComponent<RectTransform>();

        if (backdropImage != null && fillImage != null)
        {
            rayCastReceivers.Add(backdropImage.GetComponentInChildren<Image>());
            rayCastReceivers.Add(fillImage.GetComponentInChildren<Image>());

            foreach (var receiver in rayCastReceivers)
            {
                receiver.raycastTarget = false;
            }
        }
        
        closeButton.OnClickAsObservable().Subscribe(OnCloseButtonClick).AddTo(ref Bag);
    }

    private void Start()
    {
        
    }

    protected virtual void OnCloseButtonClick(Unit _) => Hide();
    
    public bool IsVisible => canvasGroup.alpha > 0f;
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        GridService.G?.OnFolderWindowClosed.OnNext(Unit.Default);
        //gameObject.SetActive(false);
    }

    public void Show()
    { 
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        //gameObject.SetActive(true);
    }
    
    protected virtual void OnDestroy() => Bag.Dispose();
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        GridService.G.MouseManager.CleanLastIcon();
        
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : GridService.G.Camera;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, eventData.position, cam, out Vector3 grabWorldPoint))
        {
            _dragOffset = _rect.position - grabWorldPoint;
        }
        
        MakeDraggable();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : GridService.G.Camera;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, eventData.position, cam, out Vector3 worldPoint))
        {
            _rect.position = worldPoint + _dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragOffset = Vector3.zero;
        CancelDraggable();
    }

    private void MakeDraggable()
    {
        IconView[] views = this.GetComponentsInChildren<IconView>();

        foreach (var receiver in views)
        {
            receiver.MakeViewDraggable();
        }

        foreach (var image in rayCastReceivers)
        {
            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                0.2f
                );
        }
        draggableTop.raycastTarget = false;
    }

    private void CancelDraggable()
    {
        IconView[] views = this.GetComponentsInChildren<IconView>();

        foreach (var receiver in views)
        {
            receiver.CancelDraggableView();
        }
        
        foreach (var image in rayCastReceivers)
        {
            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                1f
            );
        }
        draggableTop.raycastTarget = true;
    }
}