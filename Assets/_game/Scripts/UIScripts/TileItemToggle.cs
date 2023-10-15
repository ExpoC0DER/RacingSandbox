using UnityEngine;
using UnityEngine.UI;
namespace _game.Scripts.UIScripts
{
    public class TileItemToggle : MonoBehaviour
    {
        [SerializeField] private int _tileId;
        [SerializeField] private Toggle _editorModeToggle;
        [SerializeField] private TilePlacing _tilePlacing;
        private bool _wasOn;

        public void OnClick(bool value)
        {
            if (value)
            {
                _editorModeToggle.isOn = false;
                _tilePlacing.SetActiveTile(_tileId);
            }
            if(_wasOn && !value)
                _tilePlacing.SetActiveTile(-1);

            _wasOn = value;
        }

    }
}
