using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class WorldObject
{
    private Model _model;
    private Matrix _initial_world;
    private Vector3 _offset;
    private Quaternion _rotation;

    private Matrix GetCurrentWorld(GameTime gameTime)
    {
        return Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateTranslation(_offset) * _initial_world;
    }

    public void DrawOn(GameTime gameTime, Matrix view, Matrix projection)
    {
        
        _model.Draw(this.GetCurrentWorld(gameTime), view, projection);
    }
}