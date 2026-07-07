#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Sombras como POST-PROCESADO (screen-space):
// Pass previo: se renderiza la escena a color y la profundidad de cámara a texturas.
// Aquí, en un full-screen quad, por cada píxel reconstruimos su posición de mundo desde la
// profundidad de cámara, la proyectamos al espacio de la luz y comparamos contra el shadow map.

float4x4 LightViewProjection;      // View * Projection de la luz (para muestrear el shadow map)
float4x4 InverseViewProjection;    // Inversa de (View * Projection) de la cámara (reconstrucción)
float ShadowMapSize = 1536.0f;
float ShadowStrength = 0.55f;
float ShadowBias = 0.0025f;

Texture2D SceneTexture;
sampler2D sceneSampler = sampler_state
{
    Texture = (SceneTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D CameraDepthTexture;
sampler2D depthSampler = sampler_state
{
    Texture = (CameraDepthTexture);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D ShadowMap;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMap);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    // El quad ya viene en clip space, se pasa directo.
    output.Position = input.Position;
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float SampleShadow(float3 worldPosition)
{
    float4 lightClip = mul(float4(worldPosition, 1.0), LightViewProjection);
    if (lightClip.w <= 0.0)
        return 1.0;

    float3 ndc = lightClip.xyz / lightClip.w;
    float2 shadowTexCoord = ndc.xy * float2(0.5, -0.5) + 0.5;

#if OPENGL
    float currentDepth = ndc.z * 0.5 + 0.5;
#else
    float currentDepth = ndc.z;
#endif

    // Fuera del frustum de la luz => sin sombra
    if (shadowTexCoord.x < 0.0 || shadowTexCoord.x > 1.0 ||
        shadowTexCoord.y < 0.0 || shadowTexCoord.y > 1.0 ||
        currentDepth < 0.0 || currentDepth > 1.0)
        return 1.0;

    currentDepth -= ShadowBias;

    // PCF 3x3
    float texelSize = 1.0 / ShadowMapSize;
    float shadow = 0.0;
    [unroll]
    for (int x = -1; x <= 1; x++)
    {
        [unroll]
        for (int y = -1; y <= 1; y++)
        {
            float sampleDepth = tex2D(shadowSampler, shadowTexCoord + float2(x, y) * texelSize).r;
            shadow += (currentDepth <= sampleDepth) ? 1.0 : (1.0 - ShadowStrength);
        }
    }
    return shadow / 9.0;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 sceneColor = tex2D(sceneSampler, input.TextureCoordinate).rgb;
    float storedDepth = tex2D(depthSampler, input.TextureCoordinate).r;

    // Fondo / cielo (profundidad limpiada a 1.0): no recibe sombra.
    if (storedDepth >= 0.9999)
        return float4(sceneColor, 1.0);

    // Reconstruir la posición de mundo desde la profundidad de cámara.
    float2 uv = input.TextureCoordinate;
#if OPENGL
    float ndcZ = storedDepth * 2.0 - 1.0;
#else
    float ndcZ = storedDepth;
#endif
    float4 clip = float4(uv.x * 2.0 - 1.0, 1.0 - uv.y * 2.0, ndcZ, 1.0);
    float4 worldHomogeneous = mul(clip, InverseViewProjection);
    float3 worldPosition = worldHomogeneous.xyz / worldHomogeneous.w;

    float shadow = SampleShadow(worldPosition);
    return float4(sceneColor * shadow, 1.0);
}

technique ShadowPostProcess
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
