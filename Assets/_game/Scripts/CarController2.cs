using System;
using System.Collections;
using System.Collections.Generic;
using _game.Scripts.HelperScripts;
using _game.Scripts.UIScripts;
using Cinemachine;
using TMPro;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;

namespace _game.Scripts
{
    public class CarController2 : MonoBehaviour
    {
        private enum ControlMode
        {
            Keyboard,
            Buttons
        };

        public enum Axel
        {
            Front,
            Rear
        }

        public enum DriveType
        {
            FourWheelDrive,
            FrontWheelDrive,
            RearWheelDrive
        }

        [Serializable]
        public struct Wheel
        {
            public GameObject wheelModel;
            public WheelCollider wheelCollider;
            public GameObject wheelEffectObj;
            public ParticleSystem smokeParticle;
            public Axel axel;
        }

        // [SerializeField] private ControlMode _controlScheme;

        [field: SerializeField, Expandable] public CarParameters CarParameters { get; private set; }
        private DriveType _driveType;
        private float _maxAcceleration, _brakeAcceleration, _turnSensitivity, _maxSteerAngle, _boostPower;
        private FMODUnity.StudioEventEmitter _engineSound;
        [SerializeField] private bool _isTesting;

        [SerializeField] private List<Wheel> _wheels;

        private float _moveInput, _steerInput;
        private Rigidbody _rb;
        [SerializeField] private TMP_Text _speedMeter;

        private CarTransform _restartPosition, _checkpointPosition;
        private bool _isPlaying, _isResetting, _isBreaking;
        [SerializeField] private CinemachineVirtualCamera _carCam;
        [SerializeField] private Transform _followPoint;
        private float _countdownDelay;
        private int _lapCounter;

        public static event Action<bool> SetTimerActive;
        public static event Action<CarController2> ShowEndScreen;
        public static event Action<int, float, Action> StartCountdown;
        public static event Action<int> LapPassed;
        //private CarLights carLights;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            LoadParameters(CarParameters);
            _restartPosition = new(transform);
            //carLights = GetComponent<CarLights>();
            _engineSound = GetComponent<FMODUnity.StudioEventEmitter>();

            _countdownDelay = Camera.main!.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time;

            _speedMeter = GameObject.Find("Speed").GetComponent<TMP_Text>();

            if (_isTesting)
            {
                _restartPosition = new(transform);
                SwitchCamera(true);

                _rb.isKinematic = false;
                _isPlaying = true;
            }
        }

        private void Update()
        {
            AnimateWheels();
            HandleAudio();
            //WheelEffects();
            if (_speedMeter)
                _speedMeter.text = Mathf.Round(_rb.velocity.magnitude * 3.6f) + " km/h";

            if (_rb.position.y < -10 && !_isResetting)
                Reset(_checkpointPosition);
        }

        private void FixedUpdate()
        {
            _followPoint.SetPositionAndRotation(transform.position + new Vector3(0f, 0.586f, -0.1f), transform.rotation);
            if (!_isPlaying) return;
            Move();
            Steer();
            Brake();
        }

        private void HandleAudio()
        {
            float speed = (_rb.velocity.magnitude * 3.6f).Remap(0, 100, 0, 1);
            _engineSound.SetParameter("RPM", speed);
            _engineSound.SetParameter("Accel", _moveInput);
        }

        private void Move()
        {
            foreach (Wheel wheel in _wheels)
            {
                switch (_driveType)
                {
                    case DriveType.FourWheelDrive:
                        break;
                    case DriveType.FrontWheelDrive:
                        if(wheel.axel != Axel.Front)
                            continue;
                        break;
                    case DriveType.RearWheelDrive:
                        if(wheel.axel != Axel.Rear)
                            continue;
                        break;
                    default:
                        return;
                }
                wheel.wheelCollider.motorTorque = _moveInput * _maxAcceleration;
            }
        }

        private void Steer()
        {
            foreach (Wheel wheel in _wheels)
            {
                if (wheel.axel == Axel.Front)
                {
                    float steerAngle = _steerInput * _turnSensitivity * _maxSteerAngle;
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, 0.6f);
                }
            }
        }

        private void Brake()
        {
            if (_isBreaking || _isResetting) // || moveInput == 0)
            {
                foreach (Wheel wheel in _wheels)
                {
                    wheel.wheelCollider.brakeTorque = _isResetting ? float.MaxValue : _brakeAcceleration;
                }

                // carLights.isBackLightOn = true;
                // carLights.OperateBackLights();
            }
            else
            {
                foreach (Wheel wheel in _wheels)
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }

                // carLights.isBackLightOn = false;
                // carLights.OperateBackLights();
            }
        }

        private void AnimateWheels()
        {
            foreach (Wheel wheel in _wheels)
            {
                wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
                wheel.wheelModel.transform.position = pos;
                wheel.wheelModel.transform.rotation = rot;
            }
        }

        private void WheelEffects()
        {
            foreach (Wheel wheel in _wheels)
            {
                //var dirtParticleMainSettings = wheel.smokeParticle.main;

                if (_isBreaking && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded && _rb.velocity.magnitude >= 10.0f)
                {
                    wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                    wheel.smokeParticle.Emit(1);
                }
                else
                {
                    wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
                }
            }
        }

        private void LoadParameters(CarParameters carParams)
        {
            _driveType = carParams.DriveType;
            _maxAcceleration = carParams.MaxAcceleration;
            _brakeAcceleration = carParams.BrakeAcceleration;
            _turnSensitivity = carParams.TurnSensitivity;
            _maxSteerAngle = carParams.MaxSteerAngle;
            _boostPower = carParams.BoostPower;

            _rb.mass = carParams.Weight;
            _rb.centerOfMass = carParams.CenterOfMass;
            _rb.drag = carParams.AirDrag;

            foreach (Wheel wheel in _wheels)
            {
                wheel.wheelCollider.mass = carParams.Mass;
                wheel.wheelCollider.wheelDampingRate = carParams.WheelDampingRate;
                wheel.wheelCollider.suspensionDistance = carParams.SuspensionDistance;
                wheel.wheelCollider.forceAppPointDistance = carParams.ForceAppPointDistance;

                if (wheel.axel == Axel.Front)
                {
                    wheel.wheelCollider.forwardFriction = carParams.FrontWheelSettings.ForwardFriction.WheelFrictionCurve;
                    wheel.wheelCollider.sidewaysFriction = carParams.FrontWheelSettings.SidewaysFriction.WheelFrictionCurve;
                }
                
                if (wheel.axel == Axel.Rear)
                {
                    wheel.wheelCollider.forwardFriction = carParams.RearWheelSettings.ForwardFriction.WheelFrictionCurve;
                    wheel.wheelCollider.sidewaysFriction = carParams.RearWheelSettings.SidewaysFriction.WheelFrictionCurve;
                }
            }
        }
        private IEnumerator ResetCoroutine(CarTransform pos)
        {
            bool tempIsPlaying = _isPlaying;
            _isPlaying = true;

            _isResetting = true;
            _rb.isKinematic = true;
            transform.SetPositionAndRotation(pos.Position, pos.Rotation);

            yield return new WaitForSeconds(.5f);
            _isResetting = false;
            _rb.isKinematic = false;
            _isPlaying = tempIsPlaying;
        }

        private void Reset(CarTransform pos) { StartCoroutine(nameof(ResetCoroutine), pos); }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Playing)
            {
                _restartPosition = new(transform);
                _checkpointPosition = _restartPosition;
                SwitchCamera(true);

                _rb.isKinematic = false;
                StartCountdown?.Invoke(3, _countdownDelay, StartPlaying);
            }
            else
            {
                _rb.isKinematic = true;
                SwitchCamera(false);
                _isPlaying = false;
                Reset(_restartPosition);
            }
        }

        //Triggered pressing restart on endscreen
        public void RestartLevel()
        {
            _isPlaying = false;
            Reset(_restartPosition);
            StartCountdown?.Invoke(3, 0f, StartPlaying);
            LapPassed?.Invoke(0);
        }

        //Enable playingF
        private void StartPlaying()
        {
            _lapCounter = 0;
            _isPlaying = true;
            SetTimerActive?.Invoke(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                _checkpointPosition.SetPositionAndRotation(other.transform);
            }
            if (other.CompareTag("Boost"))
            {
                _rb.AddForce(transform.forward * _boostPower, ForceMode.VelocityChange);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isPlaying) return;

            if (other.CompareTag("Finish"))
            {
                Finish();
            }

            if (other.CompareTag("Lap"))
            {
                _lapCounter++;
                LapPassed?.Invoke(_lapCounter);
                if (_lapCounter > 3)
                    Finish();
            }
        }

        private void Finish()
        {
            _isPlaying = false;
            foreach (Wheel wheel in _wheels)
            {
                wheel.wheelCollider.brakeTorque = float.MaxValue;
            }
            SetTimerActive?.Invoke(false);
            ShowEndScreen?.Invoke(this);
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

        private void OnDrawGizmos() { Gizmos.DrawSphere(transform.TransformPoint(CarParameters.CenterOfMass), 0.1f); }

        private void ShowEndScreenShortcut()
        {
            if (!_isPlaying) return;

            _isPlaying = false;

            SetTimerActive?.Invoke(false);
            ShowEndScreen?.Invoke(this);
        }

        private void OnPause(bool value)
        {
            //_isPlaying = !value;
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
            PauseScreen.OnGamePause += OnPause;
            PauseScreen.OnGameRestart += RestartLevel;
            CarParameters.OnChange += LoadParameters;
            //InputController.OnShowEndScreen += ShowEndScreenShortcut;
        }
        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            PauseScreen.OnGamePause -= OnPause;
            PauseScreen.OnGameRestart -= RestartLevel;
            CarParameters.OnChange -= LoadParameters;
            //InputController.OnShowEndScreen -= ShowEndScreenShortcut;
        }

        public void GetAxisInput(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            _steerInput = input.x;
            _moveInput = input.y;
        }

        public void GetBreaking(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                _isBreaking = true;
            if (ctx.canceled)
                _isBreaking = false;
        }

        public void GetRestartingFromCheckpoint(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && !_isResetting)
                Reset(_checkpointPosition);
        }
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

        public void SetPositionAndRotation(Transform t)
        {
            Position = t.position;
            Rotation = t.rotation;
        }
    }
}
