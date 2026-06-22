using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

/// <summary>
///     Una camara que sigue objetos.
/// </summary>
internal class FollowCamera : Camera
{
    private const float AxisDistanceToTarget = 600f;
    private const float CameraHeightRatio = 0.4f;
    private const float LookAheadDistance = 300f;
    private const float MinForwardFollowSpeed = 3.5f;
    private const float MaxForwardFollowSpeed = 16f;
    private const float MinLookAtFollowSpeed = 6f;
    private const float MaxLookAtFollowSpeed = 22f;

    private Vector3 _currentForwardVector = Vector3.Forward;
    private Vector3 _currentLookAtPosition;
    private bool _isInitialized;
    private readonly GraphicsDevice _graphicsDevice;

    /// <summary>
    ///     Crea una FollowCamera que sigue a una matriz de mundo.
    /// </summary>
    /// <param name="aspectRatio"></param>
    public FollowCamera(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _projection = CreateProjectionMatrix();
    }

    private Matrix _projection;

    private Matrix _view;

    public void OnClientSizeChanged(object sender, EventArgs e)
    {
        _projection = CreateProjectionMatrix();
    }

    public static string GetName()
    {
        return "Follow Camera";
    }

    public Matrix GetView()
    {
        return _view;
    }

    public Matrix GetProjection()
    {
        return _projection;
    }

    private Matrix CreateProjectionMatrix()
    {
        return Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            _graphicsDevice.Viewport.AspectRatio,
            1f,
            4000f
        );
    }

    /// <summary>
    ///     Actualiza la Camara usando una matriz de mundo actualizada para seguirla.
    /// </summary>
    /// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
    /// <param name="followedWorld">The World matrix to follow</param>
    public void Update(GameTime gameTime, Matrix followedWorld)
    {
        var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        var followedPosition = followedWorld.Translation;
        var followedForward = Vector3.Normalize(followedWorld.Forward);
        var targetLookAtPosition = followedPosition + followedForward * LookAheadDistance;

        if (!_isInitialized)
        {
            _currentForwardVector = followedForward;
            _currentLookAtPosition = targetLookAtPosition;
            _isInitialized = true;
        }
        
        var forwardDot = MathHelper.Clamp(Vector3.Dot(_currentForwardVector, followedForward), -1f, 1f);
        var turnSharpness = (1f - forwardDot) * 0.5f;
        var forwardFollowSpeed = MathHelper.Lerp(MinForwardFollowSpeed, MaxForwardFollowSpeed, turnSharpness);
        var lookAtFollowSpeed = MathHelper.Lerp(MinLookAtFollowSpeed, MaxLookAtFollowSpeed, turnSharpness);

        _currentForwardVector = SmoothDampDirection(
            _currentForwardVector,
            followedForward,
            GetExponentialBlendAmount(forwardFollowSpeed, elapsedTime)
        );

        _currentLookAtPosition = Vector3.Lerp(
            _currentLookAtPosition,
            targetLookAtPosition,
            GetExponentialBlendAmount(lookAtFollowSpeed, elapsedTime)
        );

        var offsetedPosition = followedPosition
                               - _currentForwardVector * AxisDistanceToTarget
                               + Vector3.Up * (AxisDistanceToTarget * CameraHeightRatio);

        _view = Matrix.CreateLookAt(offsetedPosition, _currentLookAtPosition, Vector3.Up);
    }

    private static float GetExponentialBlendAmount(float speed, float elapsedTime)
    {
        return 1f - MathF.Exp(-speed * elapsedTime);
    }

    private static Vector3 SmoothDampDirection(Vector3 currentDirection, Vector3 targetDirection, float blendAmount)
    {
        var blendedDirection = Vector3.Lerp(currentDirection, targetDirection, blendAmount);

        if (blendedDirection.LengthSquared() < 0.0001f)
            return targetDirection;

        blendedDirection.Normalize();
        return blendedDirection;
    }
}
