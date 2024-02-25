using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;

namespace _game.Scripts.Saving
{
    public class FileDataHandler
    {
        private string _dataDirName;
        private string _levelDataFileName = "levelData.map";

        private bool _useEncryption;
        private const string EncryptionSalt = "strongafencryption";


        public FileDataHandler(string dataDirName, bool useEncryption)
        {
            _dataDirName = dataDirName;
            _useEncryption = useEncryption;
        }

        public void UseEncryption(bool value) { _useEncryption = value; }

        public LevelData Load()
        {
            LevelData loadedData = null;

            string fullPath = Path.Combine(Application.persistentDataPath, _dataDirName);

            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
            if (dirInfo.Exists)
            {
                try
                {
                    FileInfo fileInfo = dirInfo.GetFiles("*.map")[0];
                    _levelDataFileName = fileInfo.Name;

                    //Load the serialized data from the file
                    using FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open);
                    using StreamReader reader = new StreamReader(stream);
                    string dataToLoad = reader.ReadToEnd();

                    if (!dataToLoad[0].Equals('{'))
                        dataToLoad = EncryptDecrypt(dataToLoad);

                    // if (_useEncryption)
                    //     dataToLoad = EncryptDecrypt(dataToLoad);

                    //deserialize data from json to C# object
                    loadedData = JsonConvert.DeserializeObject<LevelData>(dataToLoad);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occured when trying to load data from file: " + fullPath + "\n" + e);
                    throw;
                }
            }
            return loadedData;
        }

        public void Save(LevelData data)
        {
            if (!PlayerPrefs.GetString("CurrentMap", "").Equals(data.Name))
            {
                PlayerPrefs.SetString("CurrentMap", data.Name);
                RenameSave(data.Name);
            }
            
            string fullPath = Path.Combine(Application.persistentDataPath, _dataDirName, _levelDataFileName);
            try
            {
                // create the directory the file will be written to if it doesn't already exist
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                // serialize object into Json
                string dataToStore = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    //!May crash unity
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                if (_useEncryption)
                    dataToStore = EncryptDecrypt(dataToStore);

                //write the serialized data to file
                using FileStream stream = new FileStream(fullPath, FileMode.Create);
                using StreamWriter writer = new StreamWriter(stream);
                writer.Write(dataToStore);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
                throw;
            }
        }

        public void RenameSave(string newName)
        {
            Debug.Log("rename");
            string fullPathOld = Path.Combine(Application.persistentDataPath, _dataDirName);
            string fullPathNew = Path.Combine(Application.persistentDataPath, newName);
            try
            {
                // create the directory the file will be written to if it doesn't already exist
                //FileUtil.ReplaceFile(fullPathOld, fullPathNew);
                Directory.Move(fullPathOld, fullPathNew);
                //Directory.Delete(fullPathOld,true);
                _dataDirName = newName;
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to rename save: " + fullPathOld + "\n" + e);
                throw;
            }
        }

        // simple implementation of XOR encryption
        private static string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for(int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ EncryptionSalt[i % EncryptionSalt.Length]);
            }

            return modifiedData;
        }
    }
}
