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

    private readonly List<Tile> _tiles;

    public TileManager(
        ContentManager content,
        GraphicsDevice graphicsDevice
    )
    {
        _content = content;
        _graphicsDevice = graphicsDevice;

        _tiles = new List<Tile>();
    }

    public void Load()
    {
        float gap = 1200f;

        // =========================
        // MODELOS
        // =========================

        var groundModel = new CustomModel(
            _content.Load<Model>("Models/road-tiles/road-square"),
            _content.Load<Effect>("Effects/BasicShader"),
            Color.DarkGreen
        );

        var roadModel = new CustomModel(
            _content.Load<Model>("Models/road-tiles/road-straight"),
            _content.Load<Effect>("Effects/BasicShader"),
            Color.Gray
        );

        var buildingModel = new CustomModel(
            _content.Load<Model>("Models/buildings/suburban/building-type-c"),
            _content.Load<Effect>("Effects/BasicShader"),
            Color.DarkBlue
        );

        // =========================
        // GENERACION
        // =========================

        for (int i = 0; i < 100; i++)
        {
            Tile tile = new Tile();

            Vector3 tileOffset =
                new Vector3(0f, -50f, -i * gap);

            // piso
            tile.AddObject(
                new WorldObject(
                    groundModel,
                    Matrix.CreateScale(12f) *
                    Matrix.CreateTranslation(tileOffset)
                )
            );

            // ruta
            tile.AddObject(
                new WorldObject(
                    roadModel,
                    Matrix.CreateScale(new Vector3(12f, 12f, 5f)) *
                    Matrix.CreateRotationY(MathHelper.PiOver2) *
                    Matrix.CreateTranslation(
                        tileOffset + new Vector3(0f, 10f, 0f)
                    )
                )
            );

            // edificios derecha
            tile.AddObject(
                new WorldObject(
                    buildingModel,
                    Matrix.CreateScale(2f) *
                    Matrix.CreateTranslation(
                        tileOffset + new Vector3(460f, 10f, 0f)
                    )
                )
            );

            // edificios izquierda
            tile.AddObject(
                new WorldObject(
                    buildingModel,
                    Matrix.CreateScale(2f) *
                    Matrix.CreateTranslation(
                        tileOffset + new Vector3(-460f, 10f, 0f)
                    )
                )
            );

            _tiles.Add(tile);
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var tile in _tiles)
        {
            tile.Update(gameTime);
        }
    }

    public void Draw(GameTime gameTime, Camera camera)
    {
        foreach (var tile in _tiles)
        {
            tile.Draw(gameTime, camera);
        }
    }
}