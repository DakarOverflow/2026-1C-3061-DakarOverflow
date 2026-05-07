using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class WorldObject
{
    private readonly CustomModel _model;
    private readonly Matrix _initial_world;
    private Vector3 _offset;
    private Vector3 _rotation;


    public WorldObject(CustomModel model, Matrix initial_world, Vector3 initial_offset, Vector3 initial_rotation)
    {
        _model = model;
        _initial_world = initial_world;
        _offset = initial_offset;
        _rotation = initial_rotation;
    }

    public Matrix GetCurrentWorld(GameTime gameTime)
    {
        return Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(_rotation.X, _rotation.Y, _rotation.Z)) * Matrix.CreateTranslation(_offset) * _initial_world;
    }

    public void Update(GameTime gameTime)
    {
        //TODO: Implementar para cada caso particular
    }

    public void Draw(GameTime gameTime, Matrix view, Matrix projection)
    {   
        _model.Draw(this.GetCurrentWorld(gameTime), view, projection);
    }

    public void DrawOn(GameTime gameTime, CameraI camera, Matrix projection)
    {
        this.Draw(gameTime, camera.GetView(), projection);
    }


}