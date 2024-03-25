using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action<GameState> OnGameStateChanged;
        private static GameState gameState = GameState.Editing;
        public void SetGameState(int value) { GameState = (GameState)value; }

        private void Start()
        {
            OnGameStateChanged?.Invoke(gameState);
        }

        public static GameState GameState
        {
            get { return gameState; }
            set
            {
                gameState = value;
                Cursor.visible = gameState != GameState.Playing; 
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
