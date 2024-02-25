using System;
using _game.Scripts;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Car Parameters")]
public class CarParameters : ScriptableObject
{
    [Header("Driving Properties"), Space]
    public float MaxAcceleration;
    public float BrakeAccelaration, TurnSensitivity, MaxSteerAngle, BoostPower;
    [Header("Physics Properties"), Space]
    public Vector3 CenterOfMass;
    public float Weight, AirDrag, WheelForwardFriction, WheelSidewaysFriction;

    public event Action<CarParameters> OnChange;

    private void OnValidate() { OnChange?.Invoke(this); }
}
