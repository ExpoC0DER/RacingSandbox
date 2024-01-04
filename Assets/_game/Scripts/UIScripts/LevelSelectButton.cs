using _game.Scripts.HelperScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _mapNameText;
        private string _mapName;
        public string MapName
        {
            get { return _mapName; }
            set
            {
                _mapName = value;
                _mapNameText.text = _mapName.Split('.')[0];
            }
        }

        public void OnClick()
        {
            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, MapName); 
            SceneManager.LoadScene("GameScene");
        }
    }
}
