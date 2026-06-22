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

    // CAMARAS
    private FreeCamera _freeCamera;
    private FollowCamera _followCamera;
    private Camera _cameraInUse;
    private bool _useFreeCamera;
    private bool _mouseCaptured = true;

    // TECLADO
    private KeyboardState _previousKeyboardState;


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

    public enum Scene
    {
        Menu,
        Road,
        GameOver
    }
    Scene _sceneNum = Scene.Menu;

    private bool _gameOver;
    private float _gameOverTimer;
    private const float GameOverDelay = 2f;

    //Para que sean accesibles globalmente
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
        _worldMenuCar = Matrix.CreateScale(0.3f)  ;
        _worldMenuCar2 =  Matrix.CreateTranslation(500f,-100f,0f ) *  Matrix.CreateScale(0.1f) ;
        _worldMenuCar3 =  Matrix.CreateTranslation(-500f,-100f,0f ) *  Matrix.CreateScale(0.1f) ;

        IsMouseVisible = false;

        _graphics.ApplyChanges();

        GraphicsDevice.RasterizerState = new RasterizerState()
        {   
            CullMode = CullMode.CullClockwiseFace,
        };

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

        _debugEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "BasicShader");

        var carKitColormap = Content.Load<Texture2D>(
            AssetPaths.ContentFolder3D +
            "car-kit/Textures/colormap"
        );

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

        //Debe ejecuitarse antes del new Road()
        Collectible.LoadLocalModels(Content);
        Tile.LoadModels(Content);

        //Genero la calle a partir del bioma para permitir que el primer bioma también sea aleatorio
        _road = new Road(
            new AsphaltBiome(
                null,
                new GameMode(BiomeType.RANDOM, GameDifficulty.EASY)
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
            case Scene.Menu:

            if (keyboardState.IsKeyDown(Keys.D1) &&
                _previousKeyboardState.IsKeyUp(Keys.D1))
            {
                _playerVehicle = _lightVehicle;
                _sceneNum = Scene.Road;
            }

            if (keyboardState.IsKeyDown(Keys.D2) &&
                _previousKeyboardState.IsKeyUp(Keys.D2))
            {
                _playerVehicle = _mediumVehicle;
                _sceneNum = Scene.Road;
            }

            if (keyboardState.IsKeyDown(Keys.D3) &&
                _previousKeyboardState.IsKeyUp(Keys.D3))
            {
                _playerVehicle = _heavyVehicle;
                _sceneNum = Scene.Road;
            }

            break;

            default: 
        
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

        _playerVehicle.Update(gameTime);
        _playerVehicle.UpdateSound(_instanciaSonidoMotor, _sonidoFrenado);
        CheckObstacleCollisions();

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

        // Cuando Termina el juego
        if (!_gameOver && _playerVehicle.CurrentHealth == 0)
        {
            _gameOver = true;
            _gameOverTimer = GameOverDelay;
        }
        if (_gameOver)
        {
            _gameOverTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_gameOverTimer <= 0)
            {
                _sceneNum = Scene.GameOver;
            }
        }


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

                if (_playerVehicle.BoundingBox
                    .Intersects(obstacle.BoundingBox))
                {
                    Vector3 obstacleCenter = (obstacle.BoundingBox.Min + obstacle.BoundingBox.Max) / 2f;
                    Vector3 vehicleCenter = (_playerVehicle.BoundingBox.Min + _playerVehicle.BoundingBox.Max) / 2f;

                    Vector3 diff = obstacleCenter - vehicleCenter;
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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        //Cambio de esena
        switch (_sceneNum){
            case  Scene.Menu: 

            DrawCenterTextY("MENU",10,10);
            DrawCenterTextY("Preciona 1, 2 O 3  para empezar",300,2);
            DrawCenterTextY("1 Para _lightVehicle",400,1);
            DrawCenterTextY("2 Para _mediumVehicle",450,1);
            DrawCenterTextY("3 Para _heavyVehicle",500,1);

            lightModel.Draw(_worldMenuCar, _cameraMenu.View, _cameraMenu.Projection);
            heavyModel.Draw(_worldMenuCar2, _cameraMenu.View, _cameraMenu.Projection);
            mediumModel.Draw(_worldMenuCar3 , _cameraMenu.View, _cameraMenu.Projection);

            _worldMenuCar *= Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *20f ));
            _worldMenuCar2 *= Matrix.CreateRotationX(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) * Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) ;
            _worldMenuCar3 *= Matrix.CreateRotationX(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) * Matrix.CreateRotationY(MathHelper.ToRadians(Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) *40f )) ;
            break;

            case  Scene.GameOver: 
                DrawCenterTextY("GAME OVER",10,10);
            break;
                
            default: 
        // =========================
        // DRAW SKYBOX
        // =========================
        {
            var view = _cameraInUse.GetView();
            var projection = _cameraInUse.GetProjection();
            var cameraPosition = Matrix.Invert(view).Translation;
            _skybox.Draw(view, projection, cameraPosition);
        }

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
            DrawBoundingBox(_playerVehicle.BoundingBox, _cameraInUse, Color.Red);

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
        DrawLeftText("Velocidad: " +string.Format("{0:N2}",_playerVehicle._speed), 10, 1); 
        DrawLeftText("Nafta: " +Convert.ToString(Math.Round(_playerVehicle.CurrentFuel)), 300, 1);
        DrawLeftText("Vida: " +Convert.ToString(Math.Round(_playerVehicle.CurrentHealth)), 500, 1); 
        DrawLeftText("Puntos: " +Convert.ToString(_playerVehicle.Score), 800, 1); 

        break; 
        }

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

    protected override void UnloadContent()
    {
        Content.Unload();

        base.UnloadContent();
    }

    public void DrawCenterText(string msg, float escala)
    {
        var W = GraphicsDevice.Viewport.Width;
        var H = GraphicsDevice.Viewport.Height;
        var size = font.MeasureString(msg) * escala;
        spriteBatch.Begin();
        spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.White);
        spriteBatch.End();
    }
    public void DrawLeftText(string msg, float X, float escala)
    {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred,null, 
            null, 
            DepthStencilState.Default,
            null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(X, 0, 0) );
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.White);
            spriteBatch.End();
    }
    public void DrawCenterTextY(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred,null, 
            null, 
            DepthStencilState.Default, 
            null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }
}