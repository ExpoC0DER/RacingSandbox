using System;
using _game.Scripts.HelperScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _mapNameText;
        [SerializeField] private RectTransform _details;
        private RectTransform _thisTransform;

        private string _mapName;
        public string MapName
        {
            get { return _mapName; }
            set
            {
                _mapName = value;
                _mapNameText.text = _mapName.Split('.')[0];
            }
        }

        private void Awake() { _thisTransform = GetComponent<RectTransform>(); }

        public void OnClick()
        {
            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, MapName);
            SceneManager.LoadScene("GameScene");
        }

        public void Expand(bool value)
        {
            if (value)
                _thisTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _thisTransform.rect.height + _details.rect.height);
            else
                _thisTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _thisTransform.rect.height - _details.rect.height);

        }
    }
}
