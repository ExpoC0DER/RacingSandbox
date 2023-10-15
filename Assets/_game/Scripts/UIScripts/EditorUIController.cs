using DG.Tweening;
using TMPro;
using UnityEngine;
namespace _game.Scripts.UIScripts
{
    public class EditorUIController : MonoBehaviour
    {
        [SerializeField] private Transform[] _toggles;
        private Canvas _editorCanvas;
        [SerializeField] private TMP_Text[] _warnings;
        [SerializeField] private RectTransform _tileMenu, _tileMenuHideArrow;

        private void Start() { _editorCanvas = GetComponent<Canvas>(); }

        public void EditorEnabled(bool value) => _editorCanvas.enabled = value;

        public void DisplayWarning(int id)
        {
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

        public void PunchButtonBasic(bool value)
        {
            if (value)
                _toggles[0].DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
        }

        public void PunchButtonControl(bool value)
        {
            if (value)
                _toggles[1].DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
        }

        public void TileItemToggle(int id) { }
    }
}
