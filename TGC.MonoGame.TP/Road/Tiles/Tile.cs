using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Linq;

namespace TGC.MonoGame.TP;

public abstract class Tile
{
    protected readonly List<WorldObject> _tileObjects;

    public Vector3 Position;

    public Vector3 NextTileOffset;

    public float NextTileRotation;

    public abstract TileType GetTileType();

    protected Biome biome;

    public Tile GenerateNextOfType(TileType type)
    {
        return this.biome.GenerateNewTileOf(type, this.Position + this.NextTileOffset,this.NextTileRotation);
    }

    public static void LoadModels(ContentManager content)
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        
        var concreteTileTypes = currentAssembly.GetTypes()
            .Where(t => 
                t.IsSubclassOf(typeof(Tile))
                && !t.IsAbstract               
                && t.IsClass
                && typeof(IAssetLoader).IsAssignableFrom(t)
            );

        //No, esto no lo hizo una IA! Parece mágia pero es simple. Uso reflection para buscar todas las subclases instanciables de Tile, que por ser iomplementar la interfac IAssetLoader tienen que implementar el método estatico LoadLocalModels utilizado para cargar una única vez los modelos a memoria y optimizar el espacio 
        foreach(Type type in concreteTileTypes)
        {
            MethodInfo modelLoader = type.GetMethod("LoadLocalModels");
            modelLoader.Invoke(null, new object[] {content});
        }
    }

    public Tile(
        Vector3 position,
        float rotation
    )
    {
        Position = position;
        _tileObjects = new List<WorldObject>();
        biome = new AsphaltBiome(null);
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