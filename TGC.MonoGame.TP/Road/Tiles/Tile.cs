using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public abstract class Tile
{

    protected ContentManager _content;
    protected readonly List<CustomModel> _tileModels;

    protected readonly List<WorldObject> _tileObjects;

    public Vector3 Position;

    public Vector3 NextTileOffset;

    public string[] tileName ={"Recta1","Recta2"};

    public abstract TileType GetTileType();

    protected Biome biome;

    public Tile GenerateNextOfType(TileType type)
    {
        return this.biome.GenerateNewTileOf(type, this.Position + this.NextTileOffset);
    }

    public Tile(
        ContentManager content,
        Vector3 position
    )
    {
        Position = position;

        _content = content;

        _tileModels = new List<CustomModel>();

        _tileObjects = new List<WorldObject>();

        biome = new AsphaltBiome(content, null);
    }

    public void AddModel(
        string modelPath,
        string effectPath,
        Color color
    )
    {
        //FIXME: Long parameter list
        _tileModels.Add(
            new CustomModel(
                _content.Load<Model>(modelPath),
                _content.Load<Effect>(effectPath),
                color
            )
        );
    }

    public void AddObject(
        CustomModel model,
        Vector3 scale,
        Vector3 offset,
        float rotationY
    )
    {
        Matrix world =
            Matrix.CreateScale(scale) *
            Matrix.CreateRotationY(rotationY) *
            Matrix.CreateTranslation(Position + offset);

        _tileObjects.Add(
            new WorldObject(model, world)
        );
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in _tileObjects)
        {
            obj.Update(gameTime);
        }
    }

    public void Draw(
        GameTime gameTime,
        Camera camera
    )
    {
        foreach (var obj in _tileObjects)
        {
            obj.DrawOn(
                gameTime,
                camera,
                camera.GetProjection()
            );
        }
    }
}