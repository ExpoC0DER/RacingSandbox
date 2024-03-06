using System;
using DG.Tweening;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace _game.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    public class TileController : MonoBehaviour
    {
        public Vector3 Position { get => transform.position; set => transform.position = value; }
        public Quaternion Rotation { get => transform.rotation; set => transform.rotation = value; }
        public BoxCollider BoxCollider { get; set; }
        [field: SerializeField, ReadOnly] public string Id { get; set; }
        [field: SerializeField, ReadOnly] public int TileID { get; set; }
        [SerializeField] private GameObject[] _arrows = Array.Empty<GameObject>();
        [SerializeField] private UnityEvent _onRotate;


        private void Awake() { BoxCollider = GetComponent<BoxCollider>(); }

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

        public void Rotate(float rotateBy)
        {
            transform.Rotate(Vector3.up, rotateBy);
            _onRotate.Invoke();
        }


        public void ChangeCollisionAndRenderLayer(int layer) { ChangeCollisionAndRenderLayer(gameObject, layer); }
        public void ChangeCollisionLayer(int layer) { ChangeCollisionLayer(gameObject, layer); }
        public void ChangeRenderLayer(int layer) { ChangeRenderLayer(gameObject, layer); }
        public void Destroy() { Destroy(gameObject); }


        private static void ChangeCollisionAndRenderLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                if (IsDefaultLayer(layer))
                {
                    ChangeRenderLayer(child.gameObject, (int)Layer.NormalRender);
                    ChangeCollisionLayer(child.gameObject, (int)Layer.Default);
                }
                else
                {
                    ChangeRenderLayer(child.gameObject, (int)Layer.PickedUpRender);
                    ChangeCollisionLayer(child.gameObject, (int)Layer.NoCollision);
                }
            }
        }

        private static void ChangeCollisionLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            if (gameObject.layer is (int)Layer.Default or (int)Layer.NoCollision)
                gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                ChangeCollisionLayer(child.gameObject, layer);
            }
        }

        private static void ChangeRenderLayer(GameObject gameObject, int layer)
        {
            if (!gameObject) return;

            if (gameObject.layer is (int)Layer.NormalRender or (int)Layer.PickedUpRender)
                gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                ChangeRenderLayer(child.gameObject, layer);
            }
        }

        private static bool IsDefaultLayer(int layer) { return layer is (int)Layer.Road or (int)Layer.Object or (int)Layer.RoadTrigger or (int)Layer.ObjectTrigger or (int)Layer.Default; }

    }
}
