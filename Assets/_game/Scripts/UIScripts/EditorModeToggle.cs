using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class EditModeToggle : MonoBehaviour
    {
        [SerializeField] private EditorMode _editorModeToToggle;
        [SerializeField] private TMP_Text _key;
        private Toggle _toggle;

        private void Awake() { _toggle = GetComponent<Toggle>(); }

        private void OnEditorModeChanged(EditorMode editorMode)
        {
            _toggle.isOn = _editorModeToToggle == editorMode;
        }

        public void ToggleKeyBind(bool value)
        {
            _key.alpha = value ? 1 : 0.5f;
        }

        private void OnEnable() { TilePlacing.OnEditorModeChanged += OnEditorModeChanged; }

        private void OnDisable() { TilePlacing.OnEditorModeChanged += OnEditorModeChanged; }
    }
}
