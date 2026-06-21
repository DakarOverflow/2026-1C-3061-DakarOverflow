using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class DirtyAsphaltBiome : Biome
{
    private const float PROBABILIDAD_PASE_A_TIERRA = 0.2f;
    private const float PROBABILIDAD_PASE_A_ASFALTO = 0.2f;

    public DirtyAsphaltBiome(Random randomGenerator, GameMode gameMode) : base(randomGenerator, gameMode) {}

    public override Biome GetNextBiome()
    {
        if(_gameMode.BiomeType == BiomeType.RANDOM)
        {
            float rand = (float) this._randomGenerator.NextDouble();
            if(rand < PROBABILIDAD_PASE_A_TIERRA)
            {
                return new DirtBiome(_randomGenerator, _gameMode);
            }else if(rand < PROBABILIDAD_PASE_A_TIERRA + PROBABILIDAD_PASE_A_ASFALTO)
            {
                return new AsphaltBiome(_randomGenerator, _gameMode);
            }
        }
        return this; // Si el bioma es constante o no se alcanza la probabilidad requerida para el cambio, continuamos en el bioma actual
    }



    protected override Tile GenerateNewTileForCurrentBiomeOf(TileType type, Vector3 position,float rotation)
    {
        if(type == TileType.STRAIGHT_LINE)
        {
            return new RectaTierraAsfalto(position,rotation, this);
        }
        else if(type == TileType.LEFT_CURVE)
        {
            return new CurvaIzquierdaTierraAsfalto(position,rotation, this);
        }
        else if(type == TileType.RIGHT_CURVE)
        {
            return new CurvaDerechaTierraAsfalto(position,rotation, this);
        }
        throw new ArgumentException("Tipo de tile no válido para el bioma de tierra-asfalto");
    }
}   