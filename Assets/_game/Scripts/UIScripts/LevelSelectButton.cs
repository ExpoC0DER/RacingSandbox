using System;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class LevelSelectButton : MonoBehaviour
    {
        private LevelData _levelData;
        public LevelData LevelData
        {
            set
            {
                _levelData = value;
                UpdateUI();
            }
        }
        public string FileName { get; set; }
        
        [SerializeField] private TMP_Text _mapNameText;
        [SerializeField] private TMP_Text _goldTrophyText;
        [SerializeField] private TMP_Text _silverTrophyText;
        [SerializeField] private TMP_Text _bronzeTrophyText;

        private void UpdateUI()
        {
            _mapNameText.text = _levelData.Name;
            _goldTrophyText.text = _levelData.TrophyTimes[Trophy.Gold].TimeToString();
            _silverTrophyText.text = _levelData.TrophyTimes[Trophy.Silver].TimeToString();
            _bronzeTrophyText.text = _levelData.TrophyTimes[Trophy.Bronze].TimeToString();
        }

        public void OnClick()
        {
            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, FileName);
            SceneManager.LoadScene("GameScene");
        }
    }
}
