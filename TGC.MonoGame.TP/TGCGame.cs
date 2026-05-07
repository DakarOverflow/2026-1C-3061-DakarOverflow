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

    // CustomModel _modeloBase;
    // WorldObject _objetoBase;

    // CustomModel _modeloRoadStraight;
    // WorldObject _objetoRoadStraight;
    List<CustomModel> _singleTile = new List<CustomModel>();
    List<WorldObject> _singleTileObjs = new List<WorldObject>();
    Vector3 _singleTileParentCoord = new Vector3(0f, -50f, 0f);
    public Tile[] _allTiles;
    public Tile _recto;
    CustomModel _modeloAuto;
    WorldObject _objetoAutoJugador;
    private FollowCamera _followCamera;
    private FreeCamera _freeCamera;
    private SpriteBatch _spriteBatch;
    private Matrix _world;
    private KeyboardState _previousKeyboardState;
    private bool _mouseCaptured = true;
    private bool _useFreeCamera;
    private Camera _cameraInUse;

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);
        var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        // 1/4 of the screen area
        _graphics.PreferredBackBufferWidth = screenWidth / 2;
        _graphics.PreferredBackBufferHeight = screenHeight / 2;
        _graphics.IsFullScreen = false;
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        Window.AllowUserResizing = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
        IsMouseVisible = false;
        _graphics.ApplyChanges();
        var rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        GraphicsDevice.RasterizerState = rasterizerState;
        _freeCamera = new FreeCamera(new Vector3(110f, 10f, 110f), new Vector3(0f, 0f, 0f), GraphicsDevice);
        Window.ClientSizeChanged += _freeCamera.OnClientSizeChanged;
        _followCamera = new FollowCamera(GraphicsDevice);
        Window.ClientSizeChanged += _followCamera.OnClientSizeChanged;
        _cameraInUse = _followCamera;
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
        AddObjtsToTile(_singleTile, ContentFolder3D + "road-tiles/road-square", ContentFolderEffects + "BasicShader",
            Color.DarkGreen);
        AddObjtsToTile(_singleTile, ContentFolder3D + "road-tiles/road-straight", ContentFolderEffects + "BasicShader",
            Color.Gray);
        AddObjtsToTile(_singleTile, ContentFolder3D + "buildings/suburban/building-type-c",
            ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(_singleTile, ContentFolder3D + "buildings/suburban/building-type-k",
            ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(_singleTile, ContentFolder3D + "buildings/suburban/building-type-f",
            ContentFolderEffects + "BasicShader", Color.DarkBlue);
        AddObjtsToTile(_singleTile, ContentFolder3D + "buildings/suburban/building-type-k",
            ContentFolderEffects + "BasicShader", Color.DarkBlue);
        _modeloAuto = new CustomModel(Content.Load<Model>(ContentFolder3D + "car-kit/sedan-sports"),
            Content.Load<Effect>(ContentFolderEffects + "BasicShader"), Color.DarkRed);
        float gap = 1200f;
        
        for (int i = 0; i < 100; i++)
        {
            //Modularizarlo aparte y que cada tile por separado tenga su propia cordenada relativa a esa
            //Piso y Autopista 
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[0], new Vector3(12f),
                _singleTileParentCoord - new Vector3(0, 0f, i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[1], new Vector3(12f, 12f, 5f),
                _singleTileParentCoord + new Vector3(0f, 10f, 0f - i * gap), MathHelper.Pi / 2f);
            //Edificios 
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[2], new Vector3(2f),
                _singleTileParentCoord + new Vector3(460f, 10f, 0f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[2], new Vector3(2f),
                _singleTileParentCoord + new Vector3(-460f, 10f, 0f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[3], new Vector3(2f),
                _singleTileParentCoord + new Vector3(460f, 10f, 500f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[3], new Vector3(2f),
                _singleTileParentCoord + new Vector3(-460f, 10f, 500f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[4], new Vector3(2f),
                _singleTileParentCoord + new Vector3(460f, 10f, 200f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[4], new Vector3(2f),
                _singleTileParentCoord + new Vector3(-460f, 10f, 200f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[5], new Vector3(2f),
                _singleTileParentCoord + new Vector3(460f, 10f, -400f - i * gap), 0);
            AddObjtsToWorldTile(_singleTileObjs, _singleTile[5], new Vector3(2f),
                _singleTileParentCoord + new Vector3(-460f, 10f, -400f - i * gap), 0);
        }
        _objetoAutoJugador = new WorldObject(_modeloAuto, Matrix.Identity, Vector3.Zero, Vector3.Zero);
        base.LoadContent();
    }

    //Para agregar los CustomModel a los elem de la tile
    public void AddObjtsToTile(List<CustomModel> Tile, string ContentFolder3DRoot, string ContentFolderEffectsRoot, Color color)
    {
        Tile.Add(new CustomModel(Content.Load<Model>(ContentFolder3DRoot),
            Content.Load<Effect>(ContentFolderEffectsRoot), color));
    }

    //Para agregar Todos los elementos a la tile y que se vean al mundo
    public void AddObjtsToWorldTile(List<WorldObject> Tile, CustomModel Model, Vector3 Scale, Vector3 Coord, float RotationY)
    {
        Tile.Add(new WorldObject(Model,
            Matrix.CreateScale(Scale) * Matrix.CreateRotationY(RotationY) * Matrix.CreateTranslation(Coord),
            Vector3.Zero, Vector3.Zero));
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        // Aca deberiamos poner toda la loBace de actualizacion 
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        if (keyboardState.IsKeyDown(Keys.M) && _previousKeyboardState.IsKeyUp(Keys.M))
        {
            _mouseCaptured = !_mouseCaptured;
            IsMouseVisible = !_mouseCaptured;
        }

        for (var i = 0; i < _singleTileObjs.Count; i++)
        {
            _singleTileObjs[i].Update(gameTime);
        }

        if (keyboardState.IsKeyDown(Keys.F) && _previousKeyboardState.IsKeyUp(Keys.F))
        {
            _useFreeCamera = !_useFreeCamera;
        }
        if (_useFreeCamera)
        {
            _cameraInUse = _freeCamera;
            _freeCamera.Update(gameTime, _mouseCaptured);
        }
        else
        {
            _cameraInUse = _followCamera;
            _followCamera.Update(gameTime, _objetoAutoJugador.GetCurrentWorld(gameTime));
        }
        // _objetoBase.Update(gameTime);
        // _objetoRoadStraight.Update(gameTime);
        //_objetoAutoJugador.Update(gameTime);
        
        _previousKeyboardState = keyboardState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logia de renderizado del juego.
        GraphicsDevice.Clear(Color.CornflowerBlue);
        for (var i = 0; i < _singleTileObjs.Count; i++)
        {
            _singleTileObjs[i].DrawOn(gameTime, _cameraInUse, _cameraInUse.GetProjection());
        }

        // _objetoBase.DrawOn(gameTime, _currentCamera);
        // _objetoRoadStraight.DrawOn(gameTime, _currentCamera);
        _objetoAutoJugador.DrawOn(gameTime, _cameraInUse, _cameraInUse.GetProjection());
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