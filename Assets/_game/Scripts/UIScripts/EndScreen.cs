using System;
using TMPro;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class EndScreen : MonoBehaviour
    {
        [SerializeField] private Timer _timer;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private GameObject _child;
        private CarController2 _car;


        private void EnableEndScreen(CarController2 car)
        {
            _car = car;
            _timerText.text = _timer.GetStringTime();
            _child.SetActive(true);
        }

        public void GoToEditor()
        {
            _child.SetActive(false);
            GameManager.GameState = GameState.Editing;
        }

        public void Restart()
        {
            _child.SetActive(false);
            _car.RestartLevel();
        }

        private void OnEnable() { CarController2.ShowEndScreen += EnableEndScreen; }
        private void OnDisable() { CarController2.ShowEndScreen -= EnableEndScreen; }
    }
}
