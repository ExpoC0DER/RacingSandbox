using System.IO;
using System.Linq;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
using SFB;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class LevelSettings : MonoBehaviour, IDataPersistence
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Image _previewImage;
        [SerializeField] private RectTransform _mask;

        private LevelData _levelData;
        private string NameInputText { get => _nameInput.text.Trim(); }

        public void OpenFile()
        {
            // Open file with filter
            ExtensionFilter[] extensions =
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
            };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select new preview image", "", extensions, false);
            if (paths.Length > 1 && paths[0].Length != 0)
            {
                if (GetPreviewImagePath() == null)
                {
                    File.Copy(paths[0], Path.Combine(Application.persistentDataPath, NameInputText, "previewImage.png"), true);
                }
                else
                {
                    File.Copy(paths[0], GetPreviewImagePath(), true);
                }

                Texture2D spriteTexture = ExtensionMethods.LoadTexture(paths[0]);
                if (spriteTexture != null)
                    SetTexture(spriteTexture);
            }
        }

        private string GetPreviewImagePath()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.persistentDataPath, _levelData.Name));
            if (directoryInfo.Exists)
            {
                FileInfo[] fileInfo = directoryInfo.GetFiles().Where(f => f.Extension is ".png" or ".jpg" or ".jpeg").ToArray();
                if (fileInfo.Length > 0)
                    return fileInfo[0].FullName;
            }
            return null;
        }

        private void SetTexture(Texture2D spriteTexture)
        {
            Sprite newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100);
            _previewImage.sprite = newSprite;

            float ratio = (float)spriteTexture.width / spriteTexture.height;
            float newWidth = _mask.sizeDelta.y * ratio;
            _mask.sizeDelta = new Vector2(newWidth, _mask.sizeDelta.y);
        }

        public void LoadLevel(LevelData data)
        {
            _nameInput.text = data.Name;
            _levelData = data;

            Texture2D spriteTexture = ExtensionMethods.LoadTexture(GetPreviewImagePath());
            if (spriteTexture != null)
                SetTexture(spriteTexture);
        }
        public void SaveLevel(LevelData data) { data.Name = NameInputText; }
    }
}
