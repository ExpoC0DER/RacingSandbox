using System;
using UnityEngine;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action<GameState> OnGameStateChanged;
        private static GameState _gameState = GameState.Editing;
        public void SetGameState(int value) { GameState = (GameState)value; }
        public static GameState GameState
        {
            get { return _gameState; }
            set
            {
                _gameState = value;
                OnGameStateChanged?.Invoke(value);
            }
        }
    }

    public enum GameState
    {
        Playing,
        Editing,
        Menu
    }
}
