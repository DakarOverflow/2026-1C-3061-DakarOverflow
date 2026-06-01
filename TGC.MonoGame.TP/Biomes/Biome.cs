using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class Biome
{
    protected ContentManager _content;
    public Biome(ContentManager content)
    {
        _content = content;   
    }

    public abstract Biome GetNextBiome(GameMode gameMode);
    public abstract Tile GenerateNewTileOf(TileType type, Vector3 position,float Rotation);
}