using System;
using UnityEngine;
using UnityEngine.UI;

namespace _game.Scripts.UIScripts
{
    public class EditModeToggle : MonoBehaviour
    {
        [SerializeField] private EditorMode _editorModeToToggle;
        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEditorModeChanged(EditorMode editorMode)
        {
            _toggle.isOn = _editorModeToToggle == editorMode;
        }

        private void OnEnable() { TilePlacing.OnEditorModeChanged += OnEditorModeChanged; }

        private void OnDisable() { TilePlacing.OnEditorModeChanged += OnEditorModeChanged; }
    }
}
