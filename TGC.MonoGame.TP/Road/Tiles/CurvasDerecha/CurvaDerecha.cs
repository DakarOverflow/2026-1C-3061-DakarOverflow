using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class CurvaDerecha : Tile
{
    public CurvaDerecha(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position, rotation, biome) {}
    
    public override TileType GetTileType()
    {
        return TileType.RIGHT_CURVE;
    }

    public override float GetRotationOffsetForNextTile()
    {
        return -MathHelper.PiOver2;
    }

        public override List<Vector3> GetObstacleSpawnPoints()
    {
        return
        [
            new Vector3(-150f,0f,100f),
            new Vector3(-50f,0f,-200f)
        ];
    }
}