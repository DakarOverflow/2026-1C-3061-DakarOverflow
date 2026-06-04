using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class DirtBiome : Biome
{
    private Random _randomGenerator;

    private const float PROBABILIDAD_PASE_A_NIEVE = 0.1f;
    private const float PROBABILIDAD_PASE_A_ASFALTO = 0.15f;

    public DirtBiome(ContentManager content, Random randomGenerator) : base(content)
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
            float rand = (float) this._randomGenerator.NextDouble();
            if(rand < PROBABILIDAD_PASE_A_NIEVE)
            {
                return new SnowBiome(_content, _randomGenerator);
            }else if(rand < PROBABILIDAD_PASE_A_NIEVE + PROBABILIDAD_PASE_A_ASFALTO)
            {
                return new AsphaltBiome(_content, _randomGenerator);
            }
        }
        return this; // Si el bioma es constante o no se alcanza la probabilidad requerida para el cambio, continuamos en el bioma actual
    }

}