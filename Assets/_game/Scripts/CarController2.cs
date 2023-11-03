using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using NaughtyAttributes;

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

        [Serializable]
        public struct Wheel
        {
            public GameObject wheelModel;
            public WheelCollider wheelCollider;
            public GameObject wheelEffectObj;
            public ParticleSystem smokeParticle;
            public Axel axel;
        }

        [SerializeField] private ControlMode _controlScheme;

        [SerializeField, Expandable] private CarParameters _carParameters;
        private float _maxAcceleration, _brakeAcceleration, _turnSensitivity, _maxSteerAngle;
        private FMODUnity.StudioEventEmitter _engineSound;
        [SerializeField] private bool _isTesting;

        [SerializeField] private List<Wheel> _wheels;

        private float _moveInput, _steerInput;
        private Rigidbody _rb;
        [SerializeField] private TMP_Text _speedMeter;

        private CarTransform _restartPosition, _checkpointPosition;
        private bool _isPlaying, _isResetting;
        [SerializeField] private CinemachineVirtualCamera _carCam;
        [SerializeField] private Transform _followPoint;
        //private CarLights carLights;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            LoadParameters(_carParameters);
            _restartPosition = new(transform);
            //carLights = GetComponent<CarLights>();
            _engineSound = GetComponent<FMODUnity.StudioEventEmitter>();

            if (_isTesting)
            {
                _restartPosition = new(transform);
                SwitchCamera(true);

                _rb.isKinematic = false;
                _isPlaying = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Checkpoint"))
            {
                _checkpointPosition.Position = other.transform.position;
                _checkpointPosition.Rotation = other.transform.rotation;
            }
        }

        void Update()
        {
            GetInputs();
            AnimateWheels();

            HandleAudio();
            //WheelEffects();
            if (_speedMeter)
                _speedMeter.text = Mathf.Round(_rb.velocity.magnitude * 3.6f) + " km/h";
        }

        private void FixedUpdate()
        {
            Move();
            Steer();
            Brake();
        }

        public void MoveInput(float input) { _moveInput = input; }

        public void SteerInput(float input) { _steerInput = input; }

        void GetInputs()
        {
            if (_controlScheme == ControlMode.Keyboard)
            {
                _moveInput = Input.GetAxis("Vertical");
                _steerInput = Input.GetAxis("Horizontal");

                if (Input.GetKeyDown(KeyCode.C) && !_isResetting)
                    Reset(_checkpointPosition);
            }
        }

        private void HandleAudio()
        {
            float speed = (_rb.velocity.magnitude * 3.6f).Remap(0, 200, 0, 1);
            _engineSound.SetParameter("RPM", speed);
            _engineSound.SetParameter("Accel", _moveInput);
        }

        void Move()
        {
            foreach (var wheel in _wheels)
            {
                wheel.wheelCollider.motorTorque = _moveInput * _maxAcceleration;
            }
        }

        void Steer()
        {
            foreach (var wheel in _wheels)
            {
                if (wheel.axel == Axel.Front)
                {
                    float _steerAngle = _steerInput * _turnSensitivity * _maxSteerAngle;
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
                }
            }
        }

        void Brake()
        {
            if (Input.GetKey(KeyCode.Space) || _isResetting) // || moveInput == 0)
            {
                foreach (var wheel in _wheels)
                {
                    wheel.wheelCollider.brakeTorque = _isResetting ? float.MaxValue : _brakeAcceleration;
                }

                // carLights.isBackLightOn = true;
                // carLights.OperateBackLights();
            }
            else
            {
                foreach (var wheel in _wheels)
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }

                // carLights.isBackLightOn = false;
                // carLights.OperateBackLights();
            }
        }

        void AnimateWheels()
        {
            foreach (var wheel in _wheels)
            {
                Quaternion rot;
                Vector3 pos;
                wheel.wheelCollider.GetWorldPose(out pos, out rot);
                wheel.wheelModel.transform.position = pos;
                wheel.wheelModel.transform.rotation = rot;
            }
        }

        void WheelEffects()
        {
            foreach (var wheel in _wheels)
            {
                //var dirtParticleMainSettings = wheel.smokeParticle.main;

                if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded && _rb.velocity.magnitude >= 10.0f)
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
            _maxAcceleration = carParams.MaxAcceleration;
            _brakeAcceleration = carParams.BrakeAccelaration;
            _turnSensitivity = carParams.TurnSensitivity;
            _maxSteerAngle = carParams.MaxSteerAngle;

            _rb.mass = carParams.Weight;
            _rb.centerOfMass = carParams.CenterOfMass;
            _rb.drag = carParams.AirDrag;

            foreach (Wheel wheel in _wheels)
            {
                WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
                forwardFriction.stiffness = carParams.WheelForwardFriction;
                wheel.wheelCollider.forwardFriction = forwardFriction;

                WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = carParams.WheelSidewaysFriction;
                wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
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
                _isPlaying = true;
            }
            else
            {
                _rb.isKinematic = true;
                SwitchCamera(false);
                _isPlaying = false;
                Reset(_restartPosition);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Finish"))
            {
                GameManager.GameState = GameState.Editing;
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

        private void OnDrawGizmos() { Gizmos.DrawSphere(transform.TransformPoint(_carParameters.CenterOfMass), 0.1f); }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
            _carParameters.OnChange += LoadParameters;
        }
        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            _carParameters.OnChange -= LoadParameters;
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
    }
}
