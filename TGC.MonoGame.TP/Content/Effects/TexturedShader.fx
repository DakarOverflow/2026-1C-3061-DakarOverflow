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
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;

    float3 WorldPosition : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);	
    output.Position = mul(viewPosition, Projection);
    
    output.TextureCoordinate = input.TextureCoordinate;

    output.WorldPosition = worldPosition.xyz;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(textureSampler, input.TextureCoordinate);

    if(!UseOverlay)
    {
        return baseColor;
    }

    float2 overlayUV;
    overlayUV.x = input.WorldPosition.x * 0.00225f;
    overlayUV.y = input.WorldPosition.z * 0.00225f;
    float4 overlayColor = tex2D(overlaySampler, overlayUV);

    float brightness = (overlayColor.r + overlayColor.g + overlayColor.b) / 3.0f;

    overlayColor.a = (brightness < 0.05f) ? 0.0f : 1.0f;

    return lerp(baseColor, overlayColor, overlayColor.a);
}

technique TexturedDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
