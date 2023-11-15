using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _game.Scripts
{
    public class Timer : MonoBehaviour
    {
        private TMP_Text _timerText;
        private float _time;
        private const string TimeFormat = @"mm\:ss\:ff";

        private bool _isRunning;

        public string GetStringTime() { return TimeSpan.FromSeconds(_time).ToString(TimeFormat); }

        private void Awake() { _timerText = GetComponent<TextMeshProUGUI>(); }

        private void Update()
        {
            if (!_isRunning) return;
            _time += Time.deltaTime;
            _timerText.text = GetStringTime();
        }

        private void StartTimer()
        {
            _time = 0;
            _timerText.text = GetStringTime();
            _isRunning = true;
            _timerText.enabled = true;
        }

        private void EndTimer()
        {
            _isRunning = false;
            _timerText.enabled = false;
        }

        private void SetTimerActive(bool value)
        {
            if (value)
                StartTimer();
            else
                EndTimer();
        }

        private void OnEnable() { CarController2.SetTimerActive += SetTimerActive; }
        private void OnDisable() { CarController2.SetTimerActive -= SetTimerActive; }
    }
}
