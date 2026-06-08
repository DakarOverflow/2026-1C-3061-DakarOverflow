using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class Biome
{
    protected Random _randomGenerator;
    public abstract Biome GetNextBiome(GameMode gameMode);
    public abstract Tile GenerateNewTileOf(TileType type, Vector3 position,float Rotation);
}