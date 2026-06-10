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

    public float GetCollectionableSpawnChance(CollectibleType type)
    {
        //FIXME: Por ahora defino las probabilidades generales pero deberíamos redefinirlas según el tipo de coleccionable 
        return this._gameMode.Difficulty switch
        {
            GameDifficulty.EASY => 0.5f,
            GameDifficulty.MEDIUM => 0.3f,
            GameDifficulty.HARD => 0.1f,
            _ => throw new Exception("Invalid Game Difficulty")
        };
    }

    public bool ShouldSpawnCollectibleOfType(CollectibleType type)
    {
        return this._randomGenerator.NextDouble() < this.GetCollectionableSpawnChance(type);
    }
}
