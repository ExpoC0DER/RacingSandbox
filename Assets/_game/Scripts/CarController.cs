using System.Collections;
using System.Collections.Generic;
using _game.Scripts;
using UnityEngine;
using Cinemachine;

public class CarController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _carCam;
    private Rigidbody _rb;

    private float _horizontalInput, _verticalInput;
    private float _currentSteerAngle, _currentbreakForce;
    private bool _isBreaking, _isPlaying;
    [SerializeField] private bool _testing;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (_testing)
            OnPlay();
    }

    private void FixedUpdate()
    {
        if (!_isPlaying)
            return;

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // Steering Input
        _horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        _verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        _isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = _verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = _verticalInput * motorForce;
        _currentbreakForce = _isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = _currentbreakForce;
        frontLeftWheelCollider.brakeTorque = _currentbreakForce;
        rearLeftWheelCollider.brakeTorque = _currentbreakForce;
        rearRightWheelCollider.brakeTorque = _currentbreakForce;
    }

    private void HandleSteering()
    {
        _currentSteerAngle = maxSteerAngle * _horizontalInput;
        frontLeftWheelCollider.steerAngle = _currentSteerAngle;
        frontRightWheelCollider.steerAngle = _currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.SetPositionAndRotation(pos, rot);
    }

    private void OnPlay()
    {
        _carCam.Priority = 20;
        _rb.isKinematic = false;
        _isPlaying = true;
    }

    void OnEnable()
    {
        GameManager.PressPlay += OnPlay;
    }
    void OnDisable()
    {
        GameManager.PressPlay -= OnPlay;
    }
}
