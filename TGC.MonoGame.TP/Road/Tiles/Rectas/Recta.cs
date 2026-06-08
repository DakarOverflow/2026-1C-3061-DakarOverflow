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
        float rotation
    ) : base(position, rotation) {}
    
    public override TileType GetTileType()
    {
        return TileType.STRAIGHT_LINE;
    }
}