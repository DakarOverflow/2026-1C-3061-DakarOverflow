using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;


public class ShadowPostProcessor : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Effect _effect;
    private readonly FullScreenQuad _fullScreenQuad;

    private RenderTarget2D _sceneColorTarget;
    private RenderTarget2D _cameraDepthTarget;
    private int _width;
    private int _height;

    public ShadowPostProcessor(GraphicsDevice graphicsDevice, Effect postProcessEffect)
    {
        _graphicsDevice = graphicsDevice;
        _effect = postProcessEffect;
        _fullScreenQuad = new FullScreenQuad(graphicsDevice);
        EnsureTargets();
    }


    private void EnsureTargets()
    {
        var width = Math.Max(1, _graphicsDevice.PresentationParameters.BackBufferWidth);
        var height = Math.Max(1, _graphicsDevice.PresentationParameters.BackBufferHeight);

        if (_sceneColorTarget != null && width == _width && height == _height)
            return;

        _width = width;
        _height = height;

        _sceneColorTarget?.Dispose();
        _cameraDepthTarget?.Dispose();

        _sceneColorTarget = new RenderTarget2D(_graphicsDevice, width, height, false,
            SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

        // Profundidad de cámara (z/w) en float de un canal para reconstruir la posición de mundo.
        _cameraDepthTarget = new RenderTarget2D(_graphicsDevice, width, height, false,
            SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
    }

    public void RenderCameraDepth(Action<Matrix> drawSceneDepth, Matrix cameraViewProjection)
    {
        EnsureTargets();

        _graphicsDevice.SetRenderTarget(_cameraDepthTarget);
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        // Blanco = 1.0 = profundidad máxima (fondo / sin geometría).
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);

        drawSceneDepth(cameraViewProjection);

        _graphicsDevice.SetRenderTarget(null);
    }
    public void BeginSceneColor(Color clearColor)
    {
        EnsureTargets();
        _graphicsDevice.SetRenderTarget(_sceneColorTarget);
        _graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1f, 0);
    }
    public void Apply(
        Texture2D shadowMap,
        Matrix lightViewProjection,
        Matrix cameraViewProjection,
        float shadowMapSize,
        float shadowStrength,
        float shadowBias)
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        _effect.Parameters["SceneTexture"]?.SetValue(_sceneColorTarget);
        _effect.Parameters["CameraDepthTexture"]?.SetValue(_cameraDepthTarget);
        _effect.Parameters["ShadowMap"]?.SetValue(shadowMap);
        _effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjection);
        _effect.Parameters["InverseViewProjection"]?.SetValue(Matrix.Invert(cameraViewProjection));
        _effect.Parameters["ShadowMapSize"]?.SetValue(shadowMapSize);
        _effect.Parameters["ShadowStrength"]?.SetValue(shadowStrength);
        _effect.Parameters["ShadowBias"]?.SetValue(shadowBias);

        _effect.CurrentTechnique = _effect.Techniques["ShadowPostProcess"];
        _fullScreenQuad.Draw(_effect);
    }

    public void Dispose()
    {
        _sceneColorTarget?.Dispose();
        _cameraDepthTarget?.Dispose();
        _fullScreenQuad?.Dispose();
        GC.SuppressFinalize(this);
    }
}
