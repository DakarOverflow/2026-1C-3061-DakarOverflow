using System;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class AsphaltBiome : Biome
{
    private Random _randomGenerator;

    private const float PROBABILIDAD_PASE_A_TIERRA = 0.1f;
    private const float PROBABILIDAD_PASE_A_NIEVE = 0.15f;

    public AsphaltBiome(Random randomGenerator)
    {
        _randomGenerator = randomGenerator;
        if(_randomGenerator == null)
        {
            _randomGenerator = new Random();   
        }
    }
    public Biome GetNextBiome(GameMode gameMode)
    {
        if(gameMode.BiomeType == BiomeType.RANDOM)
        {
            float rand = this._randomGenerator.Next();
            if(rand < PROBABILIDAD_PASE_A_TIERRA)
            {
                return new DirtBiome(_randomGenerator);
            }else if(rand < PROBABILIDAD_PASE_A_TIERRA + PROBABILIDAD_PASE_A_NIEVE)
            {
                return new SnowBiome(_randomGenerator);
            }
        }
        return this; // Si el bioma es constante o no se alcanza la probabilidad requerida para el cambio, continuamos en el bioma actual
    }

    public CustomModel getLeftCurveModel()
    {
        return null;
    }
    public CustomModel getRightCurveModel()
    {
        return null;
    }
    public CustomModel getStraightModel()
    {
        return null;
    }
}   