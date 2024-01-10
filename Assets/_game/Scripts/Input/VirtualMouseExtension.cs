using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;


namespace _game.Scripts.Input
{
    public class VirtualMouseExtension : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvasRectTransform;
        [SerializeField] private RectTransform _mouseImage;
        private VirtualMouseInput _virtualMouseInput;

        private void Awake() { _virtualMouseInput = GetComponent<VirtualMouseInput>(); }

        private void Update()
        {
            float canvasScale = _canvasRectTransform.localScale.x;
            transform.localScale = Vector3.one * 1f / canvasScale;
            _mouseImage.localScale = Vector3.one * canvasScale;
            transform.SetAsLastSibling();
        }

        private void LateUpdate()
        {
            Vector2 virtualMousePosition = _virtualMouseInput.virtualMouse.position.value;
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 0f, Screen.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 0f, Screen.height);
            InputState.Change(_virtualMouseInput.virtualMouse, virtualMousePosition);
        }
    }
}
