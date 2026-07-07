using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaIzquierdaTierraAsfalto : CurvaIzquierda, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaIzquierdaTierraAsfalto.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture)
    {
        CurvaIzquierdaTierraAsfalto.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture
        ));
    }

    private static void LoadModel(ContentManager content, string key, string path, string effect, Texture2D texture, Texture2D overlayTexture)
    {
        CurvaIzquierdaTierraAsfalto.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            texture,
            overlayTexture
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaIzquierdaTierraAsfalto.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        var roadColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        var suburbanColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "buildings/suburban/Textures/colormap");
        var overlayTexture = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/dirt-particles");
        CurvaIzquierdaTierraAsfalto.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "TexturedShader", Color.DarkGreen);
        CurvaIzquierdaTierraAsfalto.LoadModel(content, "road-tiles/road-curve", AssetPaths.ContentFolder3D + "road-tiles/road-curve", AssetPaths.ContentFolderEffects + "TexturedShader", roadColormap, overlayTexture);
        CurvaIzquierdaTierraAsfalto.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "TexturedShader", suburbanColormap);
    }

    public CurvaIzquierdaTierraAsfalto(
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
            modelMap.GetValueOrDefault("road-tiles/road-curve", null), 
            new Vector3(5f, 5f, 5f),
            new Vector3(-100f, 12f, 100f),
            rotation //Rotar 2pi = rotar 0
        );
    }
}