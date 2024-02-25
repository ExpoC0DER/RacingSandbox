using System.IO;
using System.Linq;
using _game.Scripts.HelperScripts;
using _game.Scripts.Saving;
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
            string path = EditorUtility.OpenFilePanel("Select new preview image", "", "png,jpeg,jpg");
            if (path.Length != 0)
            {
                if (GetPreviewImagePath() == null)
                    FileUtil.ReplaceFile(path, Path.Combine(Application.persistentDataPath, NameInputText, "previewImage.png"));
                else
                    FileUtil.ReplaceFile(path, GetPreviewImagePath());

                Texture2D spriteTexture = LoadTexture(path);
                if (spriteTexture != null)
                    SetTexture(spriteTexture);
            }
        }

        private Texture2D LoadTexture(string filePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D tex2D = new Texture2D(2, 2);
                if (tex2D.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
                    return tex2D; // If data = readable -> return texture
            }

            return null; // Return null if load failed
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

            Texture2D spriteTexture = LoadTexture(GetPreviewImagePath());
            if (spriteTexture != null)
                SetTexture(spriteTexture);
        }
        public void SaveLevel(LevelData data) { data.Name = NameInputText; }
    }
}
