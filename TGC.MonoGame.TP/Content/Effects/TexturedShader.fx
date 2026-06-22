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

Texture2D ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 normal   : NORMAL; 
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPosition : TEXCOORD2;
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
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 ambientColor = float3(0.2, 0.25, 0.35);
    float3 L = normalize(-float3(1.0, -1.0, 1.0));
    float3 diffuseColor = float3(0.8, 0.8, 0.8);
    float3 specularColor = float3(0.5, 0.5, 0.5);
    float3 N = normalize(input.normal);
    float3 viewDirection = normalize(CameraPosition - input.worldPosition);
    float3 halfVector = normalize(viewDirection + L); 
    float1 shininess = 32;
    float1 ka = 0.2;
    float1 kd = 1;
    float1 ks = 1;
    float4 pixelColor = tex2D(textureSampler, input.TextureCoordinate);
    float3 ambientLight = ambientColor * ka;
    float3 diffuseLight = saturate(dot(L, N)) * diffuseColor * kd;
    float3 specularLight = pow(saturate(dot(halfVector, N)), shininess) * specularColor * ks * step(0.0, dot(N, L));
    return float4((ambientLight + diffuseLight) * pixelColor.xyz + specularLight, pixelColor.a);
}

technique TexturedDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
