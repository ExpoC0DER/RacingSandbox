using System;
using System.Collections;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace _game.Scripts.Turret
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private Transform _cannon;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Rigidbody _bullet;
        [SerializeField, MinValue(0.5f)] private float _shootDelay;
        [SerializeField] private float _bulletForce;
        private Transform _car;
        private SphereCollider _detectionRadius;

        private void Awake() { _detectionRadius = GetComponent<SphereCollider>(); }

        private void FixedUpdate()
        {
            if (!_car) return;

            _cannon.LookAt(_car);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                _car = other.transform;
                StartCoroutine(nameof(Shooting));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                _car = null;
                StopCoroutine(nameof(Shooting));
            }
        }

        private IEnumerator Shooting()
        {
            yield return new WaitForSeconds(_shootDelay);
            Rigidbody bullet = Instantiate(_bullet, _shootPoint.position, _cannon.rotation);
            bullet.AddForce(bullet.transform.forward * _bulletForce, ForceMode.Impulse);
            StartCoroutine(nameof(Shooting));
        }

        private void OnDrawGizmos()
        {
            if (!_detectionRadius)
                _detectionRadius = GetComponent<SphereCollider>();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius.radius);
        }
    }
}
