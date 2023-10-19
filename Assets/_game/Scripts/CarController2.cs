using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using NaughtyAttributes;

namespace _game.Scripts
{
    public class CarController2 : MonoBehaviour
    {
        public enum ControlMode
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

        [FormerlySerializedAs("control")]
        [SerializeField] private ControlMode _controlScheme;

        [SerializeField, Expandable] private CarParameters _carParameters;
        private float _maxAcceleration, _brakeAcceleration, _turnSensitivity, _maxSteerAngle;

        [SerializeField] private List<Wheel> _wheels;

        private float _moveInput, _steerInput;
        private Rigidbody _rb;
        [SerializeField] private TMP_Text _speedMeter;
        //private CarLights carLights;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            LoadParameters(_carParameters);
            //carLights = GetComponent<CarLights>();
        }

        void Update()
        {
            GetInputs();
            AnimateWheels();
            //WheelEffects();
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
            }
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
            if (Input.GetKey(KeyCode.Space)) // || moveInput == 0)
            {
                foreach (var wheel in _wheels)
                {
                    wheel.wheelCollider.brakeTorque = _brakeAcceleration;
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

        private void OnDrawGizmos() { Gizmos.DrawSphere(transform.TransformPoint(_carParameters.CenterOfMass), 0.1f); }

        private void OnEnable() { _carParameters.OnChange += LoadParameters; }
        private void OnDisable() { _carParameters.OnChange -= LoadParameters; }
    }
}
