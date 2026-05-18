using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    // List<CustomModel> _singleTile = new List<CustomModel>();
    // List<WorldObject> _singleTileObjs = new List<WorldObject>();
    // Vector3 _singleTileParentCoord = new Vector3(0f, -50f, 0f);
    const int cantTiles = 100; //200
    public Tile[] _allTiles = new Tile[cantTiles];

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
        //Inicializar todos los tipos de tiles
        _allTiles = InitRecorrido(_allTiles);

        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _modeloAuto = new CustomModel(Content.Load<Model>(ContentFolder3D + "car-kit/sedan-sports"),
            Content.Load<Effect>(ContentFolderEffects + "BasicShader"), Color.DarkRed);

        _objetoAutoJugador = new WorldObject(_modeloAuto, Matrix.Identity, Vector3.Zero, Vector3.Zero);
        base.LoadContent();
    }

    public Tile[]  InitRecorrido(Tile[] allTiles){
        string [] Tramos = {"recto1"};
        Vector3 _relativePosc = new Vector3(0f,0f,0f);
        Random rnd = new Random();
        for (int i = 0; i < allTiles.Length; i++){
            if(i > 0){
                _relativePosc =  allTiles[i-1]._nextTile + allTiles[i-1]._TileParentCoord;
            }
            if (Tramos[rnd.Next(0, Tramos.Length)] == "recto1"){
               allTiles[i] = new Tile(ContentFolder3D,ContentFolderEffects,
                _relativePosc,Content).SetUpTileRecto1();
            }
        } 
        return allTiles;
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
        for (var j = 0; j < _allTiles.Length; j++){
            for (var i = 0; i < _allTiles[j]._TileObjs.Count; i++){
                _allTiles[j]._TileObjs[i].Update(gameTime);
            }
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
        for (int j = 0; j < _allTiles.Length; j++){
            for (var i = 0; i < _allTiles[j]._TileObjs.Count; i++){
                _allTiles[j]._TileObjs[i].DrawOn(gameTime, _cameraInUse, _cameraInUse.GetProjection());
            }     
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