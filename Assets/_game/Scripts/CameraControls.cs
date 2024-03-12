using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _game.Scripts
{
    public class CameraControls : MonoBehaviour
    {
        [FormerlySerializedAs("_sensitivity")]
        [SerializeField, MinMaxSlider(0, 1000)]
        private Vector2 _zoomMinMax;
        [SerializeField] private float _mouseSensitivity = 1f;
        [SerializeField] private float _scrollSensitivity = 1f;
        [SerializeField] private float _keyboardSensitivity = 1f;

        private void Update()
        {
            if (GameManager.GameState != GameState.Editing) return;

            Vector3 newPosition = transform.position;

            if (_mouseCameraMoving)
            {
                newPosition -= Time.unscaledDeltaTime * _mouseSensitivity * new Vector3(_mouseDelta.x, 0, _mouseDelta.y);
            }

            Vector2 keyboardInput = _cameraMoveDelta * (_keyboardSensitivity * newPosition.y);
            float scrollInput = _zoomDelta * _scrollSensitivity;

            newPosition += new Vector3(keyboardInput.x, scrollInput, keyboardInput.y) * Time.unscaledDeltaTime;
            newPosition.y = Mathf.Clamp(newPosition.y, _zoomMinMax.x, _zoomMinMax.y);
            transform.position = newPosition;
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
