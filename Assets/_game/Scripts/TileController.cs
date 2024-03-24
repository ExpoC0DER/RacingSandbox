using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using NaughtyAttributes;
using NUnit.Framework;
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
        public Rigidbody Rb { get; private set; }
        public BoxCollider BoxCollider { get; private set; }
        [field: SerializeField, ReadOnly] public string Id { get; set; }
        [field: SerializeField, ReadOnly] public int TileID { get; set; }
        [SerializeField] private bool _isColliding;
        [SerializeField] private bool _canBePlaced;
        public bool IsColliding { get { return /*!_canBePlaced ||*/ _isColliding; } }
        [field: SerializeField, ReadOnly] public bool IsSelected { get; set; } = true;
        [SerializeField] private GameObject[] _arrows = Array.Empty<GameObject>();
        [SerializeField] private UnityEvent _onRotate;

        private void Awake()
        {
            BoxCollider = GetComponent<BoxCollider>();
            Rb = GetComponent<Rigidbody>();

        }
        private void Start()
        {
            _canBePlaced = false;
            //StartCoroutine(nameof(SetColliding));
        }

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

        public void Place()
        {
            SetActiveArrows(false);
            IsSelected = false;
            ChangeRenderLayer((int)Layer.NormalRender);
        }

        public void PickUp()
        {
            SetActiveArrows(true);
            IsSelected = true;
            ChangeRenderLayer((int)Layer.PickedUpRender);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsSelected)
            {
                _isColliding = true;
                print(other.name);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (IsSelected)
                _isColliding = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsSelected)
                _isColliding = false;
        }


        private IEnumerator SetColliding()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            _canBePlaced = true;
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

        private static bool IsDefaultLayer(int layer) { return layer is (int)Layer.Road or (int)Layer.Tile or (int)Layer.Object or (int)Layer.RoadTrigger or (int)Layer.ObjectTrigger or (int)Layer.Default; }

    }
}
