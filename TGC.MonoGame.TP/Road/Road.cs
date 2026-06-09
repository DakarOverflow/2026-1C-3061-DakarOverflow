using System;
using System.Collections.Generic;
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

    private Random _randomGenerator;

    public Road(Tile firstTile)
    {
        this._tiles = new Queue<Tile>([firstTile]);  
        this._randomGenerator = new Random();      
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
        if(Vector3.DistanceSquared(car.Position, this._tiles.Last<Tile>().Position) < SQUARED_GENERATION_DISTANCE)
        {
            this._tiles.Enqueue(GetLastlyGeneratedTyle().GenerateNextOfType(this.GetNextTileType()));
        }   
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