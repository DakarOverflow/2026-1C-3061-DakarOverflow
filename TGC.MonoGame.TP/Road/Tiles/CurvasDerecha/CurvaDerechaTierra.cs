using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaDerechaTierra : CurvaDerecha, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaDerechaTierra.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        CurvaDerechaTierra.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaDerechaTierra.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        var roadColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        var suburbanColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "buildings/suburban/Textures/colormap");
        var towerdefenseColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "tower-defense-kit/Textures/colormap");
        
        // BASE
        CurvaDerechaTierra.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "TexturedShader", Color.DarkGreen);
        
        // RUTA
        CurvaDerechaTierra.LoadModel(content, "tower-defense-kit/tile-corner-large", AssetPaths.ContentFolder3D + "tower-defense-kit/tile-corner-large", AssetPaths.ContentFolderEffects + "TexturedShader", towerdefenseColormap);
        
        // DECO
        CurvaDerechaTierra.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
    }

    public CurvaDerechaTierra(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position,rotation, biome)
    {
        NextTileOffset = Vector3.Transform(new Vector3(1200f, 0f, 0f), Matrix.CreateRotationY(rotation));
        
        // BASE 
        AddObject(modelMap.GetValueOrDefault("road-tiles/road-square", null), 
            new Vector3(12f),
            Vector3.Zero,
            rotation + MathHelper.PiOver2,
            MathHelper.Pi
        );

        // RUTA
        AddObject(
            modelMap.GetValueOrDefault("tower-defense-kit/tile-corner-large", null), 
            new Vector3(5f, 1f, 5f), // Escalado
            new Vector3(100f, -5f, 100f), // Posicion en Y
            rotation
        );
        // DECO
    }
}