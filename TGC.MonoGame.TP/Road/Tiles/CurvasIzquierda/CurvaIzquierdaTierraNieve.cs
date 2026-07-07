using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaIzquierdaTierraNieve : CurvaIzquierda, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaIzquierdaTierraNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        CurvaIzquierdaTierraNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture, Texture2D overlayTexture)
    {
        CurvaIzquierdaTierraNieve.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture,
            overlayTexture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaIzquierdaTierraNieve.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        var roadColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        var suburbanColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "buildings/suburban/Textures/colormap");
        var overlayTexture = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/snow-texture-on-black-background");
        var towerdefenseColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "tower-defense-kit/Textures/colormap");
        CurvaIzquierdaTierraNieve.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "TexturedShader", Color.DarkGreen);
        CurvaIzquierdaTierraNieve.LoadModel(content, "tower-defense-kit/tile-corner-large", AssetPaths.ContentFolder3D + "tower-defense-kit/tile-corner-large", AssetPaths.ContentFolderEffects + "TexturedShader", towerdefenseColormap, overlayTexture);
        CurvaIzquierdaTierraNieve.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
    }

    public CurvaIzquierdaTierraNieve(
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
            rotation,
            MathHelper.Pi
        );


        AddObject(
            modelMap.GetValueOrDefault("tower-defense-kit/tile-corner-large", null), 
            new Vector3(5f, 1f, 5f), // Escala
            new Vector3(-100f, -5f, 100f), // Posicion
            rotation - MathHelper.PiOver2
        );

    }
}