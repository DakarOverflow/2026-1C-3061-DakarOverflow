using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace TGC.MonoGame.TP;

public class FreeCamera : CameraI
{
    [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_SetRelativeMouseMode(int enabled);
    
    [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetRelativeMouseMode();
    
    [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetError();
    
    [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetRelativeMouseState(out int x, out int y);

    private const float DefaultSpeed = 100f;
    private const float DefaultMouseSensitivity = 0.2f;

    private Vector3 _position;
    private Vector3 _target;
    private float _yaw;
    private float _pitch;
    
    public Matrix View { get; private set; }
    public Matrix GetView() { return this.View; }

    public FreeCamera(Vector3 position, Vector3 target)
    {
        var hasError = SDL_SetRelativeMouseMode(1);
        Console.WriteLine("Enabled SDL: " + SDL_GetRelativeMouseMode());
        if (hasError != 0)
        {
            Console.WriteLine("SDL Error: " + Marshal.PtrToStringAnsi(SDL_GetError()));
        }
        _position = position;
        _target = target;
        Vector3 direction = Vector3.Normalize(target - position);
        _pitch = MathHelper.ToDegrees(MathF.Asin(direction.Y));
        _yaw = MathHelper.ToDegrees(MathF.Atan2(direction.Z, direction.X));
        View = Matrix.CreateLookAt(_position, _position + _target, Vector3.Up);
    }

    public void Update(GameTime gameTime, bool mouseCaptured)
    {
        if (mouseCaptured)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            SDL_GetRelativeMouseState(out int relX, out int relY);
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
            View = Matrix.CreateLookAt(_position,  _position + Vector3.Normalize(front), Vector3.Up);
        }
    }
}