using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using FMOD;
using FMODUnity;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class EditorUIController : MonoBehaviour
    {
        private Canvas _editorCanvas;
        [SerializeField] private TMP_Text[] _warnings;
        [SerializeField] private TilePlacing _tilePlacing;
        [SerializeField] private RectTransform _tileMenu, _tileMenuHideArrow;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private StudioEventEmitter _errorSound, _clickSound;
        [SerializeField] private GameObject _grid;
        [SerializeField] private LinkedList<GameObject> _popupWindows = new LinkedList<GameObject>();

        private void Start() { _editorCanvas = GetComponent<Canvas>(); }

        public void EditorEnabled(bool value)
        {
            _grid.SetActive(value);
            _editorCanvas.enabled = value;
        }

        public void DisplayWarning(int id)
        {
            _errorSound.Play();
            _warnings[id].DOKill();
            _warnings[id].alpha = 1;
            _warnings[id].DOFade(0, 2f);
        }

        public void OpenPopupWindow(GameObject window)
        {
            _popupWindows.Last?.Value.SetActive(false);
            _popupWindows.AddLast(window);
            window.SetActive(true);
        }

        public void ClosePopupWindow()
        {
            _popupWindows.Last?.Value.SetActive(false);
            _popupWindows.RemoveLast();
            _popupWindows.Last?.Value.SetActive(true);
        }

        public void HideTileMenu(bool value)
        {
            if (value)
            {
                _tileMenu.DOAnchorPosY(-245, 0.5f);
                _tileMenuHideArrow.DORotate(Vector3.zero, 0.5f);
            }
            else
            {
                _tileMenu.DOAnchorPosY(0, 0.5f);
                _tileMenuHideArrow.DORotate(new(0, 0, -180), 0.5f);
            }
        }

        public void OnPressPlay()
        {
            if (_tilePlacing.CanStart())
            {
                _clickSound.Play();
                GameManager.GameState = 0;
            }
            else
                DisplayWarning(1);
        }

        public void GoToMenu() { SceneManager.LoadScene("Main Menu"); }
        public void ExitToDesktop() { Application.Quit(69); }

        public void OpenSetting(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                if (_popupWindows.Count > 0)
                    ClosePopupWindow();
                else
                    OpenPopupWindow(_settingsMenu);
        }
    }
}
