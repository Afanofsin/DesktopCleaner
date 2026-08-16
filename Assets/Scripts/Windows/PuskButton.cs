using System;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PuskButton : SerializedMonoBehaviour
{
    [OdinSerialize] private RectTransform _exitWindow;
    [OdinSerialize] private Button _puskButton;
    [OdinSerialize] private Button _exitButton;

    private const int StartPosY = -500;
    private const int EndPosY = 90;

    private Tween _showTween;
    private Camera _uiCamera;
    private RectTransform _puskButtonRect;
    private bool _isOpen;

    private void Awake()
    {
        _uiCamera = _exitWindow.GetComponentInParent<Canvas>().worldCamera;
        _puskButtonRect = (RectTransform)_puskButton.transform;

        _puskButton.OnClickAsObservable().Subscribe(_ => ToggleExitWindow()).AddTo(this);
        _exitButton.OnClickAsObservable().Subscribe(_ =>
        {
            GameSequence.Instance?.ExitGame();
            ToggleExitWindow();
        }).AddTo(this);
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (!_isOpen || pointer == null || !pointer.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 pointerPosition = pointer.position.ReadValue();
        
        bool clickedExitWindow = RectTransformUtility.RectangleContainsScreenPoint(_exitWindow, pointerPosition, _uiCamera);
        
        bool clickedPuskButton = RectTransformUtility.RectangleContainsScreenPoint(_puskButtonRect, pointerPosition, _uiCamera);

        if (!clickedExitWindow && !clickedPuskButton)
        {
            PlayHideAnim();
        }
    }

    private void ToggleExitWindow()
    {
        if (_isOpen)
        {
            PlayHideAnim();
        }
        else
        {
            PlayShowAnim();
        }
    }

    private void PlayShowAnim()
    {
        _showTween.Stop();
        _isOpen = true;

        _exitWindow.anchoredPosition =
            new Vector2(_exitWindow.anchoredPosition.x, StartPosY);

        _showTween = Tween.UIAnchoredPositionY(_exitWindow, EndPosY, 0.4f, Ease.OutBack);
    }

    private void PlayHideAnim()
    {
        _isOpen = false;
        _showTween.Stop();
        _showTween = Tween.UIAnchoredPositionY(_exitWindow, StartPosY, 0.25f, Ease.InBack);
    }
}
