using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public class FreeCamera : Camera
{
    private const float DefaultSpeed = 1000f;
    private const float DefaultMouseSensitivity = 0.2f;

    private Vector3 _position;
    private Vector3 _target;
    private float _yaw;
    private float _pitch;
    private Matrix _view;
    private Matrix _projection;
    private readonly GraphicsDevice _graphicsDevice;

    public FreeCamera(Vector3 position, Vector3 target, GraphicsDevice graphicsDevice)
    {
        try
        {
            Sdl2Native.SetRelativeMouseMode(true);
        }
        catch (DllNotFoundException e)
        {
            Console.WriteLine("Error enabling SDL (Not Found): " + e.Message);
            throw new ApplicationException();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine("Error enabling SDL (OS Not Supported): " + e.Message);
            throw new ApplicationException();
        }
        _position = position;
        _target = target;
        _graphicsDevice = graphicsDevice;
        var direction = Vector3.Normalize(target - position);
        _pitch = MathHelper.ToDegrees(MathF.Asin(direction.Y));
        _yaw = MathHelper.ToDegrees(MathF.Atan2(direction.Z, direction.X));
        _view = Matrix.CreateLookAt(_position, _position + _target, Vector3.Up);
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            graphicsDevice.Viewport.AspectRatio,
            0.1f,
            4000f
        );
    }

    public void OnClientSizeChanged(object sender, EventArgs e)
    {
        _projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4, _graphicsDevice.Viewport.AspectRatio, 1f, 1000f);
    }

    public static string GetName()
    {
        return "Free Camera";
    }

    public Matrix GetView()
    {
        return _view;
    }

    public Matrix GetProjection()
    {
        return _projection;
    }

    public void Update(GameTime gameTime, bool mouseCaptured)
    {
        if (mouseCaptured)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Sdl2Native.GetRelativeMouseState(out int relX, out int relY);
            _yaw += relX * DefaultMouseSensitivity;
            _pitch -= relY * DefaultMouseSensitivity;
            _pitch = MathHelper.Clamp(_pitch, -89f, 89f);
            Vector3 front;
            front.X = MathF.Cos(MathHelper.ToRadians(_pitch)) * MathF.Cos(MathHelper.ToRadians(_yaw));
            front.Y = MathF.Sin(MathHelper.ToRadians(_pitch));
            front.Z = MathF.Cos(MathHelper.ToRadians(_pitch)) * MathF.Sin(MathHelper.ToRadians(_yaw));

            KeyboardState kb = Keyboard.GetState();
            Vector3 right = Vector3.Normalize(Vector3.Cross(front, Vector3.Up));
            if (kb.IsKeyDown(Keys.W)) _position += front * DefaultSpeed * deltaTime;
            if (kb.IsKeyDown(Keys.S)) _position -= front * DefaultSpeed * deltaTime;
            if (kb.IsKeyDown(Keys.A)) _position -= right * DefaultSpeed * deltaTime;
            if (kb.IsKeyDown(Keys.D)) _position += right * DefaultSpeed * deltaTime;
            if (kb.IsKeyDown(Keys.Space)) _position += Vector3.Up * DefaultSpeed * deltaTime;
            if (kb.IsKeyDown(Keys.LeftShift)) _position -= Vector3.Up * DefaultSpeed * deltaTime;
            _view = Matrix.CreateLookAt(_position, _position + Vector3.Normalize(front), Vector3.Up);
        }
    }
}