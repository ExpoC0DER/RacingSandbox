using System;
using System.Collections.Generic;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class WindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject _defaultWindow;
        private LinkedList<GameObject> _popupWindows = new LinkedList<GameObject>();

        private void Awake()
        {
            if (_defaultWindow)
                _popupWindows.AddLast(_defaultWindow);
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
    }
}
