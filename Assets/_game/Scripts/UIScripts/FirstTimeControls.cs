using System;
using UnityEngine;

namespace _game.Scripts.UIScripts
{
    public class FirstTimeControls : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        private void Start()
        {
            _popup.SetActive(PlayerPrefs.GetInt("FirstTimeControls", 0) == 0);
            PlayerPrefs.SetInt(name, 1);
        }
    }
}
