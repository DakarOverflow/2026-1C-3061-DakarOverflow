using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;
using System.Collections;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.Zero;


public class Tile
{
    private readonly List<WorldObject> _objects;

    public Tile()
    {
        _objects = new List<WorldObject>();
    }

    public void AddObject(WorldObject obj)
    {
        _objects.Add(obj);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in _objects)
        {
            obj.Update(gameTime);
        }
    }

    public void Draw(GameTime gameTime, Camera camera)
    {
        foreach (var obj in _objects)
        {
            obj.DrawOn(
                gameTime,
                camera,
                camera.GetProjection()
            );
        }
    }
}