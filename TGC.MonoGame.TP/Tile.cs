using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
namespace TGC.MonoGame.TP;


enum TypeTile{
    Recta1,      
};

public class Tile{
    public List<CustomModel> _TileModel = new List<CustomModel>();
    public List<WorldObject> _TileObjs = new List<WorldObject>();
    public Vector3 _TileParentCoord;
    public string Content3DPath;
    public string ContentEffectsPath; 
    // ContentManager _content
    private ContentManager Content;
    public Tile[] _allTiles;

    public Vector3 _nextTile;
    public Tile(string Path3d,string EffectsPath,Vector3 Coord,ContentManager content){
        this.Content3DPath = Path3d;
        this.ContentEffectsPath = EffectsPath;
        this._TileParentCoord = Coord;
        this.Content = content;
    }

    public void UpdateCoord(Vector3 Coord){
        this._TileParentCoord = Coord;
    }

    //Para agregar los CustomModel a los elem de la tile
    public void AddObjtsToTile(string ContentFolder3DRoot,string ContentFolderEffectsRoot,Color color ){
        _TileModel.Add(new CustomModel(
            Content.Load<Model>(ContentFolder3DRoot),
            Content.Load<Effect>(ContentFolderEffectsRoot),color)
        );
    }
        //Para agregar Todos los elementos a la tile y que se vean al mundo
    public void AddObjtsToWorldTile(CustomModel Model,Vector3 Scale,Vector3 Coord,float RotationY){
        _TileObjs.Add( new WorldObject(
            Model,
            Matrix.CreateScale(Scale) * Matrix.CreateRotationY(RotationY) * Matrix.CreateTranslation(Coord),
            Vector3.Zero,
            Vector3.Zero
        )
        );
    }
    public Tile SetUpTileRecto1()    {
        this._nextTile = new Vector3(0f,0f,1200f);

        AddObjtsToTile(Content3DPath + "road-tiles/road-square", ContentEffectsPath + "BasicShader",
            Color.DarkGreen);
        AddObjtsToTile(Content3DPath + "road-tiles/road-straight", ContentEffectsPath + "BasicShader",
            Color.Gray);
        AddObjtsToTile(Content3DPath + "buildings/suburban/building-type-c",
            ContentEffectsPath + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(Content3DPath + "buildings/suburban/building-type-k",
            ContentEffectsPath + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(Content3DPath + "buildings/suburban/building-type-f",
            ContentEffectsPath + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(Content3DPath + "buildings/suburban/building-type-k",
            ContentEffectsPath + "BasicShader", Color.DarkBlue);

        //Piso y Autopista 
        AddObjtsToWorldTile(_TileModel[0], new Vector3(12f),_TileParentCoord, 0);
        AddObjtsToWorldTile( _TileModel[1], new Vector3(12f, 12f, 5f),
        _TileParentCoord + new Vector3(0f, 10f, 0f), MathHelper.Pi / 2f);
        //Edificios 
        AddObjtsToWorldTile( _TileModel[2], new Vector3(2f),
            _TileParentCoord + new Vector3(460f, 10f, 0f), 0);
        AddObjtsToWorldTile( _TileModel[2], new Vector3(2f),
            _TileParentCoord + new Vector3(-460f, 10f, 0f), 0);
        AddObjtsToWorldTile( _TileModel[3], new Vector3(2f),
            _TileParentCoord + new Vector3(460f, 10f, 500f ), 0);
        AddObjtsToWorldTile( _TileModel[3], new Vector3(2f),
            _TileParentCoord + new Vector3(-460f, 10f, 500f ), 0);
        AddObjtsToWorldTile( _TileModel[4], new Vector3(2f),
            _TileParentCoord + new Vector3(460f, 10f, 200f ), 0);
        AddObjtsToWorldTile( _TileModel[4], new Vector3(2f),
            _TileParentCoord + new Vector3(-460f, 10f, 200f ), 0);
        AddObjtsToWorldTile( _TileModel[5], new Vector3(2f),
            _TileParentCoord + new Vector3(460f, 10f, -400f ), 0);
        AddObjtsToWorldTile( _TileModel[5], new Vector3(2f),
                _TileParentCoord + new Vector3(-460f, 10f, -400f ), 0);
        return this;
    }
}
