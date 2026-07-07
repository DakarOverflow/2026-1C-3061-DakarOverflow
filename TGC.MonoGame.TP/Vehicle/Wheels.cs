using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class Wheel
{
    private readonly CustomModel _model;

    public Vector3 LocalPosition;

    public float SpinRotation;

    public float SteeringRotation;

    public bool Detached;

    public Vector3 WorldPosition;

    public Vector3 Velocity;

    public float VehicleRotation;

    private readonly bool _isRightWheel;

    public float _scaleFactor;

    public Wheel(CustomModel model, Vector3 localPosition, bool isRightWheel, float scaleFactor)
    {
        _model = model;
        LocalPosition = localPosition;

        _isRightWheel = isRightWheel;

        _scaleFactor = scaleFactor;
    }

    public void Detach(Vector3 initialVelocity)
    {
        Detached = true;
        Velocity = initialVelocity;
    }

    public void Update(GameTime gameTime, Matrix vehicleWorld)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (!Detached)
        {
            WorldPosition = Vector3.Transform(LocalPosition, vehicleWorld);
        }
        else
        {
            Velocity += Vector3.Down * 900f * dt;

            WorldPosition += Velocity * dt;

            SpinRotation += 10f * dt;
        }
    }

    private Matrix GetWorld()
    {
        Matrix mirror =
            _isRightWheel
            ? Matrix.CreateRotationY(MathHelper.Pi)
            : Matrix.Identity;

        Matrix world = 
            mirror *
            Matrix.CreateScale(_scaleFactor) *
            Matrix.CreateRotationX(SpinRotation) * 
            Matrix.CreateRotationY(SteeringRotation) * 
            Matrix.CreateRotationY(VehicleRotation) *
            Matrix.CreateTranslation(WorldPosition);

        return world;
    }

    public void DrawShadow(Effect shadowEffect, Matrix lightViewProjection, ShadowDiagnostics diagnostics = null)
    {
        _model.DrawManyShadow(new[] { GetWorld() }, shadowEffect, lightViewProjection, diagnostics);
    }

    public void Draw(Camera camera)
    {
        _model.Draw(
            GetWorld(),
            camera.GetView(), 
            camera.GetProjection());
    }
}