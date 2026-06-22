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
float4x4 LightViewProjection;
bool UseLighting = true;
float ShadowBias = 0.0005f; // Reducido un poco para evitar que la sombra se separe del objeto
float ShadowStrength = 0.55f;

Texture2D ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
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

struct ShadowVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 Depth : TEXCOORD0; // X = Z de clip space, Y = W de clip space
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.lightPosition = mul(worldPosition, LightViewProjection);

    output.TextureCoordinate = input.TextureCoordinate;
    output.normal = normalize(mul(input.normal, (float3x3)WorldInverseTranspose));
    output.worldPosition = worldPosition.xyz;
    return output;
}

VertexShaderOutput MainUnlitVS(in UnlitVertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.lightPosition = mul(worldPosition, LightViewProjection);

    output.TextureCoordinate = input.TextureCoordinate;
    output.normal = float3(0.0, 1.0, 0.0);
    output.worldPosition = worldPosition.xyz;
    return output;
}

// FIX: Función de conversión homogénea limpia y unificada para MonoGame
float FinalizeDepth(float4 lightSpacePos)
{
#if OPENGL
    // Convierte el rango nativo de OpenGL [-1, 1] al rango estándar de textura [0, 1]
    return (lightSpacePos.z / lightSpacePos.w) * 0.5 + 0.5;
#else
    // DirectX ya devuelve nativamente [0, 1]
    return lightSpacePos.z / lightSpacePos.w;
#endif
}

ShadowVertexShaderOutput ShadowVS(in VertexShaderInput input)
{
    ShadowVertexShaderOutput output = (ShadowVertexShaderOutput)0;
    float4 lightPosition = mul(mul(input.Position, World), LightViewProjection);
    output.Position = lightPosition;
    
    // FIX: Pasamos Z y W de forma directa para evitar errores de interpolación no lineal
    output.Depth = lightPosition.zw; 
    return output;
}

float4 ShadowPS(ShadowVertexShaderOutput input) : COLOR
{
    // FIX: Reconstruimos la profundidad homogénea de forma segura en el Pixel Shader
#if OPENGL
    float depth = (input.Depth.x / input.Depth.y) * 0.5 + 0.5;
#else
    float depth = input.Depth.x / input.Depth.y;
#endif
    return float4(depth, depth, depth, 1.0);
}

float GetShadowFactor(float4 lightPosition)
{
    if (lightPosition.w <= 0.0)
    {
        return 1.0;
    }

    float3 projectionCoordinates = lightPosition.xyz / lightPosition.w;
    
    // Invertir Y para mapear espacio de proyección a coordenadas UV de textura
    float2 shadowTexCoord = projectionCoordinates.xy * float2(0.5, -0.5) + 0.5;

    // FIX: Usamos la nueva función unificada para leer la profundidad del píxel actual
    float currentDepth = FinalizeDepth(lightPosition);

    // Fuera de los límites del mapa de sombras = No hay sombra
    if (shadowTexCoord.x < 0.0 || shadowTexCoord.x > 1.0 || shadowTexCoord.y < 0.0 || shadowTexCoord.y > 1.0 || currentDepth < 0.0 || currentDepth > 1.0)
    {
        return 1.0;
    }

    // Aplicar Bias para evitar el z-fighting (shadow acne)
    currentDepth -= ShadowBias;
    
    // Leer el mapa de sombras grabado previamente
    float closestDepth = tex2D(shadowSampler, shadowTexCoord).r;
    
    // Si la profundidad actual es mayor que la más cercana grabada, el píxel está en sombra
    return currentDepth <= closestDepth ? 1.0 : (1.0 - ShadowStrength);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 pixelColor = tex2D(textureSampler, input.TextureCoordinate);
    if (!UseLighting)
    {
        return pixelColor;
    }

    float3 ambientColor = float3(0.2, 0.25, 0.35);
    // FIX: Vector L corregido para que apunte HACIA la luz de forma correcta
    float3 L = normalize(-float3(1.0, 1.0, 1.0)); 
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
    
    // Calculamos el factor de sombra
    float shadowFactor = GetShadowFactor(input.lightPosition);
    
    // El factor de sombra afecta al difuso y al especular, pero NUNCA al ambiental
    float3 diffuseLight = saturate(dot(L, N)) * diffuseColor * kd * shadowFactor;
    float3 specularLight = pow(saturate(dot(halfVector, N)), shininess) * specularColor * ks * step(0.0, dot(N, L)) * shadowFactor;
    
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

technique TexturedUnlitDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainUnlitVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique ShadowMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL ShadowVS();
        PixelShader = compile PS_SHADERMODEL ShadowPS();
    }
};
