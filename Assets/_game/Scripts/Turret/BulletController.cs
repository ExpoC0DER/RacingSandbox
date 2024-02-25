using System;
using UnityEngine;

namespace _game.Scripts.Turret
{
    public class BulletController : MonoBehaviour
    {
        private void Start() { Invoke(nameof(DestroyMyself), 10f); }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Car") && other.gameObject.TryGetComponent(out Rigidbody rb))
                rb.velocity *= .5f;
            DestroyMyself();
        }

        private void DestroyMyself() { Destroy(gameObject); }
    }
}
