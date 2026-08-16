using System;
using Grid.Services;
using R3;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class CounterText : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        private int total = 0;
        private int current = 0;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        public void Start()
        {
            GameStateService.G?.TotalIcons.Subscribe(UpdateTotal).AddTo(this);
            GameStateService.G?.BinnedIcons.Subscribe(UpdateCurrent).AddTo(this);
        }

        private void UpdateTotal(int val)
        {
            total = val;
            text.text = $"{current} / {total}";
        }
        
        private void UpdateCurrent(int val)
        {
            current = val;
            text.text = $"{current} / {total}";
        }
    }
}