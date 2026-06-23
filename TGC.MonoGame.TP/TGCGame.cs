using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    // SHADDER PARA DEBUGUEAR
    private Effect _debugEffect;
    private bool _showHitboxes = false;
    private RenderTarget2D _shadowMap;
    private Matrix _lightViewProjection;
    private const int ShadowMapSize = 2048;
    private static readonly Vector3 LightDirection = Vector3.Normalize(new Vector3(1f, 1f, 1f));
    private readonly RasterizerState _mainRasterizerState = new()
    {
        CullMode = CullMode.None
    };
    private readonly RasterizerState _shadowRasterizerState = new()
    {
        CullMode = CullMode.None,
        DepthBias = 0f,
        SlopeScaleDepthBias = 0f
    };

    // CAMARAS
    private FreeCamera _freeCamera;
    private FollowCamera _followCamera;
    private Camera _cameraInUse;
    private bool _useFreeCamera;
    private bool _mouseCaptured = true;

    // TECLADO
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;


    // PLAYER
    private Vehicle _playerVehicle;

    // VEHICULOS
    private Vehicle _lightVehicle;
    private Vehicle _mediumVehicle;
    private Vehicle _heavyVehicle;

    //Sonidos
    private SoundEffectInstance _instanciaSonidoMotor;
    private SoundEffect _sonidoFrenado;
    private Road _road;
    private Skybox _skybox;
    // COLECCIONABLES
    private List<Collectible> _collectibles = new List<Collectible>();

    public SpriteFont font;
    public SpriteBatch spriteBatch;
    private Texture2D _blankTexture;

    public enum Scene
{
   Menu,
   Road,
   Difficulty,
   GameOver
}
Scene _sceneNum = Scene.Menu;

    private bool _gameOver;
    private float _gameOverTimer;
    private const float GameOverDelay = 2f;

    //Para que sean accesibles globalmente
    #region  Menu Objecs
    CustomModel lightModel;
    CustomModel mediumModel;
    CustomModel heavyModel;
    CustomModel lightBodyModel;
    CustomModel mediumBodyModel;
    CustomModel heavyBodyModel;
    CustomModel lightWheelModel;
    CustomModel mediumWheelModel;
    CustomModel heavyWheelModel;

    Matrix _worldMenuCar;
    Matrix _worldMenuCar2;
    Matrix _worldMenuCar3;

    CameraStc _cameraMenu;

    float _worldMenuCar3Rotation;
    GameDifficulty _currentDifficulty = GameDifficulty.EASY;
    #endregion


CustomModel FuelTank;
CustomModel Wrench;
CustomModel Coin;
Matrix _worldFuelTank;
Matrix _worldWrench;
Matrix _worldCoin;

Matrix _worldMainCarHud;
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

        float screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.PreferredBackBufferWidth  = screenWidth  ;
        _graphics.PreferredBackBufferHeight = (int) (screenHeight * 0.90f);

        _graphics.IsFullScreen = false;

        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _cameraMenu = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, Vector3.UnitZ);
        // _worldMenuCar = Matrix.Identity;
        # region UI Road

        _worldMenuCar = Matrix.CreateScale(0.3f)  ;
        _worldMenuCar2 =  Matrix.CreateTranslation(500f,-100f,0f ) *  Matrix.CreateScale(0.1f) ;
        _worldMenuCar3 =  Matrix.CreateTranslation(-500f,-100f,0f ) *  Matrix.CreateScale(0.1f) ;

   
        // Modelos de Coleccionables para el HUD
        _worldFuelTank = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(- _graphics.PreferredBackBufferWidth/17 , - _graphics.PreferredBackBufferHeight/17 , 0f);
        _worldWrench = Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(- _graphics.PreferredBackBufferWidth/19 , - _graphics.PreferredBackBufferHeight/17 , 0f);
        _worldCoin = Matrix.CreateScale(0.2f) * Matrix.CreateRotationZ(MathHelper.PiOver4) * Matrix.CreateTranslation(_graphics.PreferredBackBufferWidth/20f, _graphics.PreferredBackBufferHeight/19f, 0f);
        // _worldMainCarHud = Matrix.CreateScale(0.1f)*  Matrix.CreateRotationX(MathHelper.PiOver2)  * Matrix.CreateTranslation(-100f,_graphics.PreferredBackBufferHeight/18,0f ) ;
        #endregion 

        IsMouseVisible = false;

        _graphics.ApplyChanges();

        GraphicsDevice.RasterizerState = _mainRasterizerState;

        _freeCamera = new FreeCamera(
            new Vector3(110f, 10f, 110f),
            Vector3.Zero,
            GraphicsDevice
        );

        _followCamera = new FollowCamera(GraphicsDevice);

        Window.ClientSizeChanged += _freeCamera.OnClientSizeChanged;

        Window.ClientSizeChanged += _followCamera.OnClientSizeChanged;

        _cameraInUse = _followCamera;

        Services.AddService<GraphicsDeviceManager>(_graphics);
        Services.AddService<ContentManager>(Content);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Fuente
        font = Content.Load<SpriteFont>(AssetPaths.ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _blankTexture = new Texture2D(GraphicsDevice, 1, 1);
        _blankTexture.SetData(new[] { Color.White });

        _debugEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "BasicShader");
        _shadowMap = new RenderTarget2D(GraphicsDevice, ShadowMapSize, ShadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24);

        var carKitColormap = Content.Load<Texture2D>(
            AssetPaths.ContentFolder3D +
            "car-kit/Textures/colormap"
        );

        var survivalKitColormap = Content.Load<Texture2D>(AssetPaths.ContentFolder3D + "survival-kit/Textures/colormap");
        var toyCarKitColormap = Content.Load<Texture2D>(AssetPaths.ContentFolder3D + "toy-car-kit/Textures/colormap");

        var skyboxEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader");
        var skyboxTextures = new Dictionary<string, Texture2D>
        {
            { "front", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_ft") },
            { "back", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_bk") },
            { "left", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_lf") },
            { "right", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_rt") },
            { "up", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_up") },
            { "down", Content.Load<Texture2D>(AssetPaths.ContentFolderTextures + "barren_dn") }
        };
        _skybox = new Skybox(GraphicsDevice, skyboxEffect, skyboxTextures, 500f);

        var policeModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/police"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        var taxiModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/taxi"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        var ambulanceModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/ambulance"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        var obstacleModels = new List<CustomModel>
        {
            policeModel,
            taxiModel,
            ambulanceModel,
        };

        // UI 
        FuelTank = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "survival-kit/barrel"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            survivalKitColormap
        );
        Wrench = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "survival-kit/tool-hammer"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            survivalKitColormap
        );

        Coin = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "toy-car-kit/item-coin-gold"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            toyCarKitColormap
        );


        //Debe ejecuitarse antes del new Road()
        Collectible.LoadLocalModels(Content);
        Tile.LoadModels(Content);

        //Genero la calle a partir del bioma para permitir que el primer bioma también sea aleatorio
        _road = new Road(
            new AsphaltBiome(
                null,
                new GameMode(BiomeType.RANDOM, _currentDifficulty)
            ).GenerateNewTileOf(
                TileType.STRAIGHT_LINE,
                new Vector3(0f, -50f, 0f),
                0f
            ),
            obstacleModels
        );

        //Modelos para el menú:
        lightModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/race"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        mediumModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/sedan-sports"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        heavyModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/delivery"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );


        // =========================
        // PLAYER
        // =========================

        lightBodyModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/light-vehicle-body"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        mediumBodyModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/medium-vehicle-body"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        heavyBodyModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/heavy-vehicle-body"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        lightWheelModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/wheel-front-left-light"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        mediumWheelModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/wheel-front-left-medium"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        heavyWheelModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/wheel-front-left-heavy"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "TexturedShader"
            ),
            carKitColormap
        );

        _lightVehicle = new Vehicle(
            lightBodyModel,
            lightWheelModel,
            Vector3.Zero + new Vector3(0f,-34f,0f),
            VehiclePresets.Light,
            VehicleType.Light,
            new Vector3(40f,30f,66f),
            new Vector3(40f,30f,-83f)
        );

        _mediumVehicle = new Vehicle(
            mediumBodyModel,
            mediumWheelModel,
            Vector3.Zero + new Vector3(0f,-34f,0f),
            VehiclePresets.Medium,
            VehicleType.Medium,
            new Vector3(40f,30f,66f),
            new Vector3(40f,30f,-67f)
        );

        _heavyVehicle = new Vehicle(
            heavyBodyModel,
            heavyWheelModel,
            Vector3.Zero + new Vector3(0f,-34f,0f),
            VehiclePresets.Heavy,
            VehicleType.Heavy,
            new Vector3(40f,30f,103f),
            new Vector3(40f,30f,-62f)
        );

        // Vehículo inicial:
        _playerVehicle = _mediumVehicle;

        var sonidoMotor = Content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "motor_auto");
        _instanciaSonidoMotor = sonidoMotor.CreateInstance();
        _instanciaSonidoMotor.IsLooped = true;
        _instanciaSonidoMotor.Play();

        _sonidoFrenado = Content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "auto_frenando");

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
    
        var keyboardState = Keyboard.GetState();

        // EXIT

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        switch (_sceneNum){
            case  Scene.Menu: 

            if (keyboardState.IsKeyDown(Keys.D1) &&
                _previousKeyboardState.IsKeyUp(Keys.D1))
            {
                _playerVehicle = _lightVehicle;
                _sceneNum = Scene.Difficulty;
            }

            if (keyboardState.IsKeyDown(Keys.D2) &&
                _previousKeyboardState.IsKeyUp(Keys.D2))
            {
                _playerVehicle = _mediumVehicle;
                _sceneNum = Scene.Difficulty;
            }

            if (keyboardState.IsKeyDown(Keys.D3) &&
                _previousKeyboardState.IsKeyUp(Keys.D3))
            {
                _playerVehicle = _heavyVehicle;
                _sceneNum = Scene.Difficulty;
            }

            break;
            case  Scene.Difficulty:
            if (keyboardState.IsKeyDown(Keys.M) &&
                _previousKeyboardState.IsKeyUp(Keys.M))
            {
                _currentDifficulty = GameDifficulty.EASY;
                _sceneNum = Scene.Road;
            }

            if (keyboardState.IsKeyDown(Keys.N) &&
                _previousKeyboardState.IsKeyUp(Keys.N))
            {
                _currentDifficulty = GameDifficulty.MEDIUM;
                _sceneNum = Scene.Road;
            }

            if (keyboardState.IsKeyDown(Keys.B) &&
                _previousKeyboardState.IsKeyUp(Keys.B))
            {
                _currentDifficulty = GameDifficulty.HARD;
                _sceneNum = Scene.Road;
            }
            break; 

            default:
                if (_sceneNum == Scene.GameOver)
                {
                    return;
                }
        // TOGGLE MOUSE

        if (keyboardState.IsKeyDown(Keys.M) &&
            _previousKeyboardState.IsKeyUp(Keys.M))
        {
            _mouseCaptured = !_mouseCaptured;

            IsMouseVisible = !_mouseCaptured;
        }

        // TOGGLE HITBOXES
        if (keyboardState.IsKeyDown(Keys.H) && 
            _previousKeyboardState.IsKeyUp(Keys.H))
        {
            _showHitboxes = !_showHitboxes;
        }

        // TOGGLE CAMERA

        if (keyboardState.IsKeyDown(Keys.F) &&
            _previousKeyboardState.IsKeyUp(Keys.F))
        {
            _useFreeCamera = !_useFreeCamera;
        }

        // =========================
        // UPDATE PLAYER
        // =========================
        if (!_useFreeCamera){
        // Actualizo el coeficiente de fricción según el bioma actual
        _playerVehicle.FrictionCoefficient = _road.GetFrictionAtPosition(_playerVehicle.Position);

        _playerVehicle.Update(gameTime);
        _playerVehicle.UpdateSound(_instanciaSonidoMotor, _sonidoFrenado);
        CheckObstacleCollisions();
        }

        // =========================
        // UPDATE WORLD
        // =========================

        _road.UpdateFor(_playerVehicle, gameTime);

        // =========================
        // UPDATE CAMERA
        // =========================

        if (_useFreeCamera)
        {
            _cameraInUse = _freeCamera;

            _freeCamera.Update(
                gameTime,
                _mouseCaptured
            );

            CheckFreeCameraModelPicking();
        }
        else
        {
            _cameraInUse = _followCamera;

            _followCamera.Update(
                gameTime,
                _playerVehicle.GetWorld()
            );
        }

        _previousKeyboardState = keyboardState;
        _previousMouseState = Mouse.GetState();

        // Cuando Termina el juego
        if (!_gameOver && _playerVehicle.CurrentHealth == 0)
        {
            _gameOver = true;
            _gameOverTimer = GameOverDelay;
        }
        if (_gameOver && !_useFreeCamera)
        {
            _gameOverTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_gameOverTimer <= 0)
            {
                _sceneNum = Scene.GameOver;
            }
        }

        _worldMenuCar3Rotation =MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *60f  * _playerVehicle._speed);
        base.Update(gameTime);
         break;
        }
    }

    private void CheckObstacleCollisions()
    {
        foreach (var tile in _road.Tiles)
        {
            foreach (var obstacle in tile.Obstacles)
            {
                if (!obstacle.IsActive)
                    continue;

                if (_playerVehicle.OBB
                    .Intersects(obstacle.BoundingBox))
                {
                    Vector3 vehicleCenter = _playerVehicle.OBB.Center;
                    Vector3 closestPointOnObstacle = Vector3.Clamp(vehicleCenter, obstacle.BoundingBox.Min, obstacle.BoundingBox.Max);

                    Vector3 diff = closestPointOnObstacle - vehicleCenter;
                    if (diff == Vector3.Zero) 
                    {
                        Vector3 obstacleCenter = (obstacle.BoundingBox.Min + obstacle.BoundingBox.Max) / 2f;
                        diff = obstacleCenter - vehicleCenter;
                    }

                    Vector3 diffLocal = Vector3.Transform(diff, Matrix.CreateRotationY(-_playerVehicle.RotationY));
                    
                    if (obstacle.IsFatalOnFrontalCollision && Math.Abs(diffLocal.Z) > Math.Abs(diffLocal.X))
                    {
                        // Choque frontal o trasero con objeto fatal -> termina la partida
                        _playerVehicle.CollisionImpact(9999f, 0f);
                    }
                    else
                    {
                        _playerVehicle.CollisionImpact(
                            obstacle.Damage,
                            obstacle.SpeedMultiplier
                        );
                    }

                    obstacle.Deactivate();
                }
            }
        }
    }

    private void CheckFreeCameraModelPicking()
    {
        MouseState mouseState = Mouse.GetState();

        if (mouseState.LeftButton != ButtonState.Pressed ||
            _previousMouseState.LeftButton != ButtonState.Released)
        {
            return;
        }

        Point screenPoint = _mouseCaptured
            ? new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2)
            : new Point(mouseState.X, mouseState.Y);
        Ray ray = ModelRaycaster.CreateRayFromScreenPoint(screenPoint, GraphicsDevice, _freeCamera);
        ModelRaycastHit? closestHit = null;
        Tile closestTile = null;

        foreach (var tile in _road.Tiles)
        {
            if (!tile.GetBoundingSphere().Intersects(ray).HasValue)
            {
                continue;
            }

            foreach (WorldObject obj in tile.WorldObjects)
            {
                if (!ModelRaycaster.TryIntersectObject(ray, obj, out ModelRaycastHit hit))
                {
                    continue;
                }

                if (!closestHit.HasValue || hit.Distance < closestHit.Value.Distance)
                {
                    closestHit = hit;
                    closestTile = tile;
                }
            }
        }

        if (closestHit.HasValue && closestTile != null)
        {
            Vector3 localPoint = closestHit.Value.LocalPoint;
            Vector3 worldPoint = closestHit.Value.WorldPoint;
            Vector3 addObstacleOffset = Vector3.Transform(
                worldPoint - closestTile.Position,
                Matrix.CreateRotationY(-closestTile.Rotation)
            );

            Console.WriteLine($"Model local coordinates hit: X={localPoint.X:F3}, Y={localPoint.Y:F3}, Z={localPoint.Z:F3}");
            Console.WriteLine($"World coordinates hit: X={worldPoint.X:F3}, Y={worldPoint.Y:F3}, Z={worldPoint.Z:F3}");
            Console.WriteLine($"AddObstacle offset for this tile: new Vector3({addObstacleOffset.X:F3}f, {addObstacleOffset.Y:F3}f, {addObstacleOffset.Z:F3}f)");
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        ResetMainRenderState(true);
        //Cambio de esena
        switch (_sceneNum){
            case  Scene.Menu: 

            DrawCenterTextY("MENU",10,10);
            DrawCenterTextY("Preciona 1, 2 O 3  para empezar",300,2);
            DrawCenterTextY("1 Para _lightVehicle",400,1);
            DrawCenterTextY("2 Para _mediumVehicle",450,1);
            DrawCenterTextY("3 Para _heavyVehicle",500,1);

            lightModel.DrawUnlit(_worldMenuCar, _cameraMenu.View, _cameraMenu.Projection);
            heavyModel.DrawUnlit(_worldMenuCar2, _cameraMenu.View, _cameraMenu.Projection);
            mediumModel.DrawUnlit(_worldMenuCar3 , _cameraMenu.View, _cameraMenu.Projection);

            _worldMenuCar *= Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *20f ));
            _worldMenuCar2 *= Matrix.CreateRotationX(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) * Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) ;
            _worldMenuCar3 *= Matrix.CreateRotationX(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) * Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) ;
            break;

            case  Scene.Difficulty: 

            DrawCenterTextY("Dificultad",10,10);
            DrawCenterTextY("M Para Facil",400,1);
            DrawCenterTextY("N Para Normal",450,1);
            DrawCenterTextY("B Para Dificil",500,1);

            break;

            default:
        // =========================
        // SHADOW MAP
        // =========================
        UpdateLightViewProjection();
        DrawShadowMap();
        ResetMainRenderState(true);
        ApplyShadowMap();

        // =========================
        // DRAW SKYBOX
        // =========================
        {
            var view = _cameraInUse.GetView();
            var projection = _cameraInUse.GetProjection();
            var cameraPosition = Matrix.Invert(view).Translation;
            _skybox.Draw(view, projection, cameraPosition);
        }

        ResetMainRenderState(false);

        // =========================
        // DRAW WORLD
        // =========================

        _road.Draw(
            gameTime,
            _cameraInUse
        );

        // =========================
        // DRAW PLAYER
        // =========================

        _playerVehicle.Draw(
            gameTime,
            _cameraInUse
        );

        // =========================
        // DRAW COLLECTIBLES
        // =========================

        foreach (var collectible in _collectibles)
        {
            collectible.Draw(gameTime, _cameraInUse.GetView(), _cameraInUse.GetProjection());
        }

        // =========================
        // DRAW HITBOXES
        // =========================
        if(_showHitboxes)
        {
            DrawOrientedBoundingBox(_playerVehicle.OBB, _cameraInUse, Color.Red);

            // Dibujar las cajas de los coleccionables activos
            foreach (var bb in _road.GetCollectibleHitboxes())
            {
                DrawBoundingBox(bb, _cameraInUse, Color.Yellow);
            }

            foreach (var tile in _road.Tiles)
            {
                foreach (var obstacle in tile.Obstacles)
                {
                    DrawBoundingBox(
                        obstacle.BoundingBox,
                        _cameraInUse,
                        Color.Orange
                    );
                }
            }
        }
        base.Draw(gameTime);
        // =========================
        // UI
        // =========================
        // DrawLeftText("Velocidad: " +string.Format("{0:N2}",_playerVehicle._speed), 10, 1,100); 
        DrawFuelBar();
        DrawHealthBar();
        DrawScoreUI();

        FuelTank.DrawUnlit(_worldFuelTank , _cameraMenu.View, _cameraMenu.Projection);
        Wrench.DrawUnlit(_worldWrench , _cameraMenu.View, _cameraMenu.Projection);
        Coin.DrawUnlit(_worldCoin , _cameraMenu.View, _cameraMenu.Projection);
        
        switch (_playerVehicle.Type){
            
            case VehicleType.Light:
                lightModel.DrawUnlit(_worldMainCarHud, _cameraMenu.View, _cameraMenu.Projection);
            break;
            case VehicleType.Medium:
                mediumModel.DrawUnlit(_worldMainCarHud, _cameraMenu.View, _cameraMenu.Projection);
            break;
            case VehicleType.Heavy:
                heavyModel.DrawUnlit(_worldMainCarHud, _cameraMenu.View, _cameraMenu.Projection);
            break;
            
        }
        _worldMainCarHud = Matrix.CreateScale(0.1f)*  Matrix.CreateRotationX(_worldMenuCar3Rotation)  * Matrix.CreateTranslation(100f,-_graphics.PreferredBackBufferHeight/18,0f );
        if (_cameraInUse is FreeCamera)
        {
            DrawCenterText("+", 3, Color.White);
        }

        if (_gameOver &&  !_useFreeCamera)
        {
            DrawCenterText("GAME OVER",10, Color.Red);
        }
        break; 
        }
       

    }

    private void UpdateLightViewProjection()
    {
        var center = _playerVehicle?.Position ?? Vector3.Zero;
        var lightPosition = center + LightDirection * 1600f;
        var lightView = Matrix.CreateLookAt(lightPosition, center, Vector3.Up);
        var lightProjection = Matrix.CreateOrthographic(3200f, 3200f, 1f, 5000f);
        _lightViewProjection = lightView * lightProjection;
    }

    private void DrawShadowMap()
    {
        var previousBlendState = GraphicsDevice.BlendState;
        var previousDepthStencilState = GraphicsDevice.DepthStencilState;
        var previousRasterizerState = GraphicsDevice.RasterizerState;

        GraphicsDevice.SetRenderTarget(_shadowMap);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);

        GraphicsDevice.RasterizerState = _shadowRasterizerState;

        _road.DrawDepth(_lightViewProjection);
        _playerVehicle.DrawDepth(_lightViewProjection);

        GraphicsDevice.RasterizerState = previousRasterizerState;
        GraphicsDevice.DepthStencilState = previousDepthStencilState;
        GraphicsDevice.BlendState = previousBlendState;
        GraphicsDevice.SetRenderTarget(null);
    }

    private void ResetMainRenderState(bool clear)
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Viewport = new Viewport(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = _mainRasterizerState;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        if (clear)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
        }
    }

    private void ApplyShadowMap()
    {
        _road.SetShadowMap(_shadowMap, _lightViewProjection);
        _playerVehicle.SetShadowMap(_shadowMap, _lightViewProjection);
    }

    private void DrawBoundingBox(BoundingBox box, Camera camera, Color color)
    {
        Vector3[] corners = box.GetCorners();

        VertexPositionColor[] vertices =
        {
        // cara inferior
        new(corners[0], color), new(corners[1], color),
        new(corners[1], color), new(corners[2], color),
        new(corners[2], color), new(corners[3], color),
        new(corners[3], color), new(corners[0], color),

        // cara superior
        new(corners[4], color), new(corners[5], color),
        new(corners[5], color), new(corners[6], color),
        new(corners[6], color), new(corners[7], color),
        new(corners[7], color), new(corners[4], color),

        // uniones
        new(corners[0], color), new(corners[4], color),
        new(corners[1], color), new(corners[5], color),
        new(corners[2], color), new(corners[6], color),
        new(corners[3], color), new(corners[7], color),
    };

        _debugEffect.Parameters["World"].SetValue(Matrix.Identity);
        _debugEffect.Parameters["View"].SetValue(camera.GetView());
        _debugEffect.Parameters["Projection"].SetValue(camera.GetProjection());
        _debugEffect.Parameters["DiffuseColor"].SetValue(color.ToVector3());

        _debugEffect.CurrentTechnique = _debugEffect.Techniques["DebugLineDrawing"];

        foreach (EffectPass pass in _debugEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length / 2);
        }
    }

    private void DrawOrientedBoundingBox(OrientedBoundingBox obb, Camera camera, Color color)
    {
        Vector3[] corners = obb.GetCorners();

        VertexPositionColor[] vertices =
        {
        // cara inferior
        new(corners[0], color), new(corners[1], color),
        new(corners[1], color), new(corners[2], color),
        new(corners[2], color), new(corners[3], color),
        new(corners[3], color), new(corners[0], color),

        // cara superior
        new(corners[4], color), new(corners[5], color),
        new(corners[5], color), new(corners[6], color),
        new(corners[6], color), new(corners[7], color),
        new(corners[7], color), new(corners[4], color),

        // uniones
        new(corners[0], color), new(corners[4], color),
        new(corners[1], color), new(corners[5], color),
        new(corners[2], color), new(corners[6], color),
        new(corners[3], color), new(corners[7], color),
        };

        _debugEffect.Parameters["World"].SetValue(Matrix.Identity);
        _debugEffect.Parameters["View"].SetValue(camera.GetView());
        _debugEffect.Parameters["Projection"].SetValue(camera.GetProjection());
        _debugEffect.Parameters["DiffuseColor"].SetValue(color.ToVector3());

        _debugEffect.CurrentTechnique = _debugEffect.Techniques["DebugLineDrawing"];

        foreach (EffectPass pass in _debugEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length / 2);
        }
    }

    protected override void UnloadContent()
    {
        _shadowMap?.Dispose();
        Content.Unload();

        base.UnloadContent();
    }

    public void DrawCenterText(string msg, float escala, Color color)
    {
        var W = GraphicsDevice.Viewport.Width;
        var H = GraphicsDevice.Viewport.Height;
        var size = font.MeasureString(msg) * escala;
        spriteBatch.Begin(SpriteSortMode.Deferred,null, 
            null, 
            DepthStencilState.Default, 
            null, null,
            Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
        spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
        spriteBatch.End();
    }
    public void DrawLeftText(string msg, float X, float escala,float Y)
    {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred,null, 
            null, 
            DepthStencilState.Default,
            null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(X, Y, 0) );
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.White);
            spriteBatch.End();
    }
    public void DrawCenterTextY(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred,null, 
            null, 
            DepthStencilState.Default, 
            null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }

    //UI
    public void DrawFuelBar()
    {
        float fuelPercentage = Math.Max(0, _playerVehicle.CurrentFuel / _playerVehicle.MaxFuel);
        int barWidth = 20;
        int barHeight = 150;
        
        // Proyectamos la posicion real del modelo 3D a coordenadas de pixeles en 2D
        Vector3 screenPos = GraphicsDevice.Viewport.Project(Vector3.Zero, _cameraMenu.Projection, _cameraMenu.View, _worldFuelTank);
        
        // Usamos la proyeccion matematica para centrar la barra. 
        // ¡Esto funciona a cualquier resolucion de pantalla!
        int x = (int)screenPos.X - (barWidth / 2);
        int y = (int)screenPos.Y - barHeight - 50; // Subimos la barra para que no tape el bidon

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        // Fondo gris (tanque vacio)
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, barHeight), Color.DarkGray);
        
        // Relleno (tanque lleno, se dibuja de abajo hacia arriba)
        Color fuelColor = fuelPercentage > 0.2f ? Color.Orange : Color.Red;
        int fillHeight = (int)(barHeight * fuelPercentage);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - fillHeight, barWidth, fillHeight), fuelColor);
        
        // Borde negro
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - 2, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, 2, barHeight), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x + barWidth - 2, y, 2, barHeight), Color.Black);
        
        spriteBatch.End();
    }

    public void DrawHealthBar()
    {
        float healthPercentage = Math.Max(0, _playerVehicle.CurrentHealth / _playerVehicle.MaxHealth);
        int barWidth = 20;
        int barHeight = 150;
        
        // Proyectamos la posicion real del modelo 3D del martillo a coordenadas de pixeles en 2D
        Vector3 screenPos = GraphicsDevice.Viewport.Project(Vector3.Zero, _cameraMenu.Projection, _cameraMenu.View, _worldWrench);
        
        int x = (int)screenPos.X - (barWidth / 2);
        int y = (int)screenPos.Y - barHeight - 50; // Subimos la barra para que no tape el martillo

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        // Fondo gris (vacio)
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, barHeight), Color.DarkGray);
        
        // Relleno (se dibuja de abajo hacia arriba)
        Color healthColor = healthPercentage > 0.3f ? Color.LimeGreen : Color.Red;
        int fillHeight = (int)(barHeight * healthPercentage);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - fillHeight, barWidth, fillHeight), healthColor);
        
        // Borde negro
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - 2, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, 2, barHeight), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x + barWidth - 2, y, 2, barHeight), Color.Black);
        
        spriteBatch.End();
    }

    public void DrawScoreUI()
    {
        // Proyectamos la posicion real de la moneda 3D a coordenadas de pixeles en 2D
        Vector3 screenPos = GraphicsDevice.Viewport.Project(Vector3.Zero, _cameraMenu.Projection, _cameraMenu.View, _worldCoin);
        
        // Ubicamos el texto a la derecha de la moneda
        int x = (int)screenPos.X + 40;
        int y = (int)screenPos.Y - 20; // Centrado verticalmente con la moneda
        
        string scoreText = _playerVehicle.Score.ToString();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        // Efecto de borde negro (dibujamos el texto desplazado en 4 direcciones)
        Vector2 position = new Vector2(x, y);
        spriteBatch.DrawString(font, scoreText, position + new Vector2(2, 0), Color.Black);
        spriteBatch.DrawString(font, scoreText, position + new Vector2(-2, 0), Color.Black);
        spriteBatch.DrawString(font, scoreText, position + new Vector2(0, 2), Color.Black);
        spriteBatch.DrawString(font, scoreText, position + new Vector2(0, -2), Color.Black);
        
        // Texto blanco por encima
        spriteBatch.DrawString(font, scoreText, position, Color.White);
        
        spriteBatch.End();
    }
}
