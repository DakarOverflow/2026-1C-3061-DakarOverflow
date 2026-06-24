using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class WorldObject
{
    private readonly CustomModel _model;

    private Matrix _world;

    public CustomModel Model => _model;

    public Matrix World => _world;

    public void setWorld(Matrix world)
    {
        _world = world;
    }

    public WorldObject(
        CustomModel model,
        Matrix world
    )
    {
        _model = model;
        _world = world;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void SetShadowMap(Texture2D shadowMap, Matrix lightViewProjection)
    {
        _model.SetShadowMap(shadowMap, lightViewProjection);
    }

    public virtual void DrawDepth(Matrix lightViewProjection)
    {
        _model.DrawDepth(_world, lightViewProjection);
    }

    public virtual void Draw(
        GameTime gameTime,
        Matrix view,
        Matrix projection
    )
    {
        _model.Draw(
            _world,
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