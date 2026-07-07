#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProjection;
float3 LightDirection = normalize(float3(1.0, 1.0, 1.0));

float3 DiffuseColor;

float Time = 0;
float ShadowBias = 0.003f;
float ShadowStrength = 0.55f;

Texture2D ShadowMapTexture;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMapTexture);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 lightPosition : TEXCOORD0;
};

struct ShadowVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 DepthPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.lightPosition = mul(worldPosition, LightViewProjection);

    return output;
}

float NormalizeShadowDepth(float depth)
{
#if OPENGL
    return depth * 0.5 + 0.5;
#else
    return depth;
#endif
}

ShadowVertexShaderOutput ShadowVS(in VertexShaderInput input)
{
    ShadowVertexShaderOutput output = (ShadowVertexShaderOutput)0;
    float4 lightPosition = mul(mul(input.Position, World), LightViewProjection);
    output.Position = lightPosition;
    output.DepthPosition = lightPosition;
    return output;
}

float4 ShadowPS(ShadowVertexShaderOutput input) : COLOR
{
    float depth = NormalizeShadowDepth(input.DepthPosition.z / input.DepthPosition.w);
    return float4(depth, depth, depth, 1.0);
}

float GetShadowFactor(float4 lightPosition)
{
    if (lightPosition.w <= 0.0)
    {
        return 1.0;
    }

    float3 projectionCoordinates = lightPosition.xyz / lightPosition.w;
    float2 shadowTexCoord = projectionCoordinates.xy * float2(0.5, -0.5) + 0.5;

    float currentDepth = NormalizeShadowDepth(projectionCoordinates.z);

    if (shadowTexCoord.x < 0.0 || shadowTexCoord.x > 1.0 || shadowTexCoord.y < 0.0 || shadowTexCoord.y > 1.0 || currentDepth < 0.0 || currentDepth > 1.0)
    {
        return 1.0;
    }

    currentDepth -= ShadowBias;
    float closestDepth = tex2D(shadowSampler, shadowTexCoord).r;
    return currentDepth <= closestDepth ? 1.0 : 1.0 - ShadowStrength;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float shadowFactor = GetShadowFactor(input.lightPosition);
    return float4(DiffuseColor * shadowFactor, 1.0);
}

float4 DebugPS(VertexShaderOutput input) : COLOR
{
    return float4(DiffuseColor, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique DebugLineDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL DebugPS();
	}
}

technique ShadowMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL ShadowVS();
        PixelShader = compile PS_SHADERMODEL ShadowPS();
    }
};
