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
            new Vector3(-180f,15f,345f),
            new Vector3(390f,15f,-15f)
        ];
    }
}