using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Menu;

public enum MenuState
{
    SelectingVehicle,
    SelectingDifficulty
}

public enum SelectedVehicle
{
    None,
    Light,
    Medium,
    Heavy
}

public class MainMenu
{
    private CustomModel _lightModel;
    private CustomModel _mediumModel;
    private CustomModel _heavyModel;

    private Matrix _worldMenuCar;
    private Matrix _worldMenuCar2;
    private Matrix _worldMenuCar3;

    public MenuState CurrentState { get; private set; } = MenuState.SelectingVehicle;
    public SelectedVehicle ChosenVehicle { get; private set; } = SelectedVehicle.None;
    public GameDifficulty ChosenDifficulty { get; private set; } = GameDifficulty.EASY;
    public bool IsFinished { get; private set; } = false;

    private KeyboardState _previousKeyboardState;

    public void LoadContent(ContentManager content)
    {
        var carKitColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "car-kit/Textures/colormap");

        _lightModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "car-kit/race"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            carKitColormap
        );

        _mediumModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "car-kit/sedan-sports"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            carKitColormap
        );

        _heavyModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "car-kit/delivery"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            carKitColormap
        );
        
        _worldMenuCar = Matrix.CreateTranslation(500f, -100f, 0f) * Matrix.CreateScale(0.1f);
        _worldMenuCar2 = Matrix.CreateTranslation(0f, -100f, 0f) * Matrix.CreateScale(0.1f);
        _worldMenuCar3 = Matrix.CreateTranslation(-500f, -100f, 0f) * Matrix.CreateScale(0.1f);
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (CurrentState == MenuState.SelectingVehicle)
        {
            if (keyboardState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
            {
                ChosenVehicle = SelectedVehicle.Light;
                CurrentState = MenuState.SelectingDifficulty;
            }
            if (keyboardState.IsKeyDown(Keys.D2) && _previousKeyboardState.IsKeyUp(Keys.D2))
            {
                ChosenVehicle = SelectedVehicle.Medium;
                CurrentState = MenuState.SelectingDifficulty;
            }
            if (keyboardState.IsKeyDown(Keys.D3) && _previousKeyboardState.IsKeyUp(Keys.D3))
            {
                ChosenVehicle = SelectedVehicle.Heavy;
                CurrentState = MenuState.SelectingDifficulty;
            }
        }
        else if (CurrentState == MenuState.SelectingDifficulty)
        {
            if (keyboardState.IsKeyDown(Keys.M) && _previousKeyboardState.IsKeyUp(Keys.M))
            {
                ChosenDifficulty = GameDifficulty.EASY;
                IsFinished = true;
            }
            if (keyboardState.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N))
            {
                ChosenDifficulty = GameDifficulty.MEDIUM;
                IsFinished = true;
            }
            if (keyboardState.IsKeyDown(Keys.B) && _previousKeyboardState.IsKeyUp(Keys.B))
            {
                ChosenDifficulty = GameDifficulty.HARD;
                IsFinished = true;
            }
        }

        _previousKeyboardState = keyboardState;
    }

    public void Draw(GameTime gameTime, TGCGame game, CameraStc cameraMenu)
    {
        if (CurrentState == MenuState.SelectingVehicle)
        {
            game.DrawCenterTextY("MENU", 50f, 3, Color.White);
            game.DrawCenterTextY("Preciona 1, 2 O 3  para empezar", 130f, 1, Color.White);
            
            game.DrawCenterTextY("1 Para Light Vehicle", 200f, 1, Color.LightBlue);
            game.DrawCenterTextY("2 Para Medium Vehicle", 250f, 1, Color.LightGreen);
            game.DrawCenterTextY("3 Para Heavy Vehicle", 300f, 1, Color.LightCoral);

            Matrix rotation = Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds);
            
            _lightModel.DrawUnlit(rotation * _worldMenuCar, cameraMenu.View, cameraMenu.Projection);
            _mediumModel.DrawUnlit(rotation * _worldMenuCar2, cameraMenu.View, cameraMenu.Projection);
            _heavyModel.DrawUnlit(rotation * _worldMenuCar3, cameraMenu.View, cameraMenu.Projection);
        }
        else if (CurrentState == MenuState.SelectingDifficulty)
        {
            game.DrawCenterTextY("Dificultad", 50f, 3, Color.White);
            game.DrawCenterTextY("M Para Facil", 150f, 1, Color.LimeGreen);
            game.DrawCenterTextY("N Para Normal", 200f, 1, Color.Yellow);
            game.DrawCenterTextY("B Para Dificil", 250f, 1, Color.Red);
        }
    }
}
