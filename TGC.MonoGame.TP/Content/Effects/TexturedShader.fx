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
float ShadowBias = 0.0008f;
float ShadowStrength = 0.55f;
// Tiene que coincidir con ShadowMapRenderer (tamano del render target del shadow map)
float ShadowMapSize = 1024.0f;
// Tiene que coincidir con ShadowMapRenderer.LightDirection
float3 LightDirection = float3(1.0f, 1.0f, 1.0f);
// DEBUG: 0 = normal, 1 = ver normales, 2 = ver factor difuso, 3 = textura fullbright
float DebugView = 0.0f;
// Color plano para la técnica BasicColorDrawing (piso road-square, sin textura).
float3 DiffuseColor = float3(1.0f, 1.0f, 1.0f);

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

float GetShadowFactor(float4 lightPosition, float3 Normal, float3 LightDirection)
{
    if (lightPosition.w <= 0.0)
    {
        return 1.0;
    }

    float3 projectionCoordinates = lightPosition.xyz / lightPosition.w;

    // Invertir Y para mapear espacio de proyección a coordenadas UV de textura
    float2 shadowTexCoord = projectionCoordinates.xy * float2(0.5, -0.5) + 0.5;

    // Profundidad homogénea del píxel actual
    float currentDepth = FinalizeDepth(lightPosition);

    // Fuera de los límites del mapa de sombras = No hay sombra
    if (shadowTexCoord.x < 0.0 || shadowTexCoord.x > 1.0 || shadowTexCoord.y < 0.0 || shadowTexCoord.y > 1.0 || currentDepth < 0.0 || currentDepth > 1.0)
    {
        return 1.0;
    }

    // Bias dependiente de la pendiente, más grande en superficies rasantes a la luz,
    // para evitar shadow acne sin generar demasiado peter-panning en superficies frontales
    float slope = 1.0 - saturate(dot(Normal, LightDirection));
    float bias = ShadowBias + ShadowBias * 4.0 * slope;
    currentDepth -= bias;

    // Filtrado PCF 3x3, promediamos 9 muestras alrededor del texel para suavizar el borde
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

// Iluminación compartida por la técnica texturizada y la de color plano.
// albedo = color base (textura o DiffuseColor). Devuelve el color final RGB.
float3 ShadeSurface(float3 albedo, float3 rawNormal, float3 worldPosition, float4 lightPosition)
{
    // Ambiente alto = piso de brillo: ninguna superficie queda negra aunque no mire al sol.
    float3 ambientColor = float3(0.55, 0.57, 0.62);
    // lightDir: superficie -> luz. +(1,1,1) = sol desde arriba, consistente con las sombras.
    float3 lightDir = normalize(LightDirection);
    float3 diffuseColorK = float3(0.6, 0.6, 0.6);
    float3 specularColor = float3(0.5, 0.5, 0.5);
    float shininess = 32.0;

    float3 Normal = normalize(rawNormal);
    float3 viewDirection = normalize(CameraPosition - worldPosition);
    // Two-sided: algunos modelos (el piso road-square) tienen las normales invertidas. Como
    // dibujamos con backface culling, la cara visible siempre mira a la cámara: si su normal
    // apunta al lado contrario, la damos vuelta. Así todo se ilumina de forma coherente.
    if (dot(Normal, viewDirection) < 0.0)
        Normal = -Normal;
    float3 halfVector = normalize(viewDirection + lightDir);

    // DEBUG VIEWS (tecla U en el juego)
    if (DebugView > 0.5 && DebugView < 1.5) return Normal * 0.5 + 0.5;              // 1: normales como color
    if (DebugView > 1.5 && DebugView < 2.5) return saturate(dot(lightDir, Normal)).xxx; // 2: factor difuso
    if (DebugView > 2.5) return albedo;                                            // 3: albedo sin iluminar

    float3 ambientLight = ambientColor;
    // Las sombras ya NO se calculan acá: se aplican como post-procesado (ShadowPostProcess.fx).
    float shadowFactor = 1.0;

    // Half-Lambert: mapea dot de [-1,1] a [0,1] envolviendo la luz, así el lado opuesto
    // al sol queda tenue en vez de negro. La sombra afecta difuso y especular, nunca al ambiente.
    float ndotl = dot(Normal, lightDir);
    float diffuseFactor = saturate(ndotl * 0.5 + 0.5);
    float3 diffuseLight = diffuseFactor * diffuseColorK * shadowFactor;
    float3 specularLight = pow(saturate(dot(halfVector, Normal)), shininess) * specularColor * step(0.0, ndotl) * shadowFactor;

    return (ambientLight + diffuseLight) * albedo + specularLight;
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
    overlayColor.a = (brightness < 0.05f) ? 0.0f : 1.0f;

    if(UseOverlay)
    {
        finalColor = lerp(baseColor, overlayColor, overlayColor.a);
    }

    if (!UseLighting)
    {
        return finalColor;
    }

    return float4(ShadeSurface(finalColor.xyz, input.normal, input.worldPosition, input.lightPosition), finalColor.a);
}

// Técnica de color plano CON iluminación (para el piso road-square, que antes usaba BasicShader).
struct ColorVertexShaderInput
{
    float4 Position : POSITION0;
    float3 normal : NORMAL;
};

VertexShaderOutput ColorVS(in ColorVertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.lightPosition = mul(worldPosition, LightViewProjection);

    output.TextureCoordinate = float2(0.0, 0.0);
    output.normal = normalize(mul(input.normal, (float3x3)WorldInverseTranspose));
    output.worldPosition = worldPosition.xyz;
    return output;
}

float4 ColorPS(VertexShaderOutput input) : COLOR
{
    return float4(ShadeSurface(DiffuseColor, input.normal, input.worldPosition, input.lightPosition), 1.0);
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

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL ColorVS();
		PixelShader = compile PS_SHADERMODEL ColorPS();
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
