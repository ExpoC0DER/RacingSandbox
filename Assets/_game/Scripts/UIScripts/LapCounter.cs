using System;
using TMPro;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class LapCounter : MonoBehaviour
    {
        [SerializeField] private TilePlacing _tilePlacing;
        [SerializeField] private TMP_Text _lapText;
        private GameState _gameState;

        private int _laps = 3;

        private void OnGameStateChanged(GameState gameState)
        {
            _gameState = gameState;
            if (gameState == GameState.Playing && _tilePlacing.IsLapping)
                _lapText.text = "LAP: 0";
            else
                _lapText.text = "";
        }

        private void OnLapPassed(int lap)
        {
            if (_gameState != GameState.Playing) return;

            if (lap <= _laps)
                _lapText.text = $"LAP: {lap} {(lap == _laps ? "(LAST)" : string.Empty)}";
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
            CarController2.LapPassed += OnLapPassed;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            CarController2.LapPassed -= OnLapPassed;
        }
    }
}
