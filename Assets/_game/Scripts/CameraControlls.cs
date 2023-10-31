using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace _game.Scripts
{
    public class CameraControlls : MonoBehaviour
    {
        [FormerlySerializedAs("_sensitivity")]
        [SerializeField] private float _mouseSensitivity = 1f;
        [SerializeField] private float _scrollSensitivity = 1f;
        [SerializeField] private float _keyboardSensitivity = 1f;
        private CinemachineVirtualCamera _editorCam;
        private Vector3 _previousPosition;

        private void Awake() { _editorCam = GetComponent<CinemachineVirtualCamera>(); }

        private void Update()
        {
            if (GameManager.GameState != GameState.Editing) return;

            //Calculate mouse delta
            Vector3 mousePosition = Input.mousePosition;
            Vector3 mouseDelta = mousePosition - _previousPosition;
            _previousPosition = mousePosition;

            if (Input.GetMouseButton(1))
            {
                transform.position -= Time.deltaTime * _mouseSensitivity * new Vector3(mouseDelta.x, 0, mouseDelta.y);
            }

            Vector2 keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * _keyboardSensitivity;
            float scrollInput = Input.mouseScrollDelta.y * -_scrollSensitivity;

            transform.position += new Vector3(keyboardInput.x, scrollInput, keyboardInput.y) * Time.deltaTime;
        }
    }
}
