using UnityEngine;

namespace _game.Scripts
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelFrictionSetter : MonoBehaviour
    {
        [SerializeField] private CarController2 _carController;
        [SerializeField] private CarController2.Axel _axel;
        private CarParameters _carParameters;
        private WheelCollider _wheel;

        private void Start()
        {
            _wheel = GetComponent<WheelCollider>();
            _carParameters = _carController.CarParameters;
        }

        // static friction of the ground material.
        private void FixedUpdate()
        {
            if (!_wheel.GetGroundHit(out WheelHit hit)) return;

            float hitMaterialStaticFriction = hit.collider.material.staticFriction;

            //multiply wheel friction by surface material
            if (_axel == CarController2.Axel.Front)
            {
                WheelFrictionCurve fFriction = _carParameters.FrontWheelSettings.ForwardFriction.WheelFrictionCurve;
                fFriction.stiffness = hitMaterialStaticFriction * _carParameters.FrontWheelSettings.ForwardFriction.Stiffness;
                _wheel.forwardFriction = fFriction;
                
                WheelFrictionCurve sFriction = _carParameters.FrontWheelSettings.SidewaysFriction.WheelFrictionCurve;
                sFriction.stiffness = hitMaterialStaticFriction * _carParameters.FrontWheelSettings.SidewaysFriction.Stiffness;
                _wheel.sidewaysFriction = sFriction;
            }
            if (_axel == CarController2.Axel.Rear)
            {
                WheelFrictionCurve fFriction = _carParameters.RearWheelSettings.ForwardFriction.WheelFrictionCurve;               
                fFriction.stiffness = hitMaterialStaticFriction * _carParameters.RearWheelSettings.ForwardFriction.Stiffness;
                _wheel.forwardFriction = fFriction;
                
                WheelFrictionCurve sFriction = _carParameters.RearWheelSettings.SidewaysFriction.WheelFrictionCurve;
                sFriction.stiffness = hitMaterialStaticFriction * _carParameters.RearWheelSettings.SidewaysFriction.Stiffness;
                _wheel.sidewaysFriction = sFriction;
            }
        }
    }
}
