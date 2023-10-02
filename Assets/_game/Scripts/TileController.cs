using DG.Tweening;
using UnityEngine;

namespace _game.Scripts
{
    public class TileController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _arrows;
        [SerializeField] private float _time = 0.5f;

        private void Start()
        {
            StartAnim();
        }

        public void SetActiveArrows(bool value)
        {
            foreach (GameObject arrow in _arrows)
            {
                arrow.transform.DOKill();
                arrow.SetActive(value);
            }
        }

        private void OnDestroy()
        {
            foreach (GameObject arrow in _arrows)
                arrow.transform.DOKill();
        }

        private void StartAnim()
        {
            foreach (GameObject arrow in _arrows)
                switch (arrow.transform.rotation.eulerAngles.y)
                {
                    case 0:
                        arrow.transform.DOLocalMoveZ(arrow.transform.localPosition.z + 2, _time).SetLoops(-1, LoopType.Yoyo);
                        break;
                    case 90:
                        arrow.transform.DOLocalMoveX(arrow.transform.localPosition.x + 2f, _time).SetLoops(-1, LoopType.Yoyo);
                        break;
                    case 180:
                        arrow.transform.DOLocalMoveZ(arrow.transform.localPosition.z - 2f, _time).SetLoops(-1, LoopType.Yoyo);
                        break;
                    case 270:
                        arrow.transform.DOLocalMoveX(arrow.transform.localPosition.x - 2f, _time).SetLoops(-1, LoopType.Yoyo);
                        break;
                }
        }
    }
}
