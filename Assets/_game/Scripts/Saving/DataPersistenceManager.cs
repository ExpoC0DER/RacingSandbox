using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using _game.Scripts.HelperScripts;

namespace _game.Scripts.Saving
{
    public class DataPersistenceManager : MonoBehaviour
    {
        [Header("File storage config")]
        [SerializeField] private string _fileName;
        [SerializeField] private bool _useEncryption;

        private LevelData _levelData = new LevelData();
        private List<IDataPersistence> _dataPersistenceObjects;
        private FileDataHandler _fileDataHandler;

        public static DataPersistenceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Found more than one Data Persistence Manager in the scene.");
            }
            Instance = this;
        }

        private void Start()
        {
            _fileName = PlayerPref.GetPlayerPref(PlayerPref.CurrentMap);
            if (string.IsNullOrEmpty(_fileName))
                _fileName = _levelData.Name + ".map";
            
            _fileDataHandler = new FileDataHandler(Application.persistentDataPath, _fileName, _useEncryption);
            _dataPersistenceObjects = FindAllDataPersistenceObjects();
            LoadLevel();
        }

        public void NewLevel() { _levelData = new LevelData(); }

        public void LoadLevel()
        {
            // Load any saved data from a file using a data handler
            _levelData = _fileDataHandler.Load();

            //if no level data can be loaded, load new level
            if (_levelData == null)
            {
                Debug.Log("No level data was found. Loading new level.");
                NewLevel();
            }

            foreach (IDataPersistence dataPersistenceObject in _dataPersistenceObjects)
            {
                dataPersistenceObject.LoadLevel(_levelData);
            }
            Debug.Log("Loaded " + _levelData.Name);
        }

        public void SaveLevel()
        {
            foreach (IDataPersistence dataPersistenceObject in _dataPersistenceObjects)
            {
                dataPersistenceObject.SaveLevel(_levelData);
            }

            Debug.Log("Saved " + _levelData.Name);

            // save that data to a file using the data handler
            _fileDataHandler.Save(_levelData);
        }

        private void OnApplicationQuit() { SaveLevel(); }

        private static List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>();
            //List<IDataPersistence> test = new List<IDataPersistence>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).OfType<IDataPersistence>());
            return new List<IDataPersistence>(dataPersistenceObjects);
        }

    }
}
