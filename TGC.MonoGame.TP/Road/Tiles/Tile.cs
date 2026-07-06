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

    protected readonly List<Obstacle> _obstacles;

    public IReadOnlyList<Obstacle> Obstacles
    {
        get => _obstacles;
    }

    public IEnumerable<WorldObject> WorldObjects
    {
        get
        {
            foreach (var obj in _tileObjects)
            {
                yield return obj;
            }

            foreach (var obstacle in _obstacles)
            {
                if (obstacle.IsActive)
                {
                    yield return obstacle;
                }
            }
        }
    }

    public BoundingBox GetBoundingBox()
    {
        BoundingBox? box = null;

        foreach (var obj in WorldObjects)
        {
            BoundingBox objectBox = BoundingVolumeFactory.CreateWorldBoundingBox(obj);
            box = box.HasValue
                ? BoundingBox.CreateMerged(box.Value, objectBox)
                : objectBox;
        }

        return box ?? new BoundingBox(Position, Position);
    }

    public Vector3 Position;

    public Vector3 NextTileOffset;

    public float Rotation;

    public float NextTileRotation;

    public abstract TileType GetTileType();

    public abstract float GetRotationOffsetForNextTile();

    protected Biome biome;

    public Tile GenerateNextOfType(TileType type)
    {
        Vector3 localCorrection = Vector3.Zero;

        if (this.GetTileType() == TileType.STRAIGHT_LINE)
        {
            if (type == TileType.LEFT_CURVE)
            {
                localCorrection = new Vector3(-150f, 0f, 0f);
            }
            else if (type == TileType.RIGHT_CURVE)
            {
                localCorrection = new Vector3(150f, 0f, 0f);
            }
        }
        else if (this.GetTileType() == TileType.LEFT_CURVE)
        {
            if (type == TileType.STRAIGHT_LINE)
            {
                localCorrection = new Vector3(0f, 0f, -150f);
            }
            else if (type == TileType.RIGHT_CURVE)
            {
                localCorrection = new Vector3(0f, 0f, -300f);
            }
        }
        else if (this.GetTileType() == TileType.RIGHT_CURVE)
        {
            if (type == TileType.STRAIGHT_LINE)
            {
                localCorrection = new Vector3(0f, 0f, -150f);
            }
            else if (type == TileType.LEFT_CURVE)
            {
                localCorrection = new Vector3(0f, 0f, -300f);
            }
        }

        float currentTileDirection = this.NextTileRotation - this.GetRotationOffsetForNextTile();

        Vector3 worldCorrection = Vector3.Transform(
            localCorrection,
            Matrix.CreateRotationY(currentTileDirection)
        );

        Vector3 correctedNextTileOffset = this.NextTileOffset + worldCorrection;

        return this.biome.GenerateNewTileOf(
            type,
            this.Position + correctedNextTileOffset,
            this.NextTileRotation
        );
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
        foreach (Type type in concreteTileTypes)
        {
            MethodInfo modelLoader = type.GetMethod("LoadLocalModels");
            modelLoader.Invoke(null, new object[] { content });
        }
    }

    public Tile(
        Vector3 position,
        float rotation,
        Biome biome
    )
    {
        Position = position;
        Rotation = rotation;
        _tileObjects = new List<WorldObject>();
        this.biome = biome;
        NextTileRotation = rotation + this.GetRotationOffsetForNextTile();
        _obstacles = new List<Obstacle>();
    }


    private Vector3 GetWorldOffset(Vector3 localOffset)
    {
        return Vector3.Transform(localOffset, Matrix.CreateRotationY(Rotation));
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
            Matrix.CreateTranslation(GetWorldOffset(offset)) *
            Matrix.CreateTranslation(Position);

        _tileObjects.Add(
            new WorldObject(model, world)
        );
    }


    public void AddObject(
        CustomModel model,
        Vector3 scale,
        Vector3 offset,
        float rotationY,
        float rotationZ
    )
    {
        Matrix world =
            Matrix.CreateScale(scale) *
            Matrix.CreateRotationY(rotationY) *
            Matrix.CreateRotationZ(rotationZ) *
            Matrix.CreateTranslation(GetWorldOffset(offset)) *
            Matrix.CreateTranslation(Position);

        _tileObjects.Add(
            new WorldObject(model, world)
        );
    }

    public void AddObstacle(Obstacle obstacle)
    {
        _obstacles.Add(obstacle);
    }

    public void AddObstacle(
        CustomModel model,
        Vector3 scale,
        Vector3 offset,
        float rotationY,
        Vector3 hitboxSize,
        Vector3 hitboxOffset,
        float damage,
        float speedMultiplier,
        bool isFatalOnFrontalCollision = false
    )
    {
        Matrix world =
            Matrix.CreateScale(scale) *
            Matrix.CreateRotationY(rotationY) *
            Matrix.CreateTranslation(GetWorldOffset(offset)) *
            Matrix.CreateTranslation(Position);

        Vector3 finalPos = world.Translation;

        Obstacle obstacle = new Obstacle(
            model,
            world,
            finalPos,
            hitboxSize,
            hitboxOffset,
            damage,
            speedMultiplier,
            isFatalOnFrontalCollision
        );

        AddObstacle(obstacle);
    }

    public virtual List<Vector3> GetObstacleSpawnPoints()
    {
        return null;
    }

    public void AddObject(WorldObject obj)
    {
        _tileObjects.Add(obj);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var obj in _tileObjects)
        {
            obj.Update(gameTime);
        }

        foreach (var obstacle in _obstacles)
        {
            obstacle.Update(gameTime);
        }
    }

    public float GetFrictionCoefficient()
    {
        return this.biome.GetFrictionCoefficient();
    }

    public void CheckCollisions(Vehicle player)
    {
        foreach (var obj in _tileObjects)
        {
            if (obj is Collectible collectible && collectible.IsActive)
            {
                if (player.OBB.Intersects(collectible.BoundingBox))
                {
                    collectible.PickUp(player);
                }
            }
        }
    }

    public IEnumerable<BoundingBox> GetActiveCollectibleHitboxes()
    {
        foreach (var obj in _tileObjects)
        {
            if (obj is Collectible collectible && collectible.IsActive)
            {
                yield return collectible.BoundingBox;
            }
        }
    }

    public void SetShadowMap(Texture2D shadowMap, Matrix lightViewProjection)
    {
        foreach (var obj in _tileObjects)
        {
            obj.SetShadowMap(shadowMap, lightViewProjection);
        }

        foreach (var obstacle in _obstacles)
        {
            if (obstacle.IsActive)
            {
                obstacle.SetShadowMap(shadowMap, lightViewProjection);
            }
        }
    }

    public void DrawDepth(Matrix lightViewProjection)
    {
        foreach (var obj in _tileObjects)
        {
            obj.DrawDepth(lightViewProjection);
        }

        foreach (var obstacle in _obstacles)
        {
            if (obstacle.IsActive)
            {
                obstacle.DrawDepth(lightViewProjection);
            }
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

        foreach (var obstacle in _obstacles)
        {
            obstacle.Draw(
                gameTime,
                camera.GetView(),
                camera.GetProjection()
            );
        }
    }
}
