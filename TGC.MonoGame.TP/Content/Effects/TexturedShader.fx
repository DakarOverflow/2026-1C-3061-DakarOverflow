#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4x4 WorldInverseTranspose;
bool UseLighting = true;

float4x4 LightViewProjection;
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

Texture2D ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D OverlayTexture;
sampler2D overlaySampler = sampler_state
{
    Texture = (OverlayTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

bool UseOverlay;
struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 normal   : NORMAL;
};

struct UnlitVertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPosition : TEXCOORD2;
    float4 lightPosition : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TextureCoordinate = input.TextureCoordinate;
    output.normal = normalize(mul(input.normal, (float3x3)WorldInverseTranspose));
    output.worldPosition = worldPosition.xyz;
    output.lightPosition = mul(worldPosition, LightViewProjection);
    return output;
}

VertexShaderOutput MainUnlitVS(in UnlitVertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TextureCoordinate = input.TextureCoordinate;
    output.normal = float3(0.0, 1.0, 0.0);
    output.worldPosition = worldPosition.xyz;
    output.lightPosition = mul(worldPosition, LightViewProjection);
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinate);
    float4 finalColor = baseColor;

    float2 overlayUV;
    overlayUV.x = input.worldPosition.x * 0.00225f;
    overlayUV.y = input.worldPosition.z * 0.00225f;
    float4 overlayColor = tex2D(overlaySampler, overlayUV);
    
    float brightness = (overlayColor.r + overlayColor.g + overlayColor.b) / 3.0f;
    
    // --- FIX 1: Eliminate Ternary for Overlay Alpha ---
    overlayColor.a = 1.0f;
    if (brightness < 0.05f)
    {
        overlayColor.a = 0.0f;
    }

    if (UseOverlay == true)
    {
        finalColor = lerp(baseColor, overlayColor, overlayColor.a);
    }

    if (UseLighting == false)
    {
        return finalColor;
    }

    float3 ambientColor = float3(0.2, 0.25, 0.35);
    float3 L = normalize(-float3(1.0, -1.0, 1.0)); 
    float3 diffuseColor = float3(0.8, 0.8, 0.8);
    float3 specularColor = float3(0.5, 0.5, 0.5);
    float3 N = normalize(input.normal);
    float3 viewDirection = normalize(CameraPosition - input.worldPosition);
    float3 halfVector = normalize(viewDirection + L);
    float1 shininess = 32;
    float1 ka = 0.8;
    float1 kd = 1;
    float1 ks = 1;
    
    float3 ambientLight = ambientColor * ka;

    float shadow = 1.0;
    float3 projected = input.lightPosition.xyz / input.lightPosition.w;
    float2 shadowUv = projected.xy * float2(0.5, -0.5) + 0.5;

    if (UseShadowMap == true)
    {
        bool inBoundsX = (shadowUv.x >= 0.0) && (shadowUv.x <= 1.0);
        bool inBoundsY = (shadowUv.y >= 0.0) && (shadowUv.y <= 1.0);
        bool inBoundsZ = (projected.z >= 0.0) && (projected.z <= 1.0);

        if (inBoundsX && inBoundsY && inBoundsZ)
        {
            float closestDepth = tex2D(shadowSampler, shadowUv).r;
            
            if (projected.z - ShadowBias > closestDepth)
            {
                shadow = 1.0 - ShadowStrength;
            }
            else
            {
                shadow = 1.0;
            }
        }
    }
    
    float3 diffuseLight = saturate(dot(L, N)) * diffuseColor * kd * shadow;
    float3 specularLight = pow(saturate(dot(halfVector, N)), shininess) * specularColor * ks * step(0.0, dot(N, L)) * shadow;
    
    return float4((ambientLight + diffuseLight) * finalColor.xyz + specularLight, finalColor.a);
}


technique TexturedDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique TexturedUnlitDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainUnlitVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
