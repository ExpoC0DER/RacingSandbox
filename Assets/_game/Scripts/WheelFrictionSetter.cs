using UnityEngine;

namespace _game.Scripts
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelFrictionSetter : MonoBehaviour
    {
        private WheelCollider _wheel;

        private float _originalSidewaysStiffness;
        private float _originalForwardStiffness;

        private void Start()
        {
            _wheel = GetComponent<WheelCollider>();

            _originalSidewaysStiffness = _wheel.sidewaysFriction.stiffness;
            _originalForwardStiffness = _wheel.forwardFriction.stiffness;
        }

        // static friction of the ground material.
        private void FixedUpdate()
        {
            if (!_wheel.GetGroundHit(out WheelHit hit)) return;

            float hitMaterialStaticFriction = hit.collider.material.staticFriction;

            WheelFrictionCurve fFriction = _wheel.forwardFriction;
            fFriction.stiffness = hitMaterialStaticFriction * _originalForwardStiffness;
            _wheel.forwardFriction = fFriction;


            WheelFrictionCurve sFriction = _wheel.sidewaysFriction;
            sFriction.stiffness = hitMaterialStaticFriction * _originalSidewaysStiffness;
            _wheel.sidewaysFriction = sFriction;
        }
    }
}
