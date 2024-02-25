using UnityEngine;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class MainMenu : MonoBehaviour
    {
        private void Start() { Time.timeScale = 1; }
        public void PlayGame() { SceneManager.LoadScene("GameScene"); }
        public void QuitGame() { Application.Quit(); }

    }
}
