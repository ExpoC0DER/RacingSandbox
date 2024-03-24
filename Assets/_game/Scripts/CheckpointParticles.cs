using UnityEngine;
namespace _game.Scripts
{
    public class CheckpointParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystemLeft;
        [SerializeField] private ParticleSystem _particleSystemRight;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                _particleSystemLeft.Play();
                _particleSystemRight.Play();
            }
        }
    }
}
