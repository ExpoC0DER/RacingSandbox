using System;
using UnityEngine;
using System.IO;
using _game.Scripts.HelperScripts;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace _game.Scripts.UIScripts
{
    public class LevelSelectScreen : MonoBehaviour
    {
        [SerializeField] private LevelSelectButton _levelSelectBtnPrefab;
        [SerializeField] private Transform _scrollViewContent;
        [SerializeField] private TMP_InputField _inputField;

        private void LoadLevelList()
        {

            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
            FileInfo[] info = dir.GetFiles("*.map");
            foreach (FileInfo f in info)
            {
                LevelSelectButton newLevelSelectButton = Instantiate(_levelSelectBtnPrefab, _scrollViewContent);
                newLevelSelectButton.MapName = f.Name;
            }
        }

        public void CreateNewLevel()
        {
            if (string.IsNullOrEmpty(_inputField.text)) return;

            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, _inputField.text.Trim() + ".map");
            SceneManager.LoadScene("GameScene");
        }

        private void OnEnable() { LoadLevelList(); }
    }
}
