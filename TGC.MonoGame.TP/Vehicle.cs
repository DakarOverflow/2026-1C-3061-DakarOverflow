using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public enum VehicleType
{
    Light,
    Medium,
    Heavy
}

public class Vehicle
{
    private readonly CustomModel _model;

    public Vector3 Position;
    public float RotationY;
    private const float ModelRotationOffset = MathHelper.Pi;

    // Stats
    private readonly VehicleStats _stats;

    public float _speed;
    private const float Friction = 150f;

    public VehicleType Type { get; }

    public Vehicle(CustomModel model, Vector3 initialPosition, VehicleStats stats, VehicleType type)
    {
        _model = model;

        Position = initialPosition;

        _stats = stats;

        Type = type;

        RotationY = 0f;

        _speed = 0f;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        KeyboardState keyboard = Keyboard.GetState();

        // =========================
        // ACELERAR
        // =========================

        if (keyboard.IsKeyDown(Keys.W))
        {
            if (_speed < 0f) _speed += _stats.BrakeForce * deltaTime;
            else _speed += _stats.Acceleration * deltaTime;
        }
        else
        {
            // desaceleracion natural
            if (_speed > 0f) _speed -= Friction * deltaTime;
            else if (_speed < 0f) _speed += Friction * deltaTime;
            else if (_speed == 0f) _speed = 0;
        }

        // =========================
        // FRENAR
        // =========================

        if (keyboard.IsKeyDown(Keys.S))
        {
            _speed -= _stats.BrakeForce * deltaTime;
        }

        // =========================
        // LIMITES VELOCIDAD
        // =========================

        _speed = MathHelper.Clamp(_speed, _stats.MinSpeed, _stats.MaxSpeed);

        // =========================
        // GIRAR
        // =========================

        if (_speed > 5f)
        {
            float steeringDirection = _speed >= 0f ? 1f : -1f;

            if (keyboard.IsKeyDown(Keys.A))
            {
                RotationY += _stats.TurnSpeed * steeringDirection * deltaTime;
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                RotationY -= _stats.TurnSpeed * steeringDirection * deltaTime;
            }
        }

        // =========================
        // AVANZAR
        // =========================

        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(RotationY));

        Position += forward * _speed * deltaTime;
    }

    public Matrix GetWorld()
    {
        return
            Matrix.CreateRotationY(RotationY) *
            Matrix.CreateTranslation(Position);
    }

    public Matrix GetVisualWorld()
    {
        return
            Matrix.CreateRotationY(RotationY + ModelRotationOffset) *
            Matrix.CreateTranslation(Position);
    }

    public void Draw(GameTime gameTime, Camera camera)
    {
        _model.Draw(
            GetVisualWorld(),
            camera.GetView(),
            camera.GetProjection()
        );
    }
}