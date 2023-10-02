using Cinemachine;
using UnityEngine;

namespace _game.Scripts
{
    public class CameraControlls : MonoBehaviour
    {
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _scrollSensitivity = 1f;
        private CinemachineVirtualCamera _editorCam;
        private Vector3 _previousPosition;

        private void Awake()
        {
            _editorCam = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            //Calculate mouse delta
            Vector3 mousePosition = Input.mousePosition;
            Vector3 mouseDelta = mousePosition - _previousPosition;
            _previousPosition = mousePosition;

            if (Input.GetMouseButton(1))
            {
                transform.position -= Time.deltaTime * _sensitivity * new Vector3(mouseDelta.x, 0, mouseDelta.y);
            }

            transform.position -= new Vector3(0, Input.mouseScrollDelta.y * _scrollSensitivity * Time.deltaTime, 0);
        }
    }
}
