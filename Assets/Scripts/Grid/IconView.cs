using System;
using R3;
using R3.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Grid
{
    public class IconView : MonoBehaviour
    {
        [SerializeField] private Image selectBackdrop;
        [SerializeField] private Image iconSprite;
        [SerializeField] private IconInvisible icon;
        public Button ClickButton;
        
        public void Setup()
        {
            ClickButton = GetComponent<Button>();
        }
        
        public void MakeViewDraggable()
        {
            DeactivateBackdrop();
            icon.raycastTarget = false;
            iconSprite.color = new Color(
                iconSprite.color.r, 
                iconSprite.color.g, 
                iconSprite.color.b, 
                0.5f);
        }

        public void CancelDraggableView()
        {
            icon.raycastTarget = true;
            iconSprite.color = new Color(
                iconSprite.color.r, 
                iconSprite.color.g, 
                iconSprite.color.b, 
                1f);
        }

        public void ChangeBackdropAlpha(float alpha)
        {
            selectBackdrop.color = new Color(
                selectBackdrop.color.r, 
                selectBackdrop.color.g, 
                selectBackdrop.color.b, 
                alpha);
        }
        
        public void ActivateBackdrop() => selectBackdrop.gameObject.SetActive(true);
        public void DeactivateBackdrop() => selectBackdrop.gameObject.SetActive(false);
    }
}