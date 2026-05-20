using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class TileCurvaDerecha : Tile
{
    public override TileType GetTileType()
    {
        return TileType.STRAIGHT_LINE;
    }


    public TileCurvaDerecha(
        ContentManager content,
        Vector3 position
    ) : base(content, position)
    {
       NextTileOffset =
            new Vector3(1200f, 0, -1200f); //            new Vector3(1200f, 0f, -500f);

        AddModel(AssetPaths.ContentFolder3D + "road-tiles/road-square", AssetPaths.ContentFolderEffects + "BasicShader",
            Color.DarkGreen);
        AddModel(AssetPaths.ContentFolder3D + "road-tiles/road-curve", AssetPaths.ContentFolderEffects + "BasicShader",
            Color.Gray);

        AddModel(AssetPaths.ContentFolder3D + "buildings/suburban/building-type-s",
            AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddModel(AssetPaths.ContentFolder3D + "buildings/suburban/building-type-u",
            AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddModel(AssetPaths.ContentFolder3D + "buildings/suburban/building-type-d",
            AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddModel(AssetPaths.ContentFolder3D + "buildings/suburban/building-type-a",
            AssetPaths.ContentFolderEffects + "BasicShader", Color.DarkBlue);

        //Piso y Autopista 
        AddObject(_tileModels[0], 
            new Vector3(12f),
            Vector3.Zero,
            0f
        );

        AddObject(
            _tileModels[1], 
            new Vector3(5f),
            new Vector3(380f, 25f, 0f),
            MathHelper.PiOver2
        );
        //Edificios 
        AddObject( 
            _tileModels[2], 
            new Vector3(2f),
            new Vector3(322f, 10f, 450f), MathHelper.Pi
        );
    }
}