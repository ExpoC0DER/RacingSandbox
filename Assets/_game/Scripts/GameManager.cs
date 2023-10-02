using System;
using UnityEngine;

namespace _game.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action PressPlay;
        public static bool IsPlaying;

        public void InvokePressPlay()
        {
            IsPlaying = true;
            PressPlay?.Invoke();
        }
    }
}
