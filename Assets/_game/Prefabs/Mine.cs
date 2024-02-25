using System;
using _game.Scripts;
using UnityEngine;
using NaughtyAttributes;

namespace _game.Prefabs
{
    public class Mine : MonoBehaviour
    {
        [SerializeField, ValidateInput("CallbackTest","Waning")] private float _strength;
        [SerializeField] private GameObject _model;
        private Collider _collider;

        private bool CallbackTest(float value)
        {
            return value > 50;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Car")) return;

            if (other.gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = Vector3.up * _strength;
                _model.SetActive(false);
                _collider.enabled = false;
            }
        }

        private void Reset()
        {
            _model.SetActive(true);
            _collider.enabled = true;
        }

        private void OnEnable()
        {
            TilePlacing.OnResetObstacles += Reset;
        }

        private void OnDisable()
        {
            TilePlacing.OnResetObstacles -= Reset;
        }
    }
}
