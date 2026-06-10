using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
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

    private Road _road;
    // COLECCIONABLES
    private List<Collectible> _collectibles = new List<Collectible>();

    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

        var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.PreferredBackBufferWidth = screenWidth / 2;
        _graphics.PreferredBackBufferHeight = screenHeight / 2;

        _graphics.IsFullScreen = false;

        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;

        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        IsMouseVisible = false;

        _graphics.ApplyChanges();

        GraphicsDevice.RasterizerState = new RasterizerState()
        {
            CullMode = CullMode.None
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
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _debugEffect = Content.Load<Effect>(AssetPaths.ContentFolderEffects + "BasicShader");

        var trafficConeModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "road-tiles/construction-cone"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.DarkOrchid
        );

        var constructionLightModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "road-tiles/construction-light"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.DarkOrchid
        );

        var obstacleModels = new List<CustomModel>
        {
            trafficConeModel,
            constructionLightModel,
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


        // =========================
        // PLAYER
        // =========================

        var lightModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/race"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.Red
        );

        var mediumModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/sedan-sports"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.DarkOrange
        );

        var heavyModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/delivery"
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.DarkOliveGreen
        );

        _lightVehicle = new Vehicle(
            lightModel,
            Vector3.Zero,
            VehiclePresets.Light,
            VehicleType.Light
        );

        _mediumVehicle = new Vehicle(
            mediumModel,
            Vector3.Zero,
            VehiclePresets.Medium,
            VehicleType.Medium
        );

        _heavyVehicle = new Vehicle(
            heavyModel,
            Vector3.Zero,
            VehiclePresets.Heavy,
            VehicleType.Heavy
        );

        // Vehículo inicial:
        _playerVehicle = _mediumVehicle;

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
        // CHANGE VEHICLE
        // =========================

        if (keyboardState.IsKeyDown(Keys.D1) &&
            _previousKeyboardState.IsKeyUp(Keys.D1))
        {
            ChangeVehicle(_lightVehicle);
        }

        if (keyboardState.IsKeyDown(Keys.D2) &&
            _previousKeyboardState.IsKeyUp(Keys.D2))
        {
            ChangeVehicle(_mediumVehicle);
        }

        if (keyboardState.IsKeyDown(Keys.D3) &&
            _previousKeyboardState.IsKeyUp(Keys.D3))
        {
            ChangeVehicle(_heavyVehicle);
        }

        // =========================
        // UPDATE PLAYER
        // =========================

        _playerVehicle.Update(gameTime);

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

        base.Update(gameTime);
    }

    private void ChangeVehicle(Vehicle newVehicle)
    {
        // conservar posicion, rotacion y velocidad

        newVehicle.Position = _playerVehicle.Position;

        newVehicle.RotationY = _playerVehicle.RotationY;

        newVehicle._speed = 0;

        _playerVehicle = newVehicle;
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
                    _playerVehicle.CollisionImpact(
                        obstacle.Damage,
                        obstacle.SpeedMultiplier
                    );

                    obstacle.Deactivate();
                }
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

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

            foreach (var tile in _road.Tiles)
            {
                foreach (var obstacle in tile.Obstacles)
                {
                    DrawBoundingBox(
                        obstacle.BoundingBox,
                        _cameraInUse,
                        Color.Yellow
                    );
                }
            }
        }

        base.Draw(gameTime);
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
}