using System;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class TileItemToggle : MonoBehaviour
    {
        [SerializeField] private int _tileId;
        [SerializeField] private TilePlacing _tilePlacing;
        private bool _wasOn;
        private Toggle _toggle;

        private void Awake() { _toggle = GetComponent<Toggle>(); }

        public void OnClick(bool value)
        {
            if (value)
            {
                _tilePlacing.SetActiveTile(_tileId);
            }
            if (_wasOn && !value)
                _tilePlacing.SetActiveTile(-1);

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
