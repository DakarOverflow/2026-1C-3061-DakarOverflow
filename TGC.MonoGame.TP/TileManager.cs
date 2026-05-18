using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP.Zero;

public class TileManager
{
    private readonly ContentManager _content;

    private readonly GraphicsDevice _graphicsDevice;

    private readonly string _content3DPath;

    private readonly string _contentEffectsPath;

    private readonly List<Tile> _tiles;

    private const int TileCount = 100;

    public TileManager(
        ContentManager content,
        GraphicsDevice graphicsDevice,
        string content3DPath,
        string contentEffectsPath
    )
    {
        _content = content;

        _graphicsDevice = graphicsDevice;

        _content3DPath = content3DPath;

        _contentEffectsPath = contentEffectsPath;

        _tiles = new List<Tile>();
    }

    public void Load()
    {
        GenerateRoad();
    }

    private void GenerateRoad()
    {
        Vector3 currentPosition =
            new Vector3(0f, -50f, 0f);

        Random random = new Random();

        for (int i = 0; i < TileCount; i++)
        {
            Tile tile = new Tile(
                _content,
                _content3DPath,
                _contentEffectsPath,
                currentPosition
            );

            // después acá irá el random real

            tile.BuildRecta1();

            _tiles.Add(tile);

            currentPosition +=
                tile.NextTileOffset;
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var tile in _tiles)
        {
            tile.Update(gameTime);
        }
    }

    public void Draw(
        GameTime gameTime,
        Camera camera
    )
    {
        foreach (var tile in _tiles)
        {
            tile.Draw(gameTime, camera);
        }
    }
}