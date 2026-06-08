using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public interface IAssetLoader
{
    static abstract void LoadLocalModels(ContentManager content);
}