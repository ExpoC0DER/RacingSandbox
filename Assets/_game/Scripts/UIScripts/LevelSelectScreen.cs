using System;
using UnityEngine;
using System.IO;
using System.Linq;
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
            DirectoryInfo[] mapDirs = dir.GetDirectories();
            foreach (DirectoryInfo mapDir in mapDirs)
            {
                LevelSelectButton newLevelSelectButton = Instantiate(_levelSelectBtnPrefab, _scrollViewContent);
                newLevelSelectButton.FileName = mapDir.Name;
                newLevelSelectButton.LevelData = GetLevelData(mapDir.Name);
                newLevelSelectButton.Texture = ExtensionMethods.LoadTexture(GetPreviewImagePath(mapDir));
            }
        }

        private string GetPreviewImagePath(DirectoryInfo dirInfo)
        {
            if (dirInfo.Exists)
            {
                FileInfo[] fileInfo = dirInfo.GetFiles().Where(f => f.Extension is ".png" or ".jpg" or ".jpeg").ToArray();
                if (fileInfo.Length > 0)
                    return fileInfo[0].FullName;
            }
            return null;
        }

        private static LevelData GetLevelData(string fileName)
        {
            FileDataHandler fileDataHandler = new FileDataHandler(fileName, true);
            LevelData levelData = fileDataHandler.Load();
            return levelData;
        }

        public void CreateNewLevel()
        {
            string newLevelName = _inputField.text.Trim();
            if (string.IsNullOrEmpty(newLevelName)) return;

            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, newLevelName);
            FileDataHandler fileDataHandler = new FileDataHandler(newLevelName, false);
            fileDataHandler.Save(new LevelData(newLevelName));
            SceneManager.LoadScene("GameScene");
        }

        private void OnEnable() { LoadLevelList(); }
    }
}
