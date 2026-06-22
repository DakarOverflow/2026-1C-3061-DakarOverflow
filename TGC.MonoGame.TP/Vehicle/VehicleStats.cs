using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;


public class VehicleStats
{
    public float MaxAcceleration;
    public float AccelerationRate;
    public float MaxSpeed;
    public float MinSpeed;
    public float BrakeForce;
    public float TurnSpeed;

    public float MaxHealth;
    public float FuelCapacity;
    public float FuelConsumption;
}