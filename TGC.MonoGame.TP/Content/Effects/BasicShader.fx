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

float3 DiffuseColor;
bool UseShadowMap = false;
float ShadowBias = 0.003;
float ShadowStrength = 0.45;

Texture2D ShadowMap;
sampler2D shadowSampler = sampler_state
{
    Texture = (ShadowMap);
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 LightPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.LightPosition = mul(worldPosition, LightViewProjection);

    return output;
}

float ComputeShadow(float4 lightPosition)
{
    if (UseShadowMap == false)
    {
        return 1.0;
    }

    float3 projected = lightPosition.xyz / lightPosition.w;
    float2 shadowUv = projected.xy * float2(0.5, -0.5) + 0.5;

    bool inBoundsX = (shadowUv.x >= 0.0) && (shadowUv.x <= 1.0);
    bool inBoundsY = (shadowUv.y >= 0.0) && (shadowUv.y <= 1.0);
    bool inBoundsZ = (projected.z >= 0.0) && (projected.z <= 1.0);

    if (inBoundsX && inBoundsY && inBoundsZ)
    {
        float closestDepth = tex2D(shadowSampler, shadowUv).r;
        if (projected.z - ShadowBias > closestDepth)
        {
            return 1.0 - ShadowStrength;
        }
    }

    return 1.0;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float shadow = ComputeShadow(input.LightPosition);
    return float4(DiffuseColor * shadow, 1.0);
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
