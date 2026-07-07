using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public sealed class ShadowDiagnostics
{
    private const string Prefix = "[ShadowMap]";
    private int _framesRendered;
    private bool _startupLogged;
    private bool _contentLogged;
    private bool _firstFrameLogged;
    private bool _sampleLogged;

    public int SubmittedModels { get; private set; }
    public int SubmittedMeshes { get; private set; }
    public int SubmittedMeshParts { get; private set; }
    public int SubmittedPrimitives { get; private set; }

    public void LogStartup(GraphicsDevice graphicsDevice, int shadowMapSize)
    {
        if (_startupLogged) return;
        _startupLogged = true;

        var adapter = graphicsDevice.Adapter;
        Console.WriteLine($"{Prefix} Graphics profile: {graphicsDevice.GraphicsProfile}; backend: {GetBackendHint()}; adapter: {adapter.Description}; shadow map size: {shadowMapSize}x{shadowMapSize}.");
        Console.WriteLine($"{Prefix} If shadows are missing on Linux/NVIDIA, check that the shadow pass submits geometry below and that the sampled texture is not all white/empty.");
    }

    public void LogContent(Effect shadowEffect, RenderTarget2D renderTarget)
    {
        if (_contentLogged) return;
        _contentLogged = true;

        Console.WriteLine($"{Prefix} Render target: format={renderTarget.Format}, depth={renderTarget.DepthStencilFormat}, usage={renderTarget.RenderTargetUsage}, multiSample={renderTarget.MultiSampleCount}.");
        LogEffectCheck(shadowEffect, "ShadowMap effect", activeTechnique: "ShadowMapDrawing", requiredParameters: ["World", "LightViewProjection"]);
    }

    public void LogMainEffect(Effect effect, string modelName, string activeTechnique)
    {
        LogEffectCheck(effect, $"Main effect for {modelName}", activeTechnique, requiredParameters: ["UseShadowMap", "ShadowMap", "LightViewProjection", "ShadowBias", "ShadowStrength"]);
    }

    public void BeginFrame()
    {
        SubmittedModels = 0;
        SubmittedMeshes = 0;
        SubmittedMeshParts = 0;
        SubmittedPrimitives = 0;
    }

    public void RecordModel(int meshes, int meshParts, int primitives)
    {
        SubmittedModels++;
        SubmittedMeshes += meshes;
        SubmittedMeshParts += meshParts;
        SubmittedPrimitives += primitives;
    }

    public void EndFrame(Matrix lightViewProjection)
    {
        _framesRendered++;
        if (_firstFrameLogged) return;
        _firstFrameLogged = true;

        Console.WriteLine($"{Prefix} First shadow pass submitted models={SubmittedModels}, meshes={SubmittedMeshes}, meshParts={SubmittedMeshParts}, primitives={SubmittedPrimitives}.");
        if (SubmittedPrimitives == 0)
        {
            Console.WriteLine($"{Prefix} WARNING: no primitives were submitted to the shadow pass. The map will stay empty and no shadows can appear.");
        }
        Console.WriteLine($"{Prefix} First light view-projection matrix: {lightViewProjection}.");
    }

    public void TryLogShadowMapSample(RenderTarget2D renderTarget)
    {
        if (_sampleLogged || _framesRendered < 2) return;
        _sampleLogged = true;

        try
        {
            var step = Math.Max(1, renderTarget.Width / 16);
            var data = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData(data);

            int sampled = 0;
            int min = 255;
            int max = 0;
            long sum = 0;

            for (int y = 0; y < renderTarget.Height; y += step)
            {
                for (int x = 0; x < renderTarget.Width; x += step)
                {
                    var value = data[y * renderTarget.Width + x].R;
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                    sum += value;
                    sampled++;
                }
            }

            var avg = sampled == 0 ? 0 : sum / (float)sampled;
            Console.WriteLine($"{Prefix} Shadow map sample after render: min={min}, max={max}, avg={avg:0.00}, samples={sampled}.");
            if (min == 255 && max == 255)
            {
                Console.WriteLine($"{Prefix} WARNING: shadow map is all white in the sample. Likely causes: all casters are culled, depth encoding is outside 0..1, or the shadow pass did not render.");
            }
            else if (min == max)
            {
                Console.WriteLine($"{Prefix} WARNING: shadow map sample is constant. Shadows may be invisible because depth has no useful variation.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Prefix} WARNING: could not read back the shadow map for diagnostics: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void LogEffectCheck(Effect effect, string label, string activeTechnique, string[] requiredParameters)
    {
        if (effect == null)
        {
            Console.WriteLine($"{Prefix} ERROR: {label} is null.");
            return;
        }

        if (string.IsNullOrEmpty(activeTechnique) || effect.Techniques[activeTechnique] == null)
        {
            Console.WriteLine($"{Prefix} ERROR: {label} is missing active technique '{activeTechnique}'.");
        }

        foreach (var parameter in requiredParameters)
        {
            if (effect.Parameters[parameter] == null)
            {
                Console.WriteLine($"{Prefix} ERROR: {label} is missing parameter '{parameter}'.");
            }
        }
    }

    private static string GetBackendHint()
    {
#if OPENGL
        return "OpenGL";
#else
        return "DirectX";
#endif
    }
}
