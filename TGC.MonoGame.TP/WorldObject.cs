using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class WorldObject
{
    private readonly CustomModel _model;

    public Matrix World { get; protected set; }

    public WorldObject(
        CustomModel model,
        Matrix world
    )
    {
        _model = model;
        World = world;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Draw(
        GameTime gameTime,
        Matrix view,
        Matrix projection
    )
    {
        _model.Draw(
            World,
            view,
            projection
        );
    }

    public void DrawOn(
        GameTime gameTime,
        Camera camera,
        Matrix projection
    )
    {
        Draw(
            gameTime,
            camera.GetView(),
            projection
        );
    }
}