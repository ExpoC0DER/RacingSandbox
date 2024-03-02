using System;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class LevelSelectButton : MonoBehaviour
    {
        private LevelData _levelData;
        public LevelData LevelData
        {
            set
            {
                _levelData = value;
                UpdateText();
            }
        }

        private Texture2D _texture;
        public Texture2D Texture
        {
            set
            {
                _texture = value;
                if (_texture != null)
                    SetTexture(_texture);
            }
        }

        public string FileName { get; set; }

        [SerializeField] private TMP_Text _mapNameText;
        [SerializeField] private TMP_Text _goldTrophyText;
        [SerializeField] private TMP_Text _silverTrophyText;
        [SerializeField] private TMP_Text _bronzeTrophyText;
        [SerializeField] private Image _previewImage;
        [SerializeField] private RectTransform _mask;

        private void UpdateText()
        {
            _mapNameText.text = _levelData.Name;
            _goldTrophyText.text = _levelData.TrophyTimes[Trophy.Gold].TimeToString();
            _silverTrophyText.text = _levelData.TrophyTimes[Trophy.Silver].TimeToString();
            _bronzeTrophyText.text = _levelData.TrophyTimes[Trophy.Bronze].TimeToString();
        }

        private void SetTexture(Texture2D spriteTexture)
        {
            Sprite newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100);
            _previewImage.sprite = newSprite;

            float ratio = (float)spriteTexture.width / spriteTexture.height;
            float newWidth = _mask.sizeDelta.y * ratio;
            _mask.sizeDelta = new Vector2(newWidth, _mask.sizeDelta.y);
        }

        public void OnClick()
        {
            PlayerPref.SetPlayerPref(PlayerPref.CurrentMap, FileName);
            SceneManager.LoadScene("GameScene");
        }
    }
}
