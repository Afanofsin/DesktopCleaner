using System;
using Grid.Services;
using R3;
using TMPro;
using UnityEngine;

namespace Grid
{
    public class TimerText : MonoBehaviour
    {
        [SerializeField] TMP_Text textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            GameStateService.G?.TotalTime.Subscribe(UpdateTotalTime).AddTo(this);
        }

        private void UpdateTotalTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            textMesh.text = string.Format("Time: {0}:{1:00}", minutes, seconds);
        }
    }
}