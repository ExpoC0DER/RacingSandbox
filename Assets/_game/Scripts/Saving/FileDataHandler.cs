using UnityEngine;
using System;
using System.IO;
using _game.Scripts.HelperScripts;
using Newtonsoft.Json;

namespace _game.Scripts.Saving
{
    public class FileDataHandler
    {
        private readonly string _dataDirPath;
        private readonly string _dataFileName;

        private bool _useEncryption;
        private const string EncryptionSalt = "strongafencryption";


        public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
        {
            _dataDirPath = dataDirPath;
            _dataFileName = dataFileName;
            _useEncryption = useEncryption;
        }

        public void UseEncryption(bool value) { _useEncryption = value; }

        public LevelData Load()
        {
            string fullPath = Path.Combine(_dataDirPath, _dataFileName);
            LevelData loadedData = null;
            if (File.Exists(fullPath))
            {
                try
                {
                    //Load the serialized data from the file
                    using FileStream stream = new FileStream(fullPath, FileMode.Open);
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
            string fullPath = Path.Combine(_dataDirPath, _dataFileName);
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
