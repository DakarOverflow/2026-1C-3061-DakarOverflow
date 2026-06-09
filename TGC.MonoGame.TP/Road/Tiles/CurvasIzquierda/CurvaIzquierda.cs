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
}