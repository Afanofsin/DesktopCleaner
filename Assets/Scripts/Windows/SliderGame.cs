using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Windows
{
    public class SliderGame : CaptchaGame
    {
        [SerializeField] private List<RectTransform> handleList;
        [SerializeField] private List<RectTransform> trackingList;
        
        [SerializeField] private GameObject targetPrefab;
        private List<RectTransform> areaList = new();
        
        [SerializeField] Button submitButton;
        
        private void Awake()
        {
            submitButton.OnClickAsObservable().Subscribe(_ => Evaluate()).AddTo(this);
            StartLevel();
        }

        public override void StartGame()
        {
            base.StartGame();
            StartLevel();
        }

        public void StartLevel()
        {
            if (areaList?.Count != 0)
            {
                foreach (var area in areaList)
                {
                    GameObject.Destroy(area.gameObject);
                }
            }
            areaList = new List<RectTransform>();
            
            foreach (var trans in trackingList)
            {
                float val = Random.Range(0f, 1f);
                areaList.Add(CreateElementAtValue(val, trans));
            }
        }
        
        private RectTransform CreateElementAtValue(float normalizedValue,  RectTransform trackArea)
        {
            GameObject target = Instantiate(targetPrefab, trackArea);
            RectTransform rect = target.GetComponent<RectTransform>();

            SetElementPosition(rect, normalizedValue);

            return rect;
        }
        
        private void SetElementPosition(RectTransform elementRect, float normalizedValue)
        {
            normalizedValue = Mathf.Clamp01(normalizedValue);
            
            elementRect.anchorMin = new Vector2(normalizedValue, 0.5f);
            elementRect.anchorMax = new Vector2(normalizedValue, 0.5f);
            elementRect.pivot = new Vector2(0.5f, 0.5f);
            
            elementRect.anchoredPosition = Vector2.zero;
        }

        private void Evaluate()
        {
            int trueRes = 0;
            foreach (var handle in handleList)
            {
                foreach (var area in areaList)
                {
                    bool res = IsHandleOverlappingMarker(handle, area);
                    if (res) trueRes++;
                }
            }

            if (trueRes == 5)
            {
                WinGame();
            }
            else
            {
                FailGame();
            }
        }
        
        public bool IsHandleOverlappingMarker(RectTransform handle, RectTransform area)
        {
            if (handle == null || area == null) return false;

            Bounds handleBounds = GetWorldBounds(handle);
            Bounds areaBounds = GetWorldBounds(area);
            
            return areaBounds.Contains(handleBounds.center);
        }
        
        private Bounds GetWorldBounds(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Bounds bounds = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < 4; i++)
            {
                bounds.Encapsulate(corners[i]);
            }
            return bounds;
        }
    }
}