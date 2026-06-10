using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class Recta : Tile
{
    public Recta(
        Vector3 position,
        float rotation, 
        Biome biome
    ) : base(position, rotation, biome) {}
    
    public override TileType GetTileType()
    {
        return TileType.STRAIGHT_LINE;
    }

    public override float GetRotationOffsetForNextTile()
    {
        return 0f;
    }

    public override List<Vector3> GetObstacleSpawnPoints()
    {
        return
        [
            new Vector3(0f,0f,-300f),
            new Vector3(100f,0f,0f),
            new Vector3(-100f,0f,300f)
        ];
    }
}