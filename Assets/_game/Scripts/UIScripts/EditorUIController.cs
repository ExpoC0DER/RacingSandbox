using DG.Tweening;
using TMPro;
using UnityEngine;
using FMOD;
using FMODUnity;
using UnityEngine.SceneManagement;

namespace _game.Scripts.UIScripts
{
    public class EditorUIController : MonoBehaviour
    {
        [SerializeField] private Transform[] _toggles;
        private Canvas _editorCanvas;
        [SerializeField] private TMP_Text[] _warnings;
        [SerializeField] private TilePlacing _tilePlacing;
        [SerializeField] private RectTransform _tileMenu, _tileMenuHideArrow;
        [SerializeField] private StudioEventEmitter _errorSound, _clickSound;

        private void Start() { _editorCanvas = GetComponent<Canvas>(); }

        public void EditorEnabled(bool value) => _editorCanvas.enabled = value;

        public void DisplayWarning(int id)
        {
            _errorSound.Play();
            _warnings[id].DOKill();
            _warnings[id].alpha = 1;
            _warnings[id].DOFade(0, 2f);
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

        public void PunchButtonBasic(bool value) { PunchToggle(value, _toggles[0]); }

        public void PunchButtonControl(bool value) { PunchToggle(value, _toggles[1]); }

        public void PunchButtonObstacles(bool value) { PunchToggle(value, _toggles[2]); }

        private static void PunchToggle(bool value, Transform toggle)
        {
            toggle.DOKill(true);
            if (value)
                toggle.DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
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
    }
}
