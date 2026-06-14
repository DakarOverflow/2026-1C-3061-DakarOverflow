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
        var colormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "car-kit/Textures/colormap");
        Collectible.LoadModel(content, "car-kit/box", AssetPaths.ContentFolder3D + "car-kit/box", AssetPaths.ContentFolderEffects + "TexturedShader", colormap);
        Collectible.LoadModel(content, "car-kit/debris-bolt", AssetPaths.ContentFolder3D + "car-kit/debris-bolt", AssetPaths.ContentFolderEffects + "TexturedShader", colormap);
        Collectible.LoadModel(content, "car-kit/debris-nut", AssetPaths.ContentFolder3D + "car-kit/debris-nut", AssetPaths.ContentFolderEffects + "TexturedShader", colormap);
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
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/box", null), position, 1f, effectValue);
            
            case CollectibleType.Wrench:
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/debris-bolt", null), position, 5f,  effectValue);
            
            case CollectibleType.Coin:
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/debris-nut", null), position, 5f, effectValue);
            
            default:
                throw new ArgumentException("Tipo de coleccionable desconocido: " + type);
        }
    }

    public Collectible(CollectibleType type, CustomModel model, Vector3 initialPosition, float scale, float effectValue) : base(model, Matrix.CreateScale(scale) * Matrix.CreateTranslation(initialPosition))
    {
        Type = type;
        Position = initialPosition;
        EffectValue = effectValue;
        IsActive = true;
        
        // --- PREPARACIÓN PARA COLISIONES ---
        UpdateBoundingBox();
    }

    private void UpdateBoundingBox()
    {
        BoundingBox = new BoundingBox(Position - _boundingBoxHalfSize, Position + _boundingBoxHalfSize);
    }

    // Update actualizar el estado del coleccionable
    public override void Update(GameTime gameTime)
    {
        // Si ya fue recogido, no hacemos nada
        if (!IsActive) return;

        // --- PREPARACIÓN PARA LA 4TA ENTREGA ---
        // TODO: Los coleccionables deben tener rotación en Y y rebote vertical.
        // TODO: Como el modelo va a estar rebotando y rotando, hay que actualizar la posición del BoundingBox también.
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
                break;
            
            case CollectibleType.Wrench:
                // Restaura puntos de salud
                player.RepairDamage(EffectValue);
                break;
            
            case CollectibleType.Coin:
                // Incrementa puntaje, se trabaja en numeros enteros.
                player.AddScore((int)EffectValue);
                break;
        }
    }
}
