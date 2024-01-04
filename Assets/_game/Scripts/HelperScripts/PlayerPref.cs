using System;
using UnityEngine;

namespace _game.Scripts.HelperScripts
{
    public abstract class PlayerPref
    {
        public static readonly PlayerPrefsEnum<string> CurrentMap = new PlayerPrefsEnum<string>("CurrentMap", string.Empty);

        public static T GetPlayerPref<T>(PlayerPrefsEnum<T> playerPrefsEnum)
        {
            return playerPrefsEnum.DefaultValue switch
            {
                string value => (T)(object)PlayerPrefs.GetString(playerPrefsEnum.Key, value),
                float value => (T)(object)PlayerPrefs.GetFloat(playerPrefsEnum.Key, value),
                int value => (T)(object)PlayerPrefs.GetInt(playerPrefsEnum.Key, value),
                _ => default
            };
        }

        public static void SetPlayerPref<T>(PlayerPrefsEnum<T> playerPrefsEnum, T newValue)
        {
            switch (playerPrefsEnum.DefaultValue)
            {
                case string:
                    PlayerPrefs.SetString(playerPrefsEnum.Key, (string)(object)newValue);
                    break;
                case int:
                    PlayerPrefs.SetInt(playerPrefsEnum.Key, (int)(object)newValue);
                    break;
                case float:
                    PlayerPrefs.SetFloat(playerPrefsEnum.Key, (float)(object)newValue);
                    break;
            }
        }
    }
    
    public struct PlayerPrefsEnum<T>
    {
        public readonly string Key;
        public readonly T DefaultValue;

        public PlayerPrefsEnum(string key, T defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
        }
    }
}
