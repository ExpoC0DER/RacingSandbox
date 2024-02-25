using UnityEngine;

namespace _game.Scripts
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelFrictionSetter : MonoBehaviour
    {
        [SerializeField] private CarController2 _carController;
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
            WheelFrictionCurve fFriction = _wheel.forwardFriction;
            fFriction.stiffness = hitMaterialStaticFriction * _carParameters.WheelForwardFriction;
            _wheel.forwardFriction = fFriction;
            
            WheelFrictionCurve sFriction = _wheel.sidewaysFriction;
            sFriction.stiffness = hitMaterialStaticFriction * _carParameters.WheelSidewaysFriction;
            _wheel.sidewaysFriction = sFriction;
        }
    }
}
