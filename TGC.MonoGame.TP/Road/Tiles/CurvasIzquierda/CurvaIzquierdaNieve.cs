using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaIzquierdaNieve : CurvaIzquierda, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaIzquierdaNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        CurvaIzquierdaNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaIzquierdaNieve.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        var roadColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        var suburbanColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "buildings/suburban/Textures/colormap");
        CurvaIzquierdaNieve.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader", Color.WhiteSmoke);
        CurvaIzquierdaNieve.LoadModel(content, "road-tiles/road-curve", AssetPaths.ContentFolder3D + "road-tiles/road-curve", AssetPaths.ContentFolderEffects + "TexturedShader", roadColormap);
        CurvaIzquierdaNieve.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
    }

    public CurvaIzquierdaNieve(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position,rotation, biome)
    {
        NextTileOffset = Vector3.Transform(new Vector3(-1200f, 0f, 0f), Matrix.CreateRotationY(rotation));

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
            rotation //Rotar 2pi = rotar 0
        );
        //Edificios 
        AddObject( 
            modelMap.GetValueOrDefault("buildings/suburban/building-type-s", null), 
            new Vector3(2f),
            new Vector3(322f, 10f, 450f), rotation + MathHelper.Pi
        );

        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Coin))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin, this.Position + Vector3.Transform(new Vector3(0f, 50f, -600f), Matrix.CreateRotationY(rotation)), 10f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.FuelTank))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.FuelTank, this.Position + Vector3.Transform(new Vector3(0f, 50f, -600f), Matrix.CreateRotationY(rotation)), 100f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Wrench))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Wrench, this.Position + Vector3.Transform(new Vector3(0f, 50f, -600f), Matrix.CreateRotationY(rotation)), 50f));
        }
    }
}