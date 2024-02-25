using System;
using System.Linq;
using _game.Scripts.Saving;
using TMPro;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class TrophyTime : MonoBehaviour, IDataPersistence
    {
        [SerializeField] private Trophy _trophy;
        [SerializeField] private TMP_InputField _minutes;
        [SerializeField] private TMP_InputField _seconds;
        [SerializeField] private TMP_InputField _hundredths;

        public void ClampSeconds(string inputText)
        {
            if (inputText.Length > 1 && int.TryParse(inputText, out int time))
            {
                inputText = time > 59 ? inputText[0].ToString() : inputText;
            }
            _seconds.text = inputText;
        }

        public void LoadLevel(LevelData data)
        {
            int milliseconds = data.TrophyTimes[_trophy];
            _minutes.text = (milliseconds / (60 * 1000)).ToString();
            milliseconds %= (60 * 1000);

            _seconds.text = (milliseconds / 1000).ToString();
            milliseconds %= 1000;

            _hundredths.text = (milliseconds / 10).ToString();

        }
        public void SaveLevel(LevelData data)
        {
            int milliseconds = 0;
            if (int.TryParse(_minutes.text, out int minutes))
                milliseconds += minutes * 60 * 1000;
            if (int.TryParse(_seconds.text, out int seconds))
                milliseconds += seconds * 1000;
            if (int.TryParse(_hundredths.text, out int hundredths))
                milliseconds += hundredths * 10;

            data.TrophyTimes[_trophy] = milliseconds;
        }
    }
    public enum Trophy
    {
        Gold,
        Silver,
        Bronze
    }
}
