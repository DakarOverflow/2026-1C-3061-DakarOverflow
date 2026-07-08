using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

using TGC.MonoGame.TP.Menu;

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
    private ShadowMapRenderer _shadowMapRenderer;
    private ShadowPostProcessor _shadowPostProcessor;
    // DEBUG SHADOWS: T = activar/desactivar sombras, Y = ver el shadow map en pantalla
    private bool _shadowsEnabled = true;
    private bool _showShadowMap = false;
    // DEBUG LIGHTING: U = ciclar vista (0=normal, 1=normales, 2=difuso, 3=fullbright)
    private int _debugView = 0;
    private Effect _texturedEffect;
    private readonly RasterizerState _mainRasterizerState = new()
    {
        CullMode = CullMode.CullCounterClockwiseFace
    };

    // CAMARAS
    private FreeCamera _freeCamera;
    private FollowCamera _followCamera;
    private Camera _cameraInUse;
    private bool _useFreeCamera;
    private bool _mouseCaptured = true;

    public Vector3 _freeCameraOffset;

    // TECLADO
    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;


    // PLAYER
    private Vehicle _playerVehicle;

    // VEHICULOS
    private Vehicle _lightVehicle;
    private Vehicle _mediumVehicle;
    private Vehicle _heavyVehicle;

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
   GameOver
}
Scene _sceneNum = Scene.Menu;

    private bool _gameOver;
    private float _gameOverTimer;
    private const float GameOverDelay = 2f;

    bool _godMode = false;
    //Para que sean accesibles globalmente
    #region  Menu Objecs
    CameraStc _cameraMenu;

    GameDifficulty _currentDifficulty = GameDifficulty.EASY;
    #endregion

    private MainMenu _mainMenu;
    private HUD _hud;
    private List<CustomModel> _obstacleModels;
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
        // Modelos Autos Menu
        // (Se movieron a MainMenu.cs)
        _freeCameraOffset = new Vector3(110f, 50f, 110f);


        IsMouseVisible = false;

        _graphics.ApplyChanges();

        GraphicsDevice.RasterizerState = _mainRasterizerState;

        _freeCamera = new FreeCamera(
            _freeCameraOffset,
            Vector3.Zero,
            GraphicsDevice
        );

        _followCamera = new FollowCamera(GraphicsDevice);

        Window.ClientSizeChanged += _freeCamera.OnClientSizeChanged;

        Window.ClientSizeChanged += _followCamera.OnClientSizeChanged;

        _cameraInUse = _followCamera;

        Services.AddService<GraphicsDeviceManager>(_graphics);
        Services.AddService<ContentManager>(Content);

        SoundManager.Initialize(Content);

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
        _texturedEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader");
        _shadowMapRenderer = new ShadowMapRenderer(GraphicsDevice);
        var shadowPostProcessEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "ShadowPostProcess");
        _shadowPostProcessor = new ShadowPostProcessor(GraphicsDevice, shadowPostProcessEffect);

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

        _mainMenu = new MainMenu();
        _mainMenu.LoadContent(Content);

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

        _obstacleModels = new List<CustomModel>
        {
            policeModel,
            taxiModel,
            ambulanceModel,
        };

        // UI 
        _hud = new HUD();
        _hud.LoadContent(Content, GraphicsDevice, font, _blankTexture);
        _hud.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);


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
            _obstacleModels
        );

        // =========================
        // PLAYER
        // =========================

        var lightBodyModel = new CustomModel(
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

        var mediumBodyModel = new CustomModel(
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

        var heavyBodyModel = new CustomModel(
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

        var lightWheelModel = new CustomModel(
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

        var mediumWheelModel = new CustomModel(
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

        var heavyWheelModel = new CustomModel(
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
            Vector3.Zero + new Vector3(0f,-35f,0f),
            VehiclePresets.Light,
            VehicleType.Light,
            new Vector3(60f, 50f, 100f),
            new Vector3(40f,30f,66f),
            new Vector3(40f,30f,-83f)
        );

        _mediumVehicle = new Vehicle(
            mediumBodyModel,
            mediumWheelModel,
            Vector3.Zero + new Vector3(0f,-35f,0f),
            VehiclePresets.Medium,
            VehicleType.Medium,
            new Vector3(60f, 50f, 100f),
            new Vector3(40f,30f,66f),
            new Vector3(40f,30f,-67f)
        );

        _heavyVehicle = new Vehicle(
            heavyBodyModel,
            heavyWheelModel,
            Vector3.Zero + new Vector3(0f,-35f,0f),
            VehiclePresets.Heavy,
            VehicleType.Heavy,
            new Vector3(65f, 50f, 120f),
            new Vector3(40f,30f,103f),
            new Vector3(40f,30f,-62f)
        );

        // Vehículo inicial:
        _playerVehicle = _mediumVehicle;

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
    
        var keyboardState = Keyboard.GetState();

        // EXIT

        if (keyboardState.IsKeyDown(Keys.Escape) && _sceneNum != Scene.Menu)
        {
            Exit();
        }
        //FULl SCREN
        if (keyboardState.IsKeyDown(Keys.P) &&
            _previousKeyboardState.IsKeyUp(Keys.P))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            if (_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = displayMode.Width;
                _graphics.PreferredBackBufferHeight = displayMode.Height;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = displayMode.Width;
                _graphics.PreferredBackBufferHeight = (int)(displayMode.Height * 0.90f);
            }
            _graphics.ApplyChanges();
            
            // Forzar actualización de resolución en las cámaras y el HUD
            _freeCamera.OnClientSizeChanged(null, EventArgs.Empty);
            _followCamera.OnClientSizeChanged(null, EventArgs.Empty);
            _hud.Initialize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        }
        
        
        switch (_sceneNum)
        {
            case Scene.Menu:
                _mainMenu.Update(gameTime, this);
                if (_mainMenu.IsFinished)
                {
                    switch (_mainMenu.ChosenVehicle)
                    {
                        case SelectedVehicle.Light: _playerVehicle = _lightVehicle; break;
                        case SelectedVehicle.Medium: _playerVehicle = _mediumVehicle; break;
                        case SelectedVehicle.Heavy: _playerVehicle = _heavyVehicle; break;
                    }
                    _currentDifficulty = _mainMenu.ChosenDifficulty;
                    
                    _sceneNum = Scene.Road;
                    SoundManager.GetInstance().StartMotorSound();
                }
                break;
            default:
                if (_sceneNum == Scene.GameOver)
                {
                    if (keyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
                    {
                        RestartGame();
                    }
                    else if (keyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                    {
                        ReturnToMainMenu();
                    }
                    _previousKeyboardState = keyboardState;
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
            _freeCamera._position = _playerVehicle.GetWorld().Translation + _freeCameraOffset ;
        }
        //GOD MODE
        if (keyboardState.IsKeyDown(Keys.G) &&_previousKeyboardState.IsKeyUp(Keys.G)) _godMode = !_godMode ;

        // DEBUG SHADOWS: T = sombras on/off, Y = ver el shadow map
        if (keyboardState.IsKeyDown(Keys.T) && _previousKeyboardState.IsKeyUp(Keys.T)) _shadowsEnabled = !_shadowsEnabled;
        if (keyboardState.IsKeyDown(Keys.Y) && _previousKeyboardState.IsKeyUp(Keys.Y)) _showShadowMap = !_showShadowMap;
        // DEBUG LIGHTING: U = ciclar vista de debug (0..3)
        if (keyboardState.IsKeyDown(Keys.U) && _previousKeyboardState.IsKeyUp(Keys.U)) _debugView = (_debugView + 1) % 4;
        

        // =========================
        // UPDATE PLAYER
        // =========================
        if (!_useFreeCamera){
        // Actualizo el coeficiente de fricción según el bioma actual
        _playerVehicle.FrictionCoefficient = _road.GetFrictionAtPosition(_playerVehicle.Position);

            _playerVehicle.Update(gameTime);

            _road.ConstrainVehicleToRoad(_playerVehicle);
            _playerVehicle.RefreshCollisionVolumes();

            SoundManager.GetInstance().Update(gameTime);
            if(!_godMode) CheckObstacleCollisions();
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
        if (!_gameOver && (_playerVehicle.CurrentHealth <= 0 || _playerVehicle.CurrentFuel <= 0))
        {
            _gameOver = true;
            _gameOverTimer = GameOverDelay;
            if (_playerVehicle.CurrentHealth <= 0) 
            {
                SoundManager.GetInstance().SonarExplosion();
            }
        }
        if (_gameOver && !_useFreeCamera)
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

                if (_playerVehicle.OBB.Intersects(obstacle.OBB))
                {
                    Vector3 vehicleCenter = _playerVehicle.OBB.Center;
                    Vector3 closestPointOnObstacle = ClosestPointOnOBB(obstacle.OBB, vehicleCenter);

                    Vector3 diff = closestPointOnObstacle - vehicleCenter;
                    if (diff == Vector3.Zero)
                    {
                        diff = obstacle.OBB.Center - vehicleCenter;
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

    private Vector3 ClosestPointOnOBB(OrientedBoundingBox obb, Vector3 point)
    {
        Vector3 d = point - obb.Center;
        Vector3 closest = obb.Center;

        Vector3[] axes = { obb.Orientation.Right, obb.Orientation.Up, obb.Orientation.Backward };
        float[] extents = { obb.Extents.X, obb.Extents.Y, obb.Extents.Z };

        for (int i = 0; i < 3; i++)
        {
            Vector3 axis = axes[i];
            if (axis.LengthSquared() > 0.0001f) axis.Normalize();

            float distance = MathHelper.Clamp(Vector3.Dot(d, axis), -extents[i], extents[i]);
            closest += axis * distance;
        }

        return closest;
    }


    protected override void Draw(GameTime gameTime)
    {
        ResetMainRenderState(true);
        switch (_sceneNum)
        {
            case Scene.Menu:
                GraphicsDevice.Clear(Color.Black);
                _mainMenu.Draw(gameTime, this, _cameraMenu);
                break;
            default:
        // =========================
        // SOMBRAS COMO POST-PROCESADO
        // =========================
        var view = _cameraInUse.GetView();
        var projection = _cameraInUse.GetProjection();
        var cameraViewProjection = view * projection;

        // Pass 1: profundidad de la escena desde la LUZ (shadow map)
        _shadowMapRenderer.UpdateLightMatrices(_playerVehicle?.Position ?? Vector3.Zero);
        _shadowMapRenderer.RenderDepth(lightViewProjection =>
        {
            _road.DrawDepth(lightViewProjection);
            _playerVehicle.DrawDepth(lightViewProjection);
        });

        // Pass 2: profundidad de la escena desde la CÁMARA (para reconstruir la posición de mundo)
        _shadowPostProcessor.RenderCameraDepth(cameraVp =>
        {
            _road.DrawDepth(cameraVp);
            _playerVehicle.DrawDepth(cameraVp);
        }, cameraViewProjection);

        // Pass 3a: la escena se dibuja a una textura de color (SIN sombras)
        _shadowPostProcessor.BeginSceneColor(Color.CornflowerBlue);
        SetSceneRenderStates();
        SetLightingParams();

        // DRAW SKYBOX
        {
            var cameraPosition = Matrix.Invert(view).Translation;
            _skybox.Draw(view, projection, cameraPosition);
        }

        SetSceneRenderStates();

        // DRAW WORLD
        _road.Draw(
            gameTime,
            _cameraInUse
        );

        // DRAW PLAYER
        _playerVehicle.Draw(
            gameTime,
            _cameraInUse
        );

        // DRAW COLLECTIBLES
        foreach (var collectible in _collectibles)
        {
            collectible.Draw(gameTime, view, projection);
        }

        // DRAW HITBOXES
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
                    DrawOrientedBoundingBox(
                        obstacle.OBB,
                        _cameraInUse,
                        Color.Orange
                    );
                }
            }
        }

        // Pass 3b: full-screen quad que aplica las sombras sobre la imagen final (a pantalla)
        _shadowPostProcessor.Apply(
            _shadowMapRenderer.ShadowMap,
            _shadowMapRenderer.LightViewProjection,
            cameraViewProjection,
            _shadowMapRenderer.Size,
            _shadowsEnabled ? 0.55f : 0f,
            0.0025f
        );

        base.Draw(gameTime);

        // DEBUG: preview del shadow map (tecla Y). El canal rojo = profundidad desde la luz.
        if (_showShadowMap)
        {
            DrawShadowMapPreview();
        }

        // =========================
        // UI
        // =========================
        // DrawLeftText("Velocidad: " +string.Format("{0:N2}",_playerVehicle._speed), 10, 1,100);

        if (!_useFreeCamera){
            if (_godMode) DrawLeftText("GOD MODE",10f,1,0);
        
            _hud.Draw(spriteBatch, GraphicsDevice, _playerVehicle, _cameraMenu);
        }
        
        if (_cameraInUse is FreeCamera)
        {
            DrawCenterText("+", 3, Color.White);
        }

        if (_gameOver && !_useFreeCamera)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            
            float scaleGameOver = 3f;
            float scaleScore = 1.5f;
            float scaleHelp = 0.8f;
            
            var sizeGameOver = font.MeasureString("GAME OVER") * scaleGameOver;
            var scoreText = "PUNTAJE: " + _playerVehicle.Score.ToString();
            var sizeScore = font.MeasureString(scoreText) * scaleScore;
            var helpText = "Presiona [R] para volver a jugar o [Enter] para volver al menu";
            var sizeHelp = font.MeasureString(helpText) * scaleHelp;
            
            float totalHeight = sizeGameOver.Y + sizeScore.Y + sizeHelp.Y + 50f;
            float startY = (H - totalHeight) / 2f;
            
            float gameOverY = startY;
            float scoreY = startY + sizeGameOver.Y + 20f;
            float helpY = scoreY + sizeScore.Y + 30f;

            DrawCenterTextY("GAME OVER", gameOverY, scaleGameOver, Color.Red);
            DrawCenterTextY(scoreText, scoreY, scaleScore, Color.Yellow);
            DrawCenterTextY(helpText, helpY, scaleHelp, Color.White);
            SoundManager.GetInstance().StopMotorSound();
        }
        break; 
        }

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

    /// Setea los estados de render para dibujar la escena SIN cambiar el render target
    /// (durante el pase de color el target es la textura del post-procesador, no la pantalla).
    private void SetSceneRenderStates()
    {
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = _mainRasterizerState;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
    }

    private void SetLightingParams()
    {
        _texturedEffect.Parameters["LightDirection"]?.SetValue(_shadowMapRenderer.LightDirection);
        _texturedEffect.Parameters["DebugView"]?.SetValue((float)_debugView);
    }

    private void DrawShadowMapPreview()
    {
        const int previewSize = 320;
        var destination = new Rectangle(10, 10, previewSize, previewSize);

        // Opaque para poder ver una textura de un solo canal (Single) sin que el alpha la oculte.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        spriteBatch.Draw(_shadowMapRenderer.ShadowMap, destination, Color.White);
        spriteBatch.End();
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
        _shadowMapRenderer?.Dispose();
        _shadowPostProcessor?.Dispose();
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
    public void DrawCenterTextY(string msg, float Y, float escala,Color Color)
    {
        var W = GraphicsDevice.Viewport.Width;
        var size = font.MeasureString(msg) * escala;
        spriteBatch.Begin(SpriteSortMode.Deferred,null, 
        null, 
        DepthStencilState.Default, 
        null, null,
            Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
        spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color);
        spriteBatch.End();
    }
    
    public void DrawTextCenteredAtXY(string msg, float X, float Y, float escala, Color color)
    {
        var size = font.MeasureString(msg) * escala;
        spriteBatch.Begin(SpriteSortMode.Deferred,null, 
        null, 
        DepthStencilState.Default, 
        null, null,
            Matrix.CreateScale(escala) * Matrix.CreateTranslation(X - (size.X / 2), Y, 0));
        spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
        spriteBatch.End();
    }

    private void RestartGame()
    {
        _gameOver = false;
        _gameOverTimer = GameOverDelay;
        _sceneNum = Scene.Road;

        // Resetear vehículos a sus posiciones iniciales y restablecer estadísticas
        _lightVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));
        _mediumVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));
        _heavyVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));

        switch (_mainMenu.ChosenVehicle)
        {
            case SelectedVehicle.Light: _playerVehicle = _lightVehicle; break;
            case SelectedVehicle.Medium: _playerVehicle = _mediumVehicle; break;
            case SelectedVehicle.Heavy: _playerVehicle = _heavyVehicle; break;
        }

        // Recrear calle
        _road = new Road(
            new AsphaltBiome(
                null,
                new GameMode(BiomeType.RANDOM, _currentDifficulty)
            ).GenerateNewTileOf(
                TileType.STRAIGHT_LINE,
                new Vector3(0f, -50f, 0f),
                0f
            ),
            _obstacleModels
        );

        // Re-inicializar cámaras
        _freeCamera = new FreeCamera(
            new Vector3(110f, 10f, 110f),
            Vector3.Zero,
            GraphicsDevice
        );
        _followCamera = new FollowCamera(GraphicsDevice);
        _useFreeCamera = false;
        _cameraInUse = _followCamera;

        _mouseCaptured = true;
        IsMouseVisible = false;

        SoundManager.GetInstance().StartMotorSound();
    }

    private void ReturnToMainMenu()
    {
        _gameOver = false;
        _gameOverTimer = GameOverDelay;
        _sceneNum = Scene.Menu;

        _mainMenu.Reset();

        // Resetear vehículos a sus posiciones iniciales y restablecer estadísticas
        _lightVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));
        _mediumVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));
        _heavyVehicle.Reset(Vector3.Zero + new Vector3(0f,-34f,0f));

        // Recrear calle
        _road = new Road(
            new AsphaltBiome(
                null,
                new GameMode(BiomeType.RANDOM, _currentDifficulty)
            ).GenerateNewTileOf(
                TileType.STRAIGHT_LINE,
                new Vector3(0f, -50f, 0f),
                0f
            ),
            _obstacleModels
        );

        // Re-inicializar cámaras
        _freeCamera = new FreeCamera(
            new Vector3(110f, 10f, 110f),
            Vector3.Zero,
            GraphicsDevice
        );
        _followCamera = new FollowCamera(GraphicsDevice);
        _useFreeCamera = false;
        _cameraInUse = _followCamera;

        _mouseCaptured = true;
        IsMouseVisible = false;
    }
}
