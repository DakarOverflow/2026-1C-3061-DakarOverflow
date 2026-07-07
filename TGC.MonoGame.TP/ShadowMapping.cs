using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public static class ShadowMapping
{
    public static Texture2D ShadowMap { get; private set; }
    public static Matrix LightViewProjection { get; private set; } = Matrix.Identity;
    public static bool Enabled => ShadowMap != null;
    public static ShadowDiagnostics Diagnostics { get; private set; }

    public static void SetDiagnostics(ShadowDiagnostics diagnostics)
    {
        Diagnostics = diagnostics;
    }

    public static void SetShadowMap(Texture2D shadowMap, Matrix lightViewProjection)
    {
        ShadowMap = shadowMap;
        LightViewProjection = lightViewProjection;
    }

    public static void ApplyTo(Effect effect)
    {
        effect.Parameters["UseShadowMap"]?.SetValue(Enabled);
        effect.Parameters["ShadowMap"]?.SetValue(ShadowMap);
        effect.Parameters["LightViewProjection"]?.SetValue(LightViewProjection);
    }
}
