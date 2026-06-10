using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class RectaTierra : Recta, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        RectaTierra.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        RectaTierra.LoadModel(content, path, path, effect, color);
    }   

    public static void LoadLocalModels(ContentManager content)
    {
        RectaTierra.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkGreen);
        RectaTierra.LoadModel(content, "road-tiles/road-straight", AssetPaths.ContentFolder3D + "road-tiles/road-straight",  AssetPaths.ContentFolderEffects + "BasicShader", Color.Chocolate);
        RectaTierra.LoadModel(content, "buildings/suburban/building-type-c", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-c", AssetPaths.ContentFolderEffects + "BasicShader", Color.LightCoral);        
        RectaTierra.LoadModel(content, "buildings/suburban/planter", AssetPaths.ContentFolder3D + "buildings/suburban/planter", AssetPaths.ContentFolderEffects + "BasicShader", Color.Green);
        RectaTierra.LoadModel(content, "buildings/suburban/fence", AssetPaths.ContentFolder3D + "buildings/suburban/fence", AssetPaths.ContentFolderEffects + "BasicShader", Color.Brown);
   
    }

    public RectaTierra(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position,rotation, biome)
    {
        NextTileOffset = Vector3.Transform(new Vector3(0f, 0f, -1200f), Matrix.CreateRotationY(rotation));

               AddObject(
            modelMap.GetValueOrDefault("road-tiles/road-square", null),
            new Vector3(12f),
            Vector3.Zero,
            rotation
        );

        // RUTA

        AddObject(
            modelMap.GetValueOrDefault("road-tiles/road-straight", null),
            new Vector3(12f, 12f, 5f),
            new Vector3(0f, 12f, 5f),
            rotation + MathHelper.PiOver2
        );

        // EDIFICIOS

        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(460f, 10f, 250f),
            rotation,
            new Vector3(300f, 300f, 300f),
            new Vector3(0f, 150f, 0f),
            50f,
            0f,
            true
        );

        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(-460f, 10f, 250f),
            rotation,
            new Vector3(300f, 300f, 300f),
            new Vector3(0f, 150f, 0f),
            50f,
            0f,
            true
        );

        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(460f, 10f, -250f),
            rotation,
            new Vector3(300f, 300f, 300f),
            new Vector3(0f, 150f, 0f),
            50f,
            0f,
            true
        );

        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(-460f, 10f, -250f),
            rotation,
            new Vector3(300f, 300f, 300f),
            new Vector3(0f, 150f, 0f),
            50f,
            0f,
            true
        );


        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f),
            new Vector3(460f, 50f, -800f),
            rotation
        );

        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f),
            new Vector3(-460f, 50f, -800f),
            rotation
        );

        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/fence", null),
            new Vector3(6f),
            new Vector3(-460f, 10f, 0f),
            rotation,
            new Vector3(300f, 300f, 300f),
            new Vector3(0f, 150f, 0f),
            50f,
            0f,
            true
        );

        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/fence", null),
            new Vector3(6f),
            new Vector3(460f, 10f, 0f),
            rotation
        );
        


        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Coin))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin, this.Position + Vector3.Transform(new Vector3(0f, 50f, -600f), Matrix.CreateRotationY(rotation)), 10f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.FuelTank))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.FuelTank, this.Position + Vector3.Transform(new Vector3(0f, 50f, -300f), Matrix.CreateRotationY(rotation)), 100f));
        }
        if(biome.ShouldSpawnCollectibleOfType(CollectibleType.Wrench))
        {
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Wrench, this.Position + Vector3.Transform(new Vector3(0f, 50f, -600f), Matrix.CreateRotationY(rotation)), 50f));
        }
    }
}