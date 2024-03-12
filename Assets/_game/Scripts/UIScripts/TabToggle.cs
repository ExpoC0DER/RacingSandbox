using System;
using DG.Tweening;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class TabToggle : MonoBehaviour
    {
        private RectTransform _thisRectTransform;

        private void Awake() { _thisRectTransform = transform.parent.GetComponent<RectTransform>(); }

        public void OnClick(bool value)
        {
            transform.DOKill(true);
            if (value)
            {
                _thisRectTransform.SetAsLastSibling();
                transform.DOPunchScale(new(0f, 0.2f, 0f), 0.5f);
            }
        }
    }
}
