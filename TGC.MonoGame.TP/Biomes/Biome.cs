using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class Biome
{
    protected Random _randomGenerator;

    protected GameMode _gameMode;

    public Biome(Random randomGenerator, GameMode gameMode)
    {
        _randomGenerator = randomGenerator;
        if(_randomGenerator == null)
        {
            _randomGenerator = new Random();   
        }
        _gameMode = gameMode;
    }

    public abstract Biome GetNextBiome();

    protected abstract Tile GenerateNewTileForCurrentBiomeOf(TileType type, Vector3 position,float Rotation);
    public Tile GenerateNewTileOf(TileType type, Vector3 position,float Rotation)
    {
        return this.GetNextBiome().GenerateNewTileForCurrentBiomeOf(type, position, Rotation);
    }

    public CollectibleType? GetCollectibleToSpawn()
    {
        // 1. Probabilidad general de que aparezca *algún* coleccionable
        float baseSpawnChance = this._gameMode.Difficulty switch
        {
            GameDifficulty.EASY => 0.5f,
            GameDifficulty.MEDIUM => 0.3f,
            GameDifficulty.HARD => 0.1f,
            _ => throw new Exception("Invalid Game Difficulty")
        };

        if (this._randomGenerator.NextDouble() > baseSpawnChance)
        {
            return null; // No aparece nada en esta recta
        }

        // 2. Si aparece algo, decidimos QUÉ aparece usando probabilidades relativas
        // Ejemplo: 50% Moneda, 30% Combustible, 20% Llave (Wrench)
        double roll = this._randomGenerator.NextDouble();
        if (roll < 0.5) return CollectibleType.Coin;
        if (roll < 0.8) return CollectibleType.FuelTank;
        return CollectibleType.Wrench;
    }
    public abstract float GetFrictionCoefficient();
}
