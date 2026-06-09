using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaDerechaNieve : CurvaDerecha, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaDerechaNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaDerechaNieve.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        CurvaDerechaNieve.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader", Color.WhiteSmoke);
        CurvaDerechaNieve.LoadModel(content, "road-tiles/road-curve", AssetPaths.ContentFolder3D + "road-tiles/road-curve", AssetPaths.ContentFolderEffects + "BasicShader", Color.LightBlue);
        CurvaDerechaNieve.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "BasicShader", Color.Yellow);
    }

    public CurvaDerechaNieve(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position,rotation, biome)
    {
        NextTileOffset = Vector3.Transform(new Vector3(1200f, 0f, 0f), Matrix.CreateRotationY(rotation));
        
        //Piso y Autopista 
        AddObject(modelMap.GetValueOrDefault("road-tiles/road-square", null), 
            new Vector3(12f),
            Vector3.Zero,
            0f
        );

        AddObject(
            modelMap.GetValueOrDefault("road-tiles/road-curve", null), 
            new Vector3(5f),
            new Vector3(-130f, 20f, 250f),
            rotation + MathHelper.PiOver2
        );
        //Edificios 
        AddObject( 
            modelMap.GetValueOrDefault("buildings/suburban/building-type-s", null), 
            new Vector3(2f),
            new Vector3(322f, 10f, 450f), rotation + MathHelper.Pi
        );

        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Coin))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin, this.Position + new Vector3(0f, 0f, -600f), 10f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.FuelTank))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.FuelTank, this.Position + new Vector3(0f, 0f, -600f), 100f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Wrench))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Wrench, this.Position + new Vector3(0f, 0f, -600f), 50f));
        }
    }
}