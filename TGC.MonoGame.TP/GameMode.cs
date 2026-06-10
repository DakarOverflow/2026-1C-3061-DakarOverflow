namespace TGC.MonoGame.TP;

public class GameMode
{
    public GameMode(BiomeType biomeType, GameDifficulty difficulty)
    {
        BiomeType = biomeType;
        Difficulty = difficulty;
    }
  public BiomeType BiomeType;
  public GameDifficulty Difficulty;
}