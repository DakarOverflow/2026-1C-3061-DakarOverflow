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
            MaxSpeed = 2000f,
            MinSpeed = -1000f,
            Acceleration = 700f,
            BrakeForce = 1000f,
            TurnSpeed = 2.5f,
            MaxHealth = 50f,
            FuelCapacity = 60f,
            FuelConsumption = 2.0f
        };

    public static VehicleStats Medium =
        new VehicleStats()
        {
            MaxSpeed = 1500f,
            MinSpeed = -750f,
            Acceleration = 400f,
            BrakeForce = 700f,
            TurnSpeed = 2f,
            MaxHealth = 100f,
            FuelCapacity = 100f,
            FuelConsumption = 1.0f
        };

    public static VehicleStats Heavy =
        new VehicleStats()
        {
            MaxSpeed = 1000f,
            MinSpeed = -500f,
            Acceleration = 250f,
            BrakeForce = 550f,
            TurnSpeed = 1.25f,
            MaxHealth = 250f,
            FuelCapacity = 220f,
            FuelConsumption = 0.4f
        };
}