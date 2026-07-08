using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public class ShadowMapRenderer : IDisposable
{
    public const int DefaultSize = 1536;

    private static readonly Vector3 DefaultLightDirection = Vector3.Normalize(new Vector3(1f, 1f, 1f));

    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderTarget2D _shadowMapTarget;

    private readonly Vector3 _lightDirection;
    private readonly float _lightDistance;
    private readonly float _orthographicSize;
    private readonly float _nearPlane;
    private readonly float _farPlane;

    private readonly RasterizerState _depthRasterizerState = new()
    {
        CullMode = CullMode.CullCounterClockwiseFace
    };

    private Matrix _lightViewProjection = Matrix.Identity;

    public Texture2D ShadowMap => _shadowMapTarget;
    public Matrix LightViewProjection => _lightViewProjection;
    public Vector3 LightDirection => _lightDirection;
    public int Size => _shadowMapTarget.Width;

    public ShadowMapRenderer(
        GraphicsDevice graphicsDevice,
        int size = DefaultSize,
        float orthographicSize = 3200f,
        float lightDistance = 1600f,
        float nearPlane = 1f,
        float farPlane = 3500f
    )
    {
        _graphicsDevice = graphicsDevice;
        _lightDirection = DefaultLightDirection;
        _lightDistance = lightDistance;
        _orthographicSize = orthographicSize;
        _nearPlane = nearPlane;
        _farPlane = farPlane;

        _shadowMapTarget = new RenderTarget2D(
            graphicsDevice,
            size,
            size,
            false,
            SurfaceFormat.Single,
            DepthFormat.Depth24
        );
    }

    public void UpdateLightMatrices(Vector3 target)
    {
        var lightPosition = target + _lightDirection * _lightDistance;
        var lightView = Matrix.CreateLookAt(lightPosition, target, Vector3.Up);
        var lightProjection = Matrix.CreateOrthographic(_orthographicSize, _orthographicSize, _nearPlane, _farPlane);
        _lightViewProjection = lightView * lightProjection;
    }

    public void RenderDepth(Action<Matrix> drawSceneDepth)
    {
        _graphicsDevice.SetRenderTarget(_shadowMapTarget);
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = _depthRasterizerState;
        // Blanco = profundidad maxima (1.0)
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);

        drawSceneDepth(_lightViewProjection);

        _graphicsDevice.SetRenderTarget(null);
    }

    public void Dispose()
    {
        _shadowMapTarget?.Dispose();
        _depthRasterizerState?.Dispose();
        GC.SuppressFinalize(this);
    }
}
