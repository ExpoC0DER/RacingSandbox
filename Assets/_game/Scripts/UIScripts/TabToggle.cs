using System;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class TabToggle : MonoBehaviour
    {
        private RectTransform _thisRectTransform;

        private void Awake() { _thisRectTransform = transform.parent.GetComponent<RectTransform>(); }

        public void OnClick(bool value)
        {
            if (value)
                _thisRectTransform.SetAsLastSibling();
        }
    }
}
