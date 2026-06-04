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
        // float rightCurveChance = 0.2f;
        // float leftCurveChance = 0.2f;


        List<Tile> lastTwo = this._tiles.TakeLast(2).ToList<Tile>();

        TileType newTileNum;
        Random rnd = new Random();

       do{
        //2 porque los 2 primeros no van
        newTileNum = (TileType) rnd.Next(2, System.Enum.GetValues(typeof(TileType)).Length); 
    
       } while (TypeTileCheack(lastTwo,newTileNum));
        
        return newTileNum;

    } 

    private bool TypeTileCheack(List<Tile> lastTwo,TileType tileActual){
        if(lastTwo.Count < 2) return false;
    
        // if(lastTwo[0].GetTileType() == lastTwo[1].GetTileType()) return true;
        if(lastTwo[1].GetTileTypeMeta() == TileType.CURVE && tileActual== TileType.RIGHT_CURVE) return true;
        return false;
    }

    private Tile GetLastlyGeneratedTyle()
    {
        return this._tiles.Last<Tile>();
    }
}