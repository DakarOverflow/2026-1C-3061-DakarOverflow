using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;
using System.Collections;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.Zero;


public class Tile : Game {
    List<CustomModel> _singleTile = new List<CustomModel>();
    List<WorldObject> _singleTileObjs = new List<WorldObject>();
    Vector3 _singleTileParentCoord;

    public void SetUpCoord(Vector3 Coord){
        Vector3 _singleTileParentCoord = Coord;
    }


    //Para agregar los CustomModel a los elem de la tile
    public void AddObjtsToTile(List<CustomModel>Tile,string ContentFolder3DRoot,string ContentFolderEffectsRoot,Color color ){
        Tile.Add(new CustomModel(
            Content.Load<Model>(ContentFolder3DRoot),
            Content.Load<Effect>(ContentFolderEffectsRoot),
            color
            )
        );
    }
        //Para agregar Todos los elementos a la tile y que se vean al mundo
    public void AddObjtsToWorldTile(List<WorldObject>Tile,CustomModel Model,Vector3 Scale,Vector3 Coord,float RotationY){
        Tile.Add( new WorldObject(
            Model,
            Matrix.CreateScale(Scale) * Matrix.CreateRotationY(RotationY) * Matrix.CreateTranslation(Coord),
            Vector3.Zero,
            Vector3.Zero
        )
        );
    }
    
}

