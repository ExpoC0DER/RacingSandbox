using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    //public static InputController Instance { get; private set; }
    private Controls _controls;
    private InputAction _movement;

    public static event Action<Vector2> InputAxisChanged;
    public static event Action RestartCheckpointPerformed;
    public static event Action OnShowEndScreen;
    public static event Action<bool> BreakingChanged; 


    private void Awake()
    {
        // if (Instance != null && Instance != this)
        //     Destroy(this);
        // else
        //     Instance = this;

        _controls = new Controls();
    }


    private void OnEnable()
    {
        _movement = _controls.Player.Movement;
        _controls.Player.RestartCheckpoint.performed += InvokeRestartOnCheckpoint;
        _controls.Player.Brake.started += InvokeBrake;
        _controls.Player.Brake.canceled += InvokeBrake;
        _controls.Player.OpenEndScreen.performed += InvokeOnShowEndScreen;
        _controls.Enable();
    }

    private static void InvokeOnShowEndScreen(InputAction.CallbackContext context)
    {
        OnShowEndScreen?.Invoke();
    }
    
    private static void InvokeRestartOnCheckpoint(InputAction.CallbackContext context)
    {
        RestartCheckpointPerformed?.Invoke();
    }
    private static void InvokeBrake(InputAction.CallbackContext context){ BreakingChanged?.Invoke(context.started);}

    private void OnDisable() { _controls.Disable(); }

    private void Update() { InputAxisChanged?.Invoke(_movement.ReadValue<Vector2>()); }
}
