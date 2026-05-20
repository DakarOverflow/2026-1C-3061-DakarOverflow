using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
namespace TGC.MonoGame.TP;


public enum TileType
{
    Recta1,
    Recta2,
    CurvaDer1
}

public class Tile
{
    private readonly ContentManager _content;

    private readonly string _content3DPath;

    private readonly string _contentEffectsPath;

    private readonly List<CustomModel> _tileModels;

    private readonly List<WorldObject> _tileObjects;

    public Vector3 Position;

    public Vector3 NextTileOffset;

    public string[] tileName ={"Recta2","CurvaDer1"};//"Recta1","Recta2",


    public Tile(
        ContentManager content,
        string content3DPath,
        string contentEffectsPath,
        Vector3 position
    )
    {
        _content = content;

        _content3DPath = content3DPath;

        _contentEffectsPath = contentEffectsPath;

        Position = position;

        _tileModels = new List<CustomModel>();

        _tileObjects = new List<WorldObject>();
    }

    public void AddModel(
        string modelPath,
        string effectPath,
        Color color
    )
    {
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

#region CAMINOS
    public void BuildRecta1()
    {
        NextTileOffset =
            new Vector3(0f, 0f, -1200f);

        AddModel(
            _content3DPath +
            "road-tiles/road-square",

            _contentEffectsPath +
            "BasicShader",

            Color.DarkGreen
        );

        AddModel(
            _content3DPath +
            "road-tiles/road-straight",

            _contentEffectsPath +
            "BasicShader",

            Color.Gray
        );

        AddModel(
            _content3DPath +
            "buildings/suburban/building-type-c",

            _contentEffectsPath +
            "BasicShader",

            Color.DarkBlue
        );

        // PISO

        AddObject(
            _tileModels[0],
            new Vector3(12f,1f,12f),
            Vector3.Zero,
            0f
        );

        // RUTA

        AddObject(
            _tileModels[1],
            new Vector3(12f, 12f, 5f),
            new Vector3(0f, 10f, 0f),
            MathHelper.PiOver2
        );

        // EDIFICIOS

        AddObject(
            _tileModels[2],
            new Vector3(2f),
            new Vector3(460f, 10f, 0f),
            0f
        );

        AddObject(
            _tileModels[2],
            new Vector3(2f),
            new Vector3(-460f, 10f, 0f),
            0f
        );
    }
    public void BuildRecta2()
    {
         NextTileOffset =
            new Vector3(0f, 0f, -1200f);
        
        AddModel(_content3DPath + "road-tiles/road-square", _contentEffectsPath + "BasicShader",
            Color.DarkGreen);
        AddModel(_content3DPath + "road-tiles/road-straight", _contentEffectsPath + "BasicShader",
            Color.Gray);
        AddModel(_content3DPath + "buildings/suburban/building-type-c",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-k",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-f",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-k",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);

        //Piso y Autopista 
        AddObject(_tileModels[0], 
            new Vector3(16f,1f,16f),
            Vector3.Zero,
            0f
        );

        AddObject(
             _tileModels[1], 
             new Vector3(12f, 12f, 5f),
            new Vector3(0f, 10f, 0f),
            MathHelper.Pi / 2f
        );
        //Edificios 
        AddObject( 
            _tileModels[2], 
            new Vector3(2f),
            new Vector3(460f, 10f, 0f), 0
        );
        AddObject(
            _tileModels[2], 
            new Vector3(2f),
            new Vector3(-460f, 10f, 0f), 0 
        );

        AddObject( 
            _tileModels[3], 
            new Vector3(2f), 
            new Vector3(460f, 10f, 500f ), 
            0
        );
        AddObject(
             _tileModels[3], 
             new Vector3(2f),
             new Vector3(-460f, 10f, 500f ), 
             0
        );

        AddObject(
             _tileModels[4], 
             new Vector3(2f),
             new Vector3(460f, 10f, 200f ),
              0
        );
        AddObject( _tileModels[4],
            new Vector3(2f),
            new Vector3(-460f, 10f, 200f ),
            0
        );
        AddObject( 
            _tileModels[5], 
            new Vector3(2f),
             new Vector3(460f, 10f, -400f ), 
             0
        );
        AddObject( _tileModels[5],
            new Vector3(2f),
            new Vector3(-460f, 10f, -400f ),
            0
        );

    }

   public void BuildCurvaDer1()
    {
         NextTileOffset =
            new Vector3(1200f, 0, -1200f); //            new Vector3(1200f, 0f, -500f);
        
        AddModel(_content3DPath + "road-tiles/road-square", _contentEffectsPath + "BasicShader",
            Color.DarkGreen);
        AddModel(_content3DPath + "road-tiles/road-curve", _contentEffectsPath + "BasicShader",
            Color.Gray);

        AddModel(_content3DPath + "buildings/suburban/building-type-s",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-u",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-d",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);
        AddModel(_content3DPath + "buildings/suburban/building-type-a",
            _contentEffectsPath + "BasicShader", Color.DarkBlue);

        //Piso y Autopista 
        AddObject(_tileModels[0], 
            new Vector3(12f),
            Vector3.Zero,
            0f
        );

        AddObject(
            _tileModels[1], 
            new Vector3(5f),
            new Vector3(380f, 25f, 0f),
            MathHelper.PiOver2
        );
        //Edificios 
        AddObject( 
            _tileModels[2], 
            new Vector3(2f),
            new Vector3(322f, 10f, 450f), MathHelper.Pi
        );
        // AddObject(
        //     _tileModels[2], 
        //     new Vector3(2f),
        //     new Vector3(-460f, 10f, 0f), 0 
        // );

        // AddObject( 
        //     _tileModels[3], 
        //     new Vector3(2f), 
        //     new Vector3(460f, 10f, 500f ), 
        //     0
        // );
        // AddObject(
        //      _tileModels[3], 
        //      new Vector3(2f),
        //      new Vector3(-460f, 10f, 500f ), 
        //      0
        // );

        // AddObject(
        //      _tileModels[4], 
        //      new Vector3(2f),
        //      new Vector3(460f, 10f, 200f ),
        //       0
        // );
        // AddObject( _tileModels[4],
        //     new Vector3(2f),
        //     new Vector3(-460f, 10f, 200f ),
        //     0
        // );
        // AddObject( 
        //     _tileModels[5], 
        //     new Vector3(2f),
        //      new Vector3(460f, 10f, -400f ), 
        //      0
        // );
        // AddObject( _tileModels[5],
        //     new Vector3(2f),
        //     new Vector3(-460f, 10f, -400f ),
        //     0
        // );

    }


    #endregion CAMINOS


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