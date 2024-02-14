using System;
using UnityEngine;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action<GameState> OnGameStateChanged;
        private static GameState gameState = GameState.Editing;
        public void SetGameState(int value) { GameState = (GameState)value; }
        public static GameState GameState
        {
            get { return gameState; }
            set
            {
                gameState = value;
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
