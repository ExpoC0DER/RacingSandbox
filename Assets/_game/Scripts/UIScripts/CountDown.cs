using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countdownText;
        private bool _countingDown;

        /// <summary>
        /// Shows and starts timer, runs action on 0
        /// </summary>
        /// <param name="seconds">Duration of countdown</param>
        /// <param name="delay">Delay before starting countdown</param>
        /// <param name="action">Action to run on end</param>
        private void StartCountdown(int seconds, float delay, Action action)
        {
            StopAllCoroutines();
            StartCoroutine(Countdown(seconds, delay, action));
        }
        
        private void Update()
        {
            if (_countingDown && GameManager.GameState == GameState.Editing)
            {
                StopAllCoroutines();
                _countdownText.text = string.Empty;
                _countingDown = false;
            }
        }

        private IEnumerator Countdown(int seconds, float delay, Action action)
        {
            _countingDown = true;
            yield return new WaitForSeconds(delay);
            for(int i = seconds; i > 0; i--)
            {
                _countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            _countdownText.text = "GO!";
            action?.Invoke();

            yield return new WaitForSeconds(1f);
            _countdownText.text = string.Empty;
            _countingDown = false;
        }

        private void OnEnable() { CarController2.StartCountdown += StartCountdown; }
        private void OnDisable() { CarController2.StartCountdown -= StartCountdown; }
    }
}
