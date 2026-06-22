using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public static class VehiclePresets
{
    public static VehicleStats Light =
        new VehicleStats()
        {
            MaxAcceleration = 700f,
            AccelerationRate = 8f,
            MaxSpeed = 2000f,
            MinSpeed = -1000f,
            BrakeForce = 1000f,
            TurnSpeed = 2.5f,
            MaxHealth = 50f,
            FuelCapacity = 80f,
            FuelConsumption = 2.0f
        };

    public static VehicleStats Medium =
        new VehicleStats()
        {
            MaxAcceleration = 400f,
            AccelerationRate = 5f,
            MaxSpeed = 1500f,
            MinSpeed = -750f,
            BrakeForce = 700f,
            TurnSpeed = 2f,
            MaxHealth = 100f,
            FuelCapacity = 125f,
            FuelConsumption = 1.0f
        };

    public static VehicleStats Heavy =
        new VehicleStats()
        {
            MaxAcceleration = 250f,
            AccelerationRate = 3f,
            MaxSpeed = 1000f,
            MinSpeed = -500f,
            BrakeForce = 550f,
            TurnSpeed = 1.75f,
            MaxHealth = 250f,
            FuelCapacity = 220f,
            FuelConsumption = 0.4f
        };
}