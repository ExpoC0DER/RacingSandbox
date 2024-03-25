using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _game.Scripts.UIScripts
{
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _pauseMenu;
        private bool _paused;

        public static event Action<bool> OnGamePause;
        public static event Action OnGameRestart;

        private void Pause()
        {
            _paused = true;
            _pauseMenu.SetActive(_paused);
            Time.timeScale = 0;
            OnGamePause?.Invoke(true);
        }

        public void UnPause()
        {
            CloseMenu();
            OnGamePause?.Invoke(false);
        }

        public void Restart()
        {
            CloseMenu();
            OnGameRestart?.Invoke();
        }

        public void GoToEditor()
        {
            CloseMenu();
            GameManager.GameState = GameState.Editing;
        }

        private void CloseMenu()
        {
            _paused = false;
            _pauseMenu.SetActive(_paused);
            Time.timeScale = 1;
        }

        public void OnPause(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed || GameManager.GameState != GameState.Playing) return;

            if (_paused)
                UnPause();
            else
                Pause();
        }
    }
}
