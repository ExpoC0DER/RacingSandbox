using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using NaughtyAttributes;
using TMPro;

namespace _game.Scripts.UIScripts
{
    public class TileItemToggle : MonoBehaviour
    {
        [SerializeField, ReadOnly] private int _tileId;
        public int TileId
        {
            get { return _tileId; }
            set
            {
                _tileId = value;
                _idText.text = _tileId.ToString();
            }
        }
        [field: SerializeField, ReadOnly] public TilePlacing TilePlacing { get; set; }
        [field: SerializeField, ReadOnly] public ToggleGroup ToggleGroup { get; set; }
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _idText;
        public Sprite Sprite
        {
            set
            {
                if (value == null) return;
                _image.sprite = value;
                _idText.enabled = false;
            }
        }

        private bool _wasOn;
        private Toggle _toggle;

        private void Awake() { _toggle = GetComponent<Toggle>(); }
        private void Start() { _toggle.group = ToggleGroup; }

        public void OnClick(bool value)
        {
            if (value)
            {
                TilePlacing.CreateTileById(TileId);
            }
            if (_wasOn && !value)
                TilePlacing.CreateTileById(-1);

            _wasOn = value;
        }

        private void OnGameStateChanged(GameState obj)
        {
            _wasOn = false;
            _toggle.isOn = false;
        }

        private void OnEditorModeChanged(EditorMode editorMode)
        {
            if (editorMode == EditorMode.Place) return;
            _wasOn = false;
            _toggle.isOn = false;
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
            TilePlacing.OnEditorModeChanged += OnEditorModeChanged;
        }
        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            TilePlacing.OnEditorModeChanged -= OnEditorModeChanged;
        }
    }
}
