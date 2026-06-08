using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BepuPhysics;

namespace TGC.MonoGame.TP;

public class CurvaIzquierdaAsfalto : CurvaIzquierda, IAssetLoader
{
    protected static Dictionary<String, CustomModel> modelMap = new Dictionary<String, CustomModel>() {};
    
    private static void LoadModel(ContentManager content, string key, string path, string effect, Color color)
    {
        CurvaIzquierdaAsfalto.modelMap.Add(key, new CustomModel(
            content.Load<Model>(path),
            content.Load<Effect>(effect),
            color
        ));
    }

    private static void LoadModel(ContentManager content, string path, string effect, Color color)
    {
        //Genera el modelo utilizando el path del mismo como key para el diccionario interno
        CurvaIzquierdaAsfalto.LoadModel(content, path, path, effect, color);
    }

    public static void LoadLocalModels(ContentManager content)
    {
        CurvaIzquierdaAsfalto.LoadModel(content, "road-tiles/road-square", AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkGreen);
        CurvaIzquierdaAsfalto.LoadModel(content, "road-tiles/road-curve", AssetPaths.ContentFolder3D + "road-tiles/road-curve", AssetPaths.ContentFolderEffects + "BasicShader", Color.Gray);
        CurvaIzquierdaAsfalto.LoadModel(content, "buildings/suburban/building-type-s", AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s", AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkBlue);
    }

    public CurvaIzquierdaAsfalto(
        Vector3 position,
        float rotation
    ) : base(position,rotation)
    {
        NextTileOffset = Vector3.Transform(new Vector3(0f, 0f, -1200f), Matrix.CreateRotationY(rotation));

        NextTileRotation = MathHelper.PiOver2;

        //Piso y Autopista 
        AddObject(modelMap.GetValueOrDefault("road-tiles/road-square", null), 
            new Vector3(12f),
            Vector3.Zero,
            0f
        );

        AddObject(
            modelMap.GetValueOrDefault("road-tiles/road-curve", null), 
            new Vector3(5f),
            new Vector3(380f, 25f, 0f),
            rotation //Rotar 2pi = rotar 0
        );
        //Edificios 
        AddObject( 
            modelMap.GetValueOrDefault("buildings/suburban/building-type-s", null), 
            new Vector3(2f),
            new Vector3(322f, 10f, 450f), rotation + MathHelper.Pi
        );
    }
}