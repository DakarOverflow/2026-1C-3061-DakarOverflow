using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;
using System.Collections;
using System.Collections.Generic;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";

    private readonly GraphicsDeviceManager _graphics;

    CustomModel _modeloBase;
    WorldObject _objetoBase;

    CustomModel _modeloRoadStraight;
    WorldObject _objetoRoadStraight;

        List<CustomModel> _singleTile = new List<CustomModel>();
        List<WorldObject> _singleTileObjs = new List<WorldObject>();

        Vector3 _singleTileParentCoord =  new Vector3(0f, -50f, 0f);

    CustomModel _modeloAuto;
    WorldObject _objetoAutoJugador;

    FollowCamera _currentCamera;

    private SpriteBatch _spriteBatch;

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

        // Apago el backface culling.
        // Esto se hace por un problema en el diseno del modelo del logo de la materia.
        // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
        var rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        // Seria hasta aca.

        _currentCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
        // Configuramos nuestras matrices de la escena.

        base.Initialize();
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void LoadContent()
    {
   

        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Cargo el modelo del logo.
        AddObjtsToTile(_singleTile,ContentFolder3D + "road-tiles/road-square",ContentFolderEffects + "BasicShader",Color.DarkGreen);
        AddObjtsToTile(_singleTile,ContentFolder3D + "road-tiles/road-straight",ContentFolderEffects + "BasicShader", Color.Gray);
        

        _modeloAuto = new CustomModel(
            Content.Load<Model>(ContentFolder3D + "car-kit/sedan-sports"),
            Content.Load<Effect>(ContentFolderEffects + "BasicShader"),
            Color.DarkRed
        );

        AddObjtsToWorldTile(_singleTileObjs,_singleTile[0],new Vector3(12f),_singleTileParentCoord,0);
        AddObjtsToWorldTile(_singleTileObjs,_singleTile[1],new Vector3(10f, 5f, 5f),_singleTileParentCoord + new Vector3(0f,10f,0f),
        MathHelper.Pi / 2f);

        _objetoAutoJugador = new WorldObject(
            _modeloAuto,
            Matrix.Identity,
            Vector3.Zero,
            Vector3.Zero
        );

   
        base.LoadContent();
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

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        // Aca deberiamos poner toda la loBace de actualizacion 
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)){
            //Salgo del juego.
            Exit();
        }

          for (var i = 0; i < _singleTileObjs.Count; i++){
            _singleTileObjs[i].Update(gameTime);
        }

        // _objetoBase.Update(gameTime);
        // _objetoRoadStraight.Update(gameTime);
        _objetoAutoJugador.Update(gameTime);

        _currentCamera.Update(gameTime, _objetoAutoJugador.GetCurrentWorld(gameTime));

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logia de renderizado del juego.
        
        GraphicsDevice.Clear(Color.CornflowerBlue);
            
            
        for (var i = 0; i < _singleTileObjs.Count; i++){
            _singleTileObjs[i].DrawOn(gameTime, _currentCamera);
        }
        

        // _objetoBase.DrawOn(gameTime, _currentCamera);
        // _objetoRoadStraight.DrawOn(gameTime, _currentCamera);
        _objetoAutoJugador.DrawOn(gameTime, _currentCamera);
    }

    /// <summary>
    ///     Libero los recursos que se cargaron en el juego.
    /// </summary>
    protected override void UnloadContent()
    {
        // Libero los recursos.
        Content.Unload();

        base.UnloadContent();
    }
}