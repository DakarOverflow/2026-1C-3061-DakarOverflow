using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public interface Biome
{
    Biome GetNextBiome(GameMode gameMode);
    CustomModel getLeftCurveModel();
    CustomModel getRightCurveModel();
    CustomModel getStraightModel();
}