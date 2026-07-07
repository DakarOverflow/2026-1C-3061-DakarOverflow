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

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        RectaTierra.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        RectaTierra.LoadModel(content, path, path, effect, color);
    }   

    public static void LoadLocalModels(ContentManager content)
    {
                        var roadColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        var suburbanColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "buildings/suburban/Textures/colormap");
        var towerdefenseColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "tower-defense-kit/Textures/colormap");

        // BASE
        RectaTierra.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkGreen);
        
        // RUTA
        RectaTierra.LoadModel(content, "tower-defense-kit/tile-straight", AssetPaths.ContentFolder3D + "tower-defense-kit/tile-straight",  AssetPaths.ContentFolderEffects + "TexturedShader", towerdefenseColormap);

        LoadModel(content, "road-tiles/light-curved", AssetPaths.ContentFolder3D + "road-tiles/light-curved",
            AssetPaths.ContentFolderEffects + "TexturedShader", roadColormap);
        LoadModel(content, "buildings/suburban/building-type-c", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-c",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/building-type-n", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-n",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/planter", AssetPaths.ContentFolder3D + "buildings/suburban/planter",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/driveway-long", AssetPaths.ContentFolder3D + "buildings/suburban/driveway-long",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/tree-small", AssetPaths.ContentFolder3D + "buildings/suburban/tree-small",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/tree-tall", AssetPaths.ContentFolder3D + "buildings/suburban/tree-large",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/fence-3x3", AssetPaths.ContentFolder3D + "buildings/suburban/fence-3x3",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
        LoadModel(content, "buildings/suburban/path-stones-messy", AssetPaths.ContentFolder3D + "buildings/suburban/path-stones-messy",
            AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
    }

    public RectaTierra(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position,rotation, biome)
    {
        NextTileOffset = Vector3.Transform(new Vector3(0f, 0f, -1200f), Matrix.CreateRotationY(rotation));

        // BASE
        AddObject(modelMap.GetValueOrDefault("road-tiles/road-square", null), 
            new Vector3(12f),
            Vector3.Zero,
            rotation,
            MathHelper.Pi
        );

        // RUTA
        AddObject(
            modelMap.GetValueOrDefault("tower-defense-kit/tile-straight", null),
            new Vector3(5.5f, 1f, 12f), // Escalado
            new Vector3(0f, -5f, 0f), // Posicion en eje Y
            rotation
        );

        // DECO;
       
         // EDIFICIO 1;
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(400f, 0f, 450f),
            rotation + MathHelper.PiOver2,
            new Vector3(180f, 100f, 260f),
            new Vector3(25f, 50f, 0f),
            0f,
            0f,
            true
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/fence-3x3", null),
            new Vector3(3f, 2f, 2f),
            new Vector3(450f, 0f, 410f),
            rotation + MathHelper.PiOver2,
            new Vector3(230f, 100f, 360f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/tree-tall", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(517f, 0f, 277f),
            rotation,
            new Vector3(50f, 100f, 50f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/path-stones-messy", null),
            new Vector3(2f, 1f, 2f),
            new Vector3(296f, 0f, 450f),
            rotation + MathHelper.PiOver2
        );
        // Edificio 2
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-n", null),
            new Vector3(2f),
            new Vector3(430f, 0f, -350f),
            rotation + MathHelper.PiOver2,
            new Vector3(240f, 100f, 360f),
            new Vector3(0f, 50f, 0f),
            0f,
            0f,
            true
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/fence-3x3", null),
            new Vector3(3.5f, 2f, 2f),
            new Vector3(455f, 0f, -352f),
            rotation + MathHelper.PiOver2,
            new Vector3(250f, 100f, 420f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/tree-small", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(304f, 0f, -240f),
            rotation,
            new Vector3(50f, 100f, 50f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/path-stones-messy", null),
            new Vector3(2f, 1f, 2f),
            new Vector3(328f, 0f, -326f),
            rotation + MathHelper.PiOver2
        );
        // Edificio 3
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/driveway-long", null),
            new Vector3(2f, 1f, 8f),
            new Vector3(400f, 5f, 50f),
            rotation + MathHelper.PiOver2
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(500f, 0f, -50f),
            rotation,
            new Vector3(150f, 100f, 100f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(380f, 0f, 150f),
            rotation,
            new Vector3(150f, 100f, 100f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        // Luces
        AddObstacle(
            modelMap.GetValueOrDefault("road-tiles/light-curved", null),
            new Vector3(5f, 5f, 7f),
            new Vector3(229f, 22f, 300f),
            rotation + MathHelper.PiOver2,
            new Vector3(70f, 240f, 70f),
            new Vector3(0f, 120f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("road-tiles/light-curved", null),
            new Vector3(5f, 5f, 7f),
            new Vector3(229f, 22f, -300f),
            rotation + MathHelper.PiOver2,
            new Vector3(70f, 240f, 70f),
            new Vector3(0f, 120f, 0f),
            10f,
            0f
        );
        // Other side
       // EDIFICIO 1
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-c", null),
            new Vector3(2f),
            new Vector3(-400f, 0f, 450f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(180f, 100f, 260f),
            new Vector3(25f, 50f, 0f),
            0f,
            0f,
            true
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/fence-3x3", null),
            new Vector3(3f, 2f, 2f),
            new Vector3(-450f, 0f, 410f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(230f, 100f, 360f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/tree-tall", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(-517f, 0f, 277f),
            rotation + MathHelper.Pi,
            new Vector3(50f, 100f, 50f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/path-stones-messy", null),
            new Vector3(2f, 1f, 2f),
            new Vector3(-296f, 0f, 450f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi
        );
        // Edificio 2
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/building-type-n", null),
            new Vector3(2f),
            new Vector3(-430f, 0f, -350f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(240f, 100f, 360f),
            new Vector3(0f, 50f, 0f),
            0f,
            0f,
            true
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/fence-3x3", null),
            new Vector3(3.5f, 2f, 2f),
            new Vector3(-455f, 0f, -352f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(250f, 100f, 420f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/tree-small", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(-304f, 0f, -470f),
            rotation + MathHelper.Pi,
            new Vector3(50f, 100f, 50f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/path-stones-messy", null),
            new Vector3(2f, 1f, 2f),
            new Vector3(-328f, 0f, -326f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi
        );
        // Edificio 3
        AddObject(
            modelMap.GetValueOrDefault("buildings/suburban/driveway-long", null),
            new Vector3(2f, 1f, 8f),
            new Vector3(-400f, 5f, 50f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(-500f, 0f, -50f),
            rotation + MathHelper.Pi,
            new Vector3(150f, 100f, 100f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("buildings/suburban/planter", null),
            new Vector3(4f, 4f, 4f),
            new Vector3(-380f, 0f, 150f),
            rotation + MathHelper.Pi,
            new Vector3(150f, 100f, 100f),
            new Vector3(0f, 50f, 0f),
            10f,
            0f
        );
        // Luces
        AddObstacle(
            modelMap.GetValueOrDefault("road-tiles/light-curved", null),
            new Vector3(5f, 5f, 7f),
            new Vector3(-229f, 22f, 300f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(70f, 240f, 70f),
            new Vector3(0f, 120f, 0f),
            10f,
            0f
        );
        AddObstacle(
            modelMap.GetValueOrDefault("road-tiles/light-curved", null),
            new Vector3(5f, 5f, 7f),
            new Vector3(-229f, 22f, -300f),
            rotation + MathHelper.PiOver2 + MathHelper.Pi,
            new Vector3(70f, 240f, 70f),
            new Vector3(0f, 120f, 0f),
            10f,
            0f
        );

        var collectibleToSpawn = biome.GetCollectibleToSpawn();
        switch (collectibleToSpawn)
        {
            case CollectibleType.Coin:
 AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 540f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 480f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 420f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 360f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 300f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 240f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(75f, 50f, 180f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(-75f, 50f, -330f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(-75f, 50f, -380f), Matrix.CreateRotationY(rotation)), 10f));
            AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Coin,
                this.Position + Vector3.Transform(new Vector3(-75f, 50f, -430f), Matrix.CreateRotationY(rotation)), 10f));
                break;
            case CollectibleType.FuelTank:
                AddObject(Collectible.CreateCollectibleOfType(CollectibleType.FuelTank,
                    this.Position + Vector3.Transform(new Vector3(-80f, 50f, 380f), Matrix.CreateRotationY(rotation)), 40f));
                break;
            case CollectibleType.Wrench:
                AddObject(Collectible.CreateCollectibleOfType(CollectibleType.Wrench,
                    this.Position + Vector3.Transform(new Vector3(134f, 50f, -490f), Matrix.CreateRotationY(rotation)), 50f));
                break;
        }
    }
}