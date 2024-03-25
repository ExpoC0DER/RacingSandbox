using System;
using _game.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Scriptable Objects/Car Parameters")]
public class CarParameters : ScriptableObject
{
    [Header("Driving Properties")]
    public CarController2.DriveType DriveType;
    public float MaxAcceleration;
    [FormerlySerializedAs("BrakeAccelaration")]
    public float BrakeAcceleration;
    public float TurnSensitivity, MaxSteerAngle, BoostPower;

    [Header("Physics Properties")]
    public Vector3 CenterOfMass;
    public float Weight, AirDrag;

    private static readonly WheelCurve DefaultForwardFriction = new WheelCurve(0.4f, 1f, 0.8f, 0.5f, 1f);
    private static readonly WheelCurve DefaultSidewaysFriction = new WheelCurve(0.2f, 1f, 0.5f, 0.75f, 1f);

    [Header("Wheel Properties")]
    public float Mass = 15f;
    public float WheelDampingRate = 0.25f, SuspensionDistance = 0.3f, ForceAppPointDistance;
    [Space] public WheelSettings FrontWheelSettings = new WheelSettings(DefaultForwardFriction, DefaultSidewaysFriction);
    [Space] public WheelSettings RearWheelSettings = new WheelSettings(DefaultForwardFriction, DefaultSidewaysFriction);
    public event Action<CarParameters> OnChange;

    [Serializable]
    public class WheelCurve
    {
        [field: SerializeField] public float ExtremumSlip { get; private set; }
        [field: SerializeField] public float ExtremumValue { get; private set; }
        [field: SerializeField] public float AsymptoteSlip { get; private set; }
        [field: SerializeField] public float AsymptoteValue { get; private set; }
        [field: SerializeField] public float Stiffness { get; private set; }

        public WheelFrictionCurve WheelFrictionCurve
        {
            get
            {
                WheelFrictionCurve newWheelFrictionCurve = new WheelFrictionCurve
                {
                    extremumSlip = ExtremumSlip,
                    extremumValue = ExtremumValue,
                    asymptoteSlip = AsymptoteSlip,
                    asymptoteValue = AsymptoteValue,
                    stiffness = Stiffness
                };
                return newWheelFrictionCurve;
            }
        }

        public WheelCurve(float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue, float stiffness)
        {
            ExtremumSlip = extremumSlip;
            ExtremumValue = extremumValue;
            AsymptoteSlip = asymptoteSlip;
            AsymptoteValue = asymptoteValue;
            Stiffness = stiffness;
        }
    }

    [Serializable]
    public class WheelSettings
    {
        [field: SerializeField] public WheelCurve ForwardFriction { get; private set; }
        [field: SerializeField] public WheelCurve SidewaysFriction { get; private set; }

        public WheelSettings(WheelCurve forwardFriction, WheelCurve sidewaysFriction)
        {
            ForwardFriction = forwardFriction;
            SidewaysFriction = sidewaysFriction;
        }
    }

    private void OnValidate() { OnChange?.Invoke(this); }
}
