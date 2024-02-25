using FMODUnity;
using UnityEngine;

namespace _game.Scripts
{
    public class BgMusicChange : MonoBehaviour
    {
        private StudioEventEmitter _eventEmitter;

        private void Awake() { _eventEmitter = GetComponent<StudioEventEmitter>(); }

        public void ChangeMusicState(int state) { _eventEmitter.SetParameter("MusicChange", state); }
    }
}
