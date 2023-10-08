using System;
using System.Collections;
using System.Collections.Generic;
using _game.Scripts;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CarController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _carCam;
    [SerializeField] private Transform _followPoint;
    [SerializeField] private float _rewindPosMulti, _rewindRotMulti;
    private Rigidbody _rb;

    private float _horizontalInput, _verticalInput;
    private float _currentSteerAngle, _currentBreakForce;
    private bool _isBreaking, _isPlaying, _isRewinding, _isResetting;
    [SerializeField] private bool _testing;
    private CarTransform _startTransform;
    private readonly Stack<CarTransform> _positions = new();

    // Settings
    [SerializeField] private float _motorForce, _breakForce, _maxSteerAngle, _finishBreakForce;

    // Wheel Colliders
    [SerializeField] private WheelCollider _frontLeftWheelCollider, _frontRightWheelCollider, _rearLeftWheelCollider, _rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform _frontLeftWheelTransform, _frontRightWheelTransform, _rearLeftWheelTransform, _rearRightWheelTransform;

    private void Awake() { _rb = GetComponent<Rigidbody>(); }

    private void Start()
    {
        if (_testing)
            GameManager.GameState = GameState.Playing;
        _startTransform = new(transform);
    }

    private void FixedUpdate()
    {
        if (!_isPlaying)
        {
            ApplyBreaking(float.MaxValue);
            return;
        }

        HandleMotor();
        HandleSteering();
        UpdateWheels();
        HandleStopping();
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        GetInput();
        HandleRewinding();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            GameManager.GameState = GameState.Editing;
        }
    }

    private void GetInput()
    {
        // Steering Input
        _horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration Input
        _verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        _isBreaking = Input.GetKey(KeyCode.Space);

        _isRewinding = Input.GetKey(KeyCode.R);

        if (Input.GetKeyDown(KeyCode.C) && !_isResetting)
            Reset();
    }

    private void Reset() { StartCoroutine(nameof(ResetCoroutine)); }
    private IEnumerator ResetCoroutine()
    {
        bool tempIsPlaying = _isPlaying;
        _isPlaying = true;

        _isResetting = true;
        transform.SetPositionAndRotation(_startTransform.Position, _startTransform.Rotation);
        _positions.Clear();
        _positions.Push(_startTransform);

        yield return new WaitForSeconds(.5f);
        _isResetting = false;
        _isPlaying = tempIsPlaying;
    }

    private void HandleRewinding()
    {
        if (_isRewinding)
        {
            if (_positions.Count > 1 && Vector3.Distance(transform.position, _positions.Peek().Position) < .1f)
                _positions.Pop();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _positions.Peek().Rotation, Time.deltaTime * _rewindRotMulti);
            transform.position = Vector3.MoveTowards(transform.position, _positions.Peek().Position, Time.deltaTime * _rewindPosMulti);
        }
        else
        {
            if (Vector3.Distance(transform.position, _positions.Peek().Position) > 0.5f)
                _positions.Push(new(transform));
        }
    }

    private void HandleStopping()
    {
        if (_isResetting || _isRewinding)
        {
            _rb.isKinematic = true;
            _frontRightWheelCollider.rotationSpeed = 0;
            _frontLeftWheelCollider.rotationSpeed = 0;
            _rearLeftWheelCollider.rotationSpeed = 0;
            _rearRightWheelCollider.rotationSpeed = 0;
        }
        else
        {
            _rb.isKinematic = false;
        }
    }

    private void HandleMotor()
    {
        _frontLeftWheelCollider.motorTorque = _verticalInput * _motorForce;
        _frontRightWheelCollider.motorTorque = _verticalInput * _motorForce;
        _currentBreakForce = _isBreaking ? _breakForce : 0f;
        ApplyBreaking(_currentBreakForce);
    }

    private void ApplyBreaking(float value)
    {
        _frontRightWheelCollider.brakeTorque = value;
        _frontLeftWheelCollider.brakeTorque = value;
        _rearLeftWheelCollider.brakeTorque = value;
        _rearRightWheelCollider.brakeTorque = value;
    }

    private void HandleSteering()
    {
        _currentSteerAngle = _maxSteerAngle * _horizontalInput;
        _frontLeftWheelCollider.steerAngle = _currentSteerAngle;
        _frontRightWheelCollider.steerAngle = _currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(_frontLeftWheelCollider, _frontLeftWheelTransform);
        UpdateSingleWheel(_frontRightWheelCollider, _frontRightWheelTransform);
        UpdateSingleWheel(_rearRightWheelCollider, _rearRightWheelTransform);
        UpdateSingleWheel(_rearLeftWheelCollider, _rearLeftWheelTransform);
    }

    private static void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelTransform.SetPositionAndRotation(pos, rot);
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Playing)
        {
            _startTransform = new(transform);
            SwitchCamera(true);

            _rb.isKinematic = false;
            _isPlaying = true;
            _positions.Push(new(transform));
        }
        else
        {
            SwitchCamera(false);
            _isPlaying = false;
            Reset();
        }
    }

    private void SwitchCamera(bool value)
    {
        if (value)
        {
            _carCam.m_Follow = _followPoint;
            _carCam.m_LookAt = _followPoint;
            _carCam.Priority = 20;
        }
        else
        {
            _carCam.m_Follow = null;
            _carCam.m_LookAt = null;
            _carCam.Priority = 0;
        }
    }

    private void OnEnable() { GameManager.OnGameStateChanged += OnGameStateChanged; }
    private void OnDisable() { GameManager.OnGameStateChanged -= OnGameStateChanged; }
}

public struct CarTransform
{
    public Vector3 Position;
    public Quaternion Rotation;
    public CarTransform(Transform t)
    {
        Position = t.position;
        Rotation = t.rotation;
    }
}
