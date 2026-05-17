using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class Vehicle
{
    private readonly CustomModel _model;

    public Vector3 Position;
    public float RotationY;

    public Vehicle(CustomModel model, Vector3 initialPosition)
    {
        _model = model;

        Position = initialPosition;

        RotationY = 0f;
    }

    public void Update(GameTime gameTime)
    {

    }

    public Matrix GetWorld()
    {
        return
            Matrix.CreateRotationY(RotationY) *
            Matrix.CreateTranslation(Position);
    }

    public void Draw(GameTime gameTime, Camera camera)
    {
        _model.Draw(
            GetWorld(),
            camera.GetView(),
            camera.GetProjection()
        );
    }
}