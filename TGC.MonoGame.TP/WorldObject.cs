using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class WorldObject
{
    private readonly Model _model;
    private readonly Matrix _initial_world;
    private Vector3 _offset;
    private Quaternion _rotation;

    public WorldObject(Model model, Matrix initial_world, Vector3 initial_offset, Quaternion initial_rotation)
    {
        _model = model;
        _initial_world = initial_world;
        _offset = initial_offset;
        _rotation = initial_rotation;
    }

    private Matrix GetCurrentWorld(GameTime gameTime)
    {
        return Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateTranslation(_offset) * _initial_world;
    }

    public void DrawOn(GameTime gameTime, Matrix view, Matrix projection)
    {
        
        _model.Draw(this.GetCurrentWorld(gameTime), view, projection);
    }
}