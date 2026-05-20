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


        _road = new Road(new TileRecta(Content, new Vector3(0f, -50f, 0f)));


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

        // =========================
        // COLECCIONABLES
        // =========================

        var fuelTankModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/box" // TODO: Cambiar por el modelo real
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.Red
        );

        var wrenchModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/debris-bolt" // TODO: Cambiar por el modelo real
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.Gray
        );

        var coinModel = new CustomModel(
            Content.Load<Model>(
                AssetPaths.ContentFolder3D +
                "car-kit/debris-nut" // TODO: Cambiar por el modelo real
            ),
            Content.Load<Effect>(
                AssetPaths.ContentFolderEffects +
                "BasicShader"
            ),
            Color.Gold
        );

        // Para instanciar los coleccionables se indica el tipo, el modelo, la posicion y el valor que otorga.
        _collectibles.Add(new Collectible(
            CollectibleType.FuelTank, fuelTankModel, new Vector3(-150f, 0f, -500f), 100f));

        _collectibles.Add(new Collectible(
            CollectibleType.Wrench, wrenchModel, new Vector3(0f, 0f, -500f), 50f));

        _collectibles.Add(new Collectible(
            CollectibleType.Coin, coinModel, new Vector3(150f, 0f, -500f), 10f));

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

        _road.UpdateFor(_playerVehicle,gameTime);

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

        newVehicle.Position =
            _playerVehicle.Position;

        newVehicle.RotationY =
            _playerVehicle.RotationY;

        newVehicle._speed = 
            _playerVehicle._speed;

        _playerVehicle = newVehicle;
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
            collectible.Draw(_cameraInUse.GetView(), _cameraInUse.GetProjection());
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        Content.Unload();

        base.UnloadContent();
    }
}