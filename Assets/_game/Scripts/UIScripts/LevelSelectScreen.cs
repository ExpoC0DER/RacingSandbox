using System;
using UnityEngine;
using System.IO;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
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
            foreach (Transform child in _scrollViewContent)
            {
                Destroy(child.gameObject);
            }
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
            FileInfo[] info = dir.GetFiles("*.map");
            foreach (FileInfo f in info)
            {
                LevelSelectButton newLevelSelectButton = Instantiate(_levelSelectBtnPrefab, _scrollViewContent);
                newLevelSelectButton.FileName = f.Name;
                newLevelSelectButton.LevelData = GetLevelData(f.Name);
            }
        }

        private static LevelData GetLevelData(string fileName)
        {
            FileDataHandler fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName, true);
            LevelData levelData = fileDataHandler.Load();
            return levelData;
        }

        public void CreateNewLevel()
        {
            string newLevelName = _inputField.text.Trim();
            if (string.IsNullOrEmpty(newLevelName)) return;

            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, newLevelName + ".map");
            FileDataHandler fileDataHandler = new FileDataHandler(Application.persistentDataPath, newLevelName + ".map", false);
            fileDataHandler.Save(new LevelData(newLevelName));
            SceneManager.LoadScene("GameScene");
        }

        private void OnEnable() { LoadLevelList(); }
    }
}
