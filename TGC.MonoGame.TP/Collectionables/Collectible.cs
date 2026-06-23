using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;
public class Collectible : WorldObject, IAssetLoader
{   
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        Collectible.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        Collectible.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        Collectible.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        // Se carga el colormap de cada kit y se cargan los modelos de los coleccionables aplicando la textura del colormap correspondiente.

        var survivalKitColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "survival-kit/Textures/colormap");
        Collectible.LoadModel(content, "survival-kit/barrel", AssetPaths.ContentFolder3D + "survival-kit/barrel", AssetPaths.ContentFolderEffects + "TexturedShader", survivalKitColormap);
        Collectible.LoadModel(content, "survival-kit/tool-hammer", AssetPaths.ContentFolder3D + "survival-kit/tool-hammer", AssetPaths.ContentFolderEffects + "TexturedShader", survivalKitColormap);

        var toyCarKitColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "toy-car-kit/Textures/colormap");
        Collectible.LoadModel(content, "toy-car-kit/item-coin-gold", AssetPaths.ContentFolder3D + "toy-car-kit/item-coin-gold", AssetPaths.ContentFolderEffects + "TexturedShader", toyCarKitColormap);
    }
    
    
    // Atributos
    public CollectibleType Type { get; private set; }
    
    // True por defecto, cambia a false cuando el jugador lo recoge. Esto define si se muestra en pantalla.
    public bool IsActive { get; private set; }

    public Vector3 Position { get; private set; }

    // Valor de Combustible, Salud o Puntos que otorga el coleccionable
    public float EffectValue { get; private set; }

    // --- PREPARACIÓN PARA COLISIONES ---
    public BoundingBox BoundingBox { get; private set; }
    private readonly Vector3 _boundingBoxHalfSize = new Vector3(30f, 30f, 30f);

    
    public static Collectible CreateCollectibleOfType(CollectibleType type, Vector3 position, float effectValue)
    {
        switch (type)
        {
            case CollectibleType.FuelTank:
                return new Collectible(type, modelMap.GetValueOrDefault("survival-kit/barrel", null), position, 1.5f, effectValue);
            
            case CollectibleType.Wrench:
                return new Collectible(type, modelMap.GetValueOrDefault("survival-kit/tool-hammer", null), position, 3f,  effectValue);
            
            case CollectibleType.Coin:
                return new Collectible(type, modelMap.GetValueOrDefault("toy-car-kit/item-coin-gold", null), position, 1.5f, effectValue);
            
            default:
                throw new ArgumentException("Tipo de coleccionable desconocido: " + type);
        }
    }

    private float _scale;
    private Vector3 _modelCenterOffset;

    public Collectible(CollectibleType type, CustomModel model, Vector3 initialPosition, float scale, float effectValue) : base(model, Matrix.CreateScale(scale) * Matrix.CreateTranslation(initialPosition))
    {
        Type = type;
        Position = initialPosition;
        EffectValue = effectValue;
        IsActive = true;
        _scale = scale;
        _modelCenterOffset = CalculateModelCenter(model.Model);
        
        // --- PREPARACIÓN PARA COLISIONES ---
        UpdateBoundingBox();
    }

    private Vector3 CalculateModelCenter(Model model)
    {
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);
        foreach (var mesh in model.Meshes)
        {
            var sphere = mesh.BoundingSphere;
            min = Vector3.Min(min, sphere.Center - new Vector3(sphere.Radius));
            max = Vector3.Max(max, sphere.Center + new Vector3(sphere.Radius));
        }
        return (min + max) / 2f;
    }

    private void UpdateBoundingBox()
    {
        Vector3 currentPosition = World.Translation;
        BoundingBox = new BoundingBox(currentPosition - _boundingBoxHalfSize, currentPosition + _boundingBoxHalfSize);
    }

    // Update actualizar el estado del coleccionable
    public override void Update(GameTime gameTime)
    {
        // Si ya fue recogido, no hacemos nada
        if (!IsActive) return;

        // --- PREPARACIÓN PARA LA 4TA ENTREGA ---
        float time = (float)gameTime.TotalGameTime.TotalSeconds;
        
        // Efecto de rebote vertical
        float bounceHeight = 10f; 
        float bounceSpeed = 2f;
        float yOffset = (float)Math.Sin(time * bounceSpeed) * bounceHeight;

        // Efecto de rotación en Y
        float rotationSpeed = 1.5f;
        float rotationY = time * rotationSpeed;

        // Posición actual con el rebote
        Vector3 animatedPosition = Position + new Vector3(0, yOffset, 0);

        // Actualizar matriz World centrando el modelo antes de rotar
        setWorld(Matrix.CreateTranslation(-_modelCenterOffset) * Matrix.CreateScale(_scale) * Matrix.CreateRotationY(rotationY) * Matrix.CreateTranslation(animatedPosition));

        // Actualizar la posición del BoundingBox también
        UpdateBoundingBox();
    }

    // Draw dibuja el coleccionable si está activo
    public override void Draw(GameTime gameTime, Matrix view, Matrix projection)
    {
        if (!IsActive) return;
        base.Draw(gameTime, view, projection);
    }

    // --- PREPARACIÓN PARA COLISIONES ---
    // Método que se llamará al detectar una colisión entre el auto y el coleccionable
    // TODO: Definir quien llama a este método
    public void PickUp(Vehicle player)
    {
        if (!IsActive) return;

        IsActive = false; // Se desactiva el coleccionable para que desaparezca de la pista

        // Se aplica el efecto correspondiente a los medidores del jugador
        switch (Type)
        {
            case CollectibleType.FuelTank:
                // Restaura combustible
                player.AddFuel(EffectValue); 
                SoundManager.GetInstance().SonarCargaCombustible();
                break;
            
            case CollectibleType.Wrench:
                // Restaura puntos de salud
                player.RepairDamage(EffectValue);
                SoundManager.GetInstance().SonarReparacionAuto();
                break;
            
            case CollectibleType.Coin:
                // Incrementa puntaje, se trabaja en numeros enteros.
                player.AddScore((int)EffectValue);
                SoundManager.GetInstance().SonarRecoleccionMoneda();
                break;
        }
    }
}
