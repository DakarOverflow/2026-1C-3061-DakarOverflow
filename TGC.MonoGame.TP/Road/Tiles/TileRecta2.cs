using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class TileRecta2 : Tile
{
    public override TileType GetTileType()
    {
        return TileType.STRAIGHT_LINE2;
    }


    public TileRecta2(
        ContentManager content,
        Vector3 position,
        float rotation
    ) : base(content, position,rotation)
    {
        NextTileOffset = new Vector3(0f, 0f, -1200f);
        NextTileRotation = 0f;
        AddModel(
            AssetPaths.ContentFolder3D +
            "road-tiles/road-square",

            AssetPaths.ContentFolderEffects +
            "BasicShader",

            Color.DarkGreen
        );

        AddModel(
            AssetPaths.ContentFolder3D +
            "road-tiles/road-straight",

            AssetPaths.ContentFolderEffects +
            "BasicShader",

            Color.Gray
        );

        AddModel(
            AssetPaths.ContentFolder3D +
            "buildings/suburban/building-type-c",

            AssetPaths.ContentFolderEffects +
            "BasicShader",

            Color.DarkBlue
        );

        // PISO

        AddObject(
            _tileModels[0],
            new Vector3(12f),
            Vector3.Zero,
           rotation + MathHelper.PiOver2
        );

        // RUTA

        AddObject(
            _tileModels[1],
            new Vector3(12f, 12f, 5f),
            new Vector3(0f, 10f, 0f),
           rotation + MathHelper.PiOver2 
        );

        // EDIFICIOS

        // AddObject(
        //     _tileModels[2],
        //     new Vector3(2f),
        //     new Vector3(460f, 10f, 0f),
        //     0f
        // );

        // AddObject(
        //     _tileModels[2],
        //     new Vector3(2f),
        //     new Vector3(-460f, 10f, 0f),
        //     0f
        // );
    }
}