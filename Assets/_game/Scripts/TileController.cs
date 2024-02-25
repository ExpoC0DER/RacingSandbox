using System;
using DG.Tweening;
using UnityEngine;
using NaughtyAttributes;

namespace _game.Scripts
{
    public class TileController : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public string Id { get; set; }
        [field: SerializeField, ReadOnly] public int TileID { get; set; }
        [SerializeField] private GameObject[] _arrows = Array.Empty<GameObject>();

        public void SetActiveArrows(bool value)
        {
            foreach (GameObject arrow in _arrows)
            {
                arrow.SetActive(value);
            }
        }

        private void OnDestroy()
        {
            foreach (GameObject arrow in _arrows)
                arrow.transform.DOKill();
        }
    }
}
