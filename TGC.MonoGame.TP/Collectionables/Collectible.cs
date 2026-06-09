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

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        Collectible.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        Collectible.LoadModel(content, "car-kit/box", AssetPaths.ContentFolder3D + "car-kit/box", AssetPaths.ContentFolderEffects + "BasicShader", Color.Red);
        Collectible.LoadModel(content, "car-kit/debris-bolt", AssetPaths.ContentFolder3D + "car-kit/debris-bolt", AssetPaths.ContentFolderEffects + "BasicShader", Color.Gray);
        Collectible.LoadModel(content, "car-kit/debris-nut", AssetPaths.ContentFolder3D + "car-kit/debris-nut", AssetPaths.ContentFolderEffects + "BasicShader", Color.Gold);
    }
    
    
    // Atributos
    public CollectibleType Type { get; private set; }
    
    // True por defecto, cambia a false cuando el jugador lo recoge. Esto define si se muestra en pantalla.
    public bool IsActive { get; private set; }

    public Vector3 Position { get; private set; }

    // Valor de Combustible, Salud o Puntos que otorga el coleccionable
    public float EffectValue { get; private set; }

    // --- PREPARACIÓN PARA COLISIONES ---
    // TODO: definir atributo

    
    public static Collectible CreateCollectibleOfType(CollectibleType type, Vector3 position, float effectValue)
    {
        switch (type)
        {
            case CollectibleType.FuelTank:
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/box", null), position, effectValue);
            
            case CollectibleType.Wrench:
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/debris-bolt", null), position, effectValue);
            
            case CollectibleType.Coin:
                return new Collectible(type, modelMap.GetValueOrDefault("car-kit/debris-nut", null), position, effectValue);
            
            default:
                throw new ArgumentException("Tipo de coleccionable desconocido: " + type);
        }
    }

    public Collectible(CollectibleType type, CustomModel model, Vector3 initialPosition, float effectValue) : base(model, Matrix.CreateTranslation(initialPosition))
    {
        Type = type;
        Position = initialPosition;
        EffectValue = effectValue;
        IsActive = true;
        
        // --- PREPARACIÓN PARA COLISIONES ---
        // Inicializar el atributo de colisiones.
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
