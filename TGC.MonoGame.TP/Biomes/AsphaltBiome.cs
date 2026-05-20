using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class AsphaltBiome : Biome
{
    private Random _randomGenerator;

    private const float PROBABILIDAD_PASE_A_TIERRA = 0.1f;
    private const float PROBABILIDAD_PASE_A_NIEVE = 0.15f;

    public AsphaltBiome(ContentManager content, Random randomGenerator) : base(content)
    {
        _randomGenerator = randomGenerator;
        if(_randomGenerator == null)
        {
            _randomGenerator = new Random();   
        }
    }
    public override Biome GetNextBiome(GameMode gameMode)
    {
        if(gameMode.BiomeType == BiomeType.RANDOM)
        {
            float rand = this._randomGenerator.Next();
            if(rand < PROBABILIDAD_PASE_A_TIERRA)
            {
                return new DirtBiome(_content, _randomGenerator);
            }else if(rand < PROBABILIDAD_PASE_A_TIERRA + PROBABILIDAD_PASE_A_NIEVE)
            {
                return new SnowBiome(_content, _randomGenerator);
            }
        }
        return this; // Si el bioma es constante o no se alcanza la probabilidad requerida para el cambio, continuamos en el bioma actual
    }

    public override Tile GenerateNewTileOf(TileType type, Vector3 position)
    {
        //FIXME: Implementar los diferentes tiles por tipo y bioma

        if(type == TileType.STRAIGHT_LINE)
        {
            return new TileRecta(_content, position);
        }
        else
        {
            return new TileCurvaDerecha(_content, position);
        }
    }
}   