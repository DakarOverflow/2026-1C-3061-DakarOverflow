using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class CurvaIzquierda : Tile
{
    public CurvaIzquierda(
        Vector3 position,
        float rotation,
        Biome biome
    ) : base(position, rotation, biome) {}
    
    public override TileType GetTileType()
    {
        return TileType.LEFT_CURVE;
    }

    public override float GetRotationOffsetForNextTile()
    {
        return MathHelper.PiOver2;
    }

        public override List<Vector3> GetObstacleSpawnPoints()
    {
        return
        [
            new Vector3(190f,15f,400f),
            new Vector3(-430f,0f,-225f)
        ];
    }
}