using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;


// Dataclass
public class VehicleStats
{
    public float MaxSpeed;
    public float MinSpeed;
    public float Acceleration;
    public float BrakeForce;
    public float TurnSpeed;

    public float MaxHealth;
    public float FuelCapacity;
    public float FuelConsumption;
}