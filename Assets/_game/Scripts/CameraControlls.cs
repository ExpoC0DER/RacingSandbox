using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
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

        private void Awake() { _editorCam = GetComponent<CinemachineVirtualCamera>(); }

        private void Update()
        {
            if (GameManager.GameState != GameState.Editing) return;
            
            if (_mouseCameraMoving)
            {
                transform.position -= Time.deltaTime * _mouseSensitivity * new Vector3(_mouseDelta.x, 0, _mouseDelta.y);
            }

            Vector2 keyboardInput = _cameraMoveDelta * _keyboardSensitivity;
            float scrollInput = _zoomDelta * _scrollSensitivity;
            
            transform.position += new Vector3(keyboardInput.x, scrollInput, keyboardInput.y) * Time.unscaledDeltaTime;
        }

        private Vector2 _mouseDelta;
        public void GetMouseDelta(InputAction.CallbackContext ctx) { _mouseDelta = ctx.ReadValue<Vector2>(); }

        private bool _mouseCameraMoving;
        public void GetMouseCameraMove(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                _mouseCameraMoving = true;
            if (ctx.canceled)
                _mouseCameraMoving = false;
        }

        private float _zoomDelta;
        public void GetZoom(InputAction.CallbackContext ctx) { _zoomDelta = ctx.ReadValue<float>(); }

        private Vector2 _cameraMoveDelta;
        public void CameraMove(InputAction.CallbackContext ctx) { _cameraMoveDelta = ctx.ReadValue<Vector2>(); }
    }
}
