using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using BepuPhysics.CollisionDetection.CollisionTasks;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class Road
{
    private const float SQUARED_GENERATION_DISTANCE = 4000000f; //Generation distance: 2000

    private int _generalDirection = 0;
    private Queue<Tile> _tiles;
    public IEnumerable<Tile> Tiles{get => _tiles;}
    
    private readonly  Random _randomGenerator;

    private readonly List<CustomModel> _obstacleModels;

    public Road(Tile firstTile, List<CustomModel> obstacleModels)
    {
        this._tiles = new Queue<Tile>([firstTile]);  
        this._randomGenerator = new Random();      
        _obstacleModels = obstacleModels;
    }

    public virtual void UpdateFor(Vehicle car, GameTime gameTime)
    {
        foreach(Tile tile in this._tiles)
        {
            tile.Update(gameTime);
        }

        this.ExtendRoadIfCarNearEnd(car);
    }

    public virtual void Draw(
        GameTime gameTime,
        Camera camera
    )
    {
        foreach(Tile tile in this._tiles)
        {
            tile.Draw(gameTime, camera);
        }
    }

    private void ExtendRoadIfCarNearEnd(Vehicle car)
    {
        if (Vector3.DistanceSquared(car.Position, GetLastlyGeneratedTyle().Position) < SQUARED_GENERATION_DISTANCE)
        {
            Tile newTile =
                GetLastlyGeneratedTyle().GenerateNextOfType(
                    GetNextTileType()
                );

            PopulateObstacles(newTile);

            _tiles.Enqueue(newTile);
        }
    }

    private void PopulateObstacles(Tile tile)
    {
        if (_randomGenerator.NextDouble() > 0.3) return;

        var spawnPoints = tile.GetObstacleSpawnPoints();

        if (spawnPoints == null || spawnPoints.Count == 0) return;

        Vector3 localPoint = spawnPoints[_randomGenerator.Next(spawnPoints.Count)];

        Vector3 worldPoint = tile.Position + Vector3.Transform(localPoint, Matrix.CreateRotationY(tile.NextTileRotation));

        CustomModel obstacleModel = GetRandomObstacleModel();

        Matrix world = Matrix.CreateScale(5f) * Matrix.CreateTranslation(worldPoint + new Vector3(0f, 30f, 0f));

        tile.AddObstacle(new Obstacle(obstacleModel, world, worldPoint, new Vector3(90f), new Vector3(0f, 90f, 0f), 20f, 0.5f));
    }

    private CustomModel GetRandomObstacleModel()
    {
        return _obstacleModels[_randomGenerator.Next(_obstacleModels.Count)];
    }

    private TileType GetNextTileType()
    {
        float rightCurveChance = 0.2f;
        float leftCurveChance = 0.2f;


        if(_generalDirection >= 1)
        {
            rightCurveChance = 0f;
        }
        else if(_generalDirection <= -1)
        {
            leftCurveChance = 0f;
        }

        float randomNumber = (float) _randomGenerator.NextDouble();

        if(randomNumber < rightCurveChance)
        {
            _generalDirection++;
            return TileType.RIGHT_CURVE;
        } else if(randomNumber < rightCurveChance + leftCurveChance)
        {
            _generalDirection--;
            return TileType.LEFT_CURVE;
        }
        else
        {
            return TileType.STRAIGHT_LINE;
        }
    }

    private Tile GetLastlyGeneratedTyle()
    {
        return this._tiles.Last<Tile>();
    }
}