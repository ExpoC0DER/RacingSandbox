using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        public ControlMode control;

        public float maxAcceleration = 30.0f;
        public float brakeAcceleration = 50.0f;

        public float turnSensitivity = 1.0f;
        public float maxSteerAngle = 30.0f;

        public Vector3 _centerOfMass;

        public List<Wheel> wheels;

        float moveInput;
        float steerInput;

        private Rigidbody _rb;
        [SerializeField] private TMP_Text _speedMeter;

        //private CarLights carLights;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = _centerOfMass;

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

        public void MoveInput(float input) { moveInput = input; }

        public void SteerInput(float input) { steerInput = input; }

        void GetInputs()
        {
            if (control == ControlMode.Keyboard)
            {
                moveInput = Input.GetAxis("Vertical");
                steerInput = Input.GetAxis("Horizontal");
            }
        }

        void Move()
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = moveInput * maxAcceleration;
            }
        }

        void Steer()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.axel == Axel.Front)
                {
                    float _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
                }
            }
        }

        void Brake()
        {
            if (Input.GetKey(KeyCode.Space) || moveInput == 0)
            {
                foreach (var wheel in wheels)
                {
                    wheel.wheelCollider.brakeTorque = brakeAcceleration;
                }

                // carLights.isBackLightOn = true;
                // carLights.OperateBackLights();
            }
            else
            {
                foreach (var wheel in wheels)
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }

                // carLights.isBackLightOn = false;
                // carLights.OperateBackLights();
            }
        }

        void AnimateWheels()
        {
            foreach (var wheel in wheels)
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
            foreach (var wheel in wheels)
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
    }
}
