using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class Light
{
    private static readonly Vector3 DefaultLightDirection = Vector3.Normalize(new Vector3(1f, 1f, 1f));

    public Vector3 Position { get; private set; }

    public Vector3 Direction { get; private set; }

    public Matrix ViewProjection { get; private set; }

    private readonly Vector3 _up;
    private readonly float _orthographicWidth;
    private readonly float _orthographicHeight;
    private readonly float _nearPlane;
    private readonly float _farPlane;
    private readonly float _distance;

    public Light(
        Vector3 position,
        Vector3 initialTarget,
        Vector3? up = null,
        float orthographicWidth = 3200f,
        float orthographicHeight = 3200f,
        float nearPlane = 1f,
        float farPlane = 5000f,
        float distance = 1600f)
    {
        _up = up ?? Vector3.Up;
        _orthographicWidth = orthographicWidth;
        _orthographicHeight = orthographicHeight;
        _nearPlane = nearPlane;
        _farPlane = farPlane;
        _distance = distance;

        Position = position;
        Direction = DefaultLightDirection;
        ViewProjection = Matrix.Identity;

        Follow(initialTarget);
    }

    public void Follow(Vector3 targetPosition)
    {
        var lightPosition = targetPosition + DefaultLightDirection * _distance;
        Position = lightPosition;

        var directionToTarget = lightPosition - targetPosition;
        Direction = directionToTarget.LengthSquared() > 0.0001f
            ? Vector3.Normalize(directionToTarget)
            : DefaultLightDirection;

        var lightView = Matrix.CreateLookAt(lightPosition, targetPosition, _up);
        var lightProjection = Matrix.CreateOrthographic(_orthographicWidth, _orthographicHeight, _nearPlane, _farPlane);
        ViewProjection = lightView * lightProjection;
    }
}
