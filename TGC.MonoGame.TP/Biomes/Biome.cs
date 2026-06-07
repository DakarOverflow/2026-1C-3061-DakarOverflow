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
    public Tile GenerateNewTileOf(TileType type, Vector3 position,float rotation){    
        if(type == TileType.STRAIGHT_LINE)
        {
            return new TileRecta(_content, position,rotation);
        }
        if(type == TileType.RIGHT_CURVE){
            return new TileCurvaDerecha(_content, position,rotation);
        }
        if (type == TileType.LEFT_CURVE){
            return new TileCurvaIzquierda(_content, position,rotation);
        }else{
            return new TileRecta2(_content, position,rotation);
        } 
    }
}