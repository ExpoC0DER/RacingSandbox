using UnityEngine;
using System;
using System.IO;

namespace _game.Scripts.Saving
{
    public class FileDataHandler
    {
        private readonly string _dataDirPath;
        private readonly string _dataFileName;

        private readonly bool _useEncryption = false;
        private const string EncryptionSalt = "strongafencryption";


        public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
        {
            _dataDirPath = dataDirPath;
            _dataFileName = dataFileName;
            _useEncryption = useEncryption;
        }

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

                    if (_useEncryption)
                        dataToLoad = EncryptDecrypt(dataToLoad);

                    //deserialize data from json to C# object
                    loadedData = JsonUtility.FromJson<LevelData>(dataToLoad);
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
                string dataToStore = JsonUtility.ToJson(data, true);

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
