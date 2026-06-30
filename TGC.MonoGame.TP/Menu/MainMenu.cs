using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Menu;

public enum MenuState
{
    Home,
    Controls,
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
    private CustomModel _tileModel;

    public MenuState CurrentState { get; private set; } = MenuState.Home;
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
        var tileColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "road-tiles/Textures/colormap");
        _tileModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "road-tiles/road-straight"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            tileColormap
        );
    }

    public void Update(GameTime gameTime, TGCGame game)
    {
        var keyboardState = Keyboard.GetState();

        if (CurrentState == MenuState.Home)
        {
            if (keyboardState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
            {
                CurrentState = MenuState.SelectingVehicle;
            }
            if (keyboardState.IsKeyDown(Keys.D2) && _previousKeyboardState.IsKeyUp(Keys.D2))
            {
                CurrentState = MenuState.Controls;
            }
            if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                game.Exit();
            }
        }
        else if (CurrentState == MenuState.Controls)
        {
            if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                CurrentState = MenuState.Home;
            }
        }
        else if (CurrentState == MenuState.SelectingVehicle)
        {
            if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                CurrentState = MenuState.Home;
            }
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
            if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
            {
                CurrentState = MenuState.SelectingVehicle;
            }
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
        float height = game.GraphicsDevice.Viewport.Height;

        if (CurrentState == MenuState.Home)
        {
            game.DrawCenterTextY("DAKAR OVERFLOW", height * 0.15f, 3, Color.White);
            game.DrawCenterTextY("1. Jugar", height * 0.65f, 2, Color.LimeGreen);
            game.DrawCenterTextY("2. Comandos", height * 0.75f, 2, Color.Yellow);
            game.DrawCenterTextY("ESC - Salir", height * 0.85f, 2, Color.Red);

            Matrix scale = Matrix.CreateScale(0.1f);
            Matrix tileScale = Matrix.CreateScale(0.01f);
            Matrix rotation = Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds * 0.5f);
            Matrix centerTrans = Matrix.CreateTranslation(0f, -10f, 0f);
            
            // Dibujar el tile base (la calle)
            _tileModel.DrawUnlit(tileScale * centerTrans, cameraMenu.View, cameraMenu.Projection);
            // Dibujar un auto girando
            _lightModel.DrawUnlit(scale * rotation * centerTrans, cameraMenu.View, cameraMenu.Projection);
        }
        else if (CurrentState == MenuState.Controls)
        {
            game.DrawCenterTextY("COMANDOS", height * 0.15f, 3, Color.White);
            game.DrawCenterTextY("A S D: Moverse", height * 0.4f, 1, Color.White);
            game.DrawCenterTextY("G: God Mode", height * 0.5f, 1, Color.LightBlue);
            game.DrawCenterTextY("H: Hitboxes", height * 0.6f, 1, Color.LightCoral);
            game.DrawCenterTextY("F: Modo Foto", height * 0.7f, 1, Color.LightGreen);
            game.DrawCenterTextY("P: FullScreen ", height * 0.78f, 1, Color.LightCyan);

            game.DrawCenterTextY("ESC - Volver", height * 0.85f, 1, Color.Gray);
        }
        else if (CurrentState == MenuState.SelectingVehicle)
        {
            game.DrawCenterTextY("SELECCIONA TU VEHICULO", height * 0.15f, 2, Color.White);
            game.DrawCenterTextY("Presiona 1, 2 o 3", height * 0.25f, 1, Color.White);

            Matrix scale = Matrix.CreateScale(0.08f);
            Matrix rotation = Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds);
            
            Matrix transRight = Matrix.CreateTranslation(50f, -5f, 0f);
            Matrix transCenter = Matrix.CreateTranslation(0f, -5f, 0f);
            Matrix transLeft = Matrix.CreateTranslation(-50f, -5f, 0f);
            
            _lightModel.DrawUnlit(scale * rotation * transLeft, cameraMenu.View, cameraMenu.Projection);
            _mediumModel.DrawUnlit(scale * rotation * transCenter, cameraMenu.View, cameraMenu.Projection);
            _heavyModel.DrawUnlit(scale * rotation * transRight, cameraMenu.View, cameraMenu.Projection);

            Vector3 screenRight = game.GraphicsDevice.Viewport.Project(new Vector3(50f, -5f, 0f), cameraMenu.Projection, cameraMenu.View, Matrix.Identity);
            Vector3 screenCenter = game.GraphicsDevice.Viewport.Project(new Vector3(0f, -5f, 0f), cameraMenu.Projection, cameraMenu.View, Matrix.Identity);
            Vector3 screenLeft = game.GraphicsDevice.Viewport.Project(new Vector3(-50f, -5f, 0f), cameraMenu.Projection, cameraMenu.View, Matrix.Identity);

            float statY = screenCenter.Y + 80f;
            float lineSpacing = 25f;
            
            // Light
            game.DrawTextCenteredAtXY("1 - LIGERO", screenLeft.X, statY, 0.8f, Color.LightBlue);
            game.DrawTextCenteredAtXY($"Velocidad: {VehiclePresets.Light.MaxSpeed}", screenLeft.X, statY + lineSpacing, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Tanque de Nafta: {VehiclePresets.Light.FuelCapacity}", screenLeft.X, statY + lineSpacing * 2, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Vida: {VehiclePresets.Light.MaxHealth}", screenLeft.X, statY + lineSpacing * 3, 0.6f, Color.White);

            // Medium
            game.DrawTextCenteredAtXY("2 - MEDIANO", screenCenter.X, statY, 0.8f, Color.LightGreen);
            game.DrawTextCenteredAtXY($"Velocidad: {VehiclePresets.Medium.MaxSpeed}", screenCenter.X, statY + lineSpacing, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Tanque de Nafta: {VehiclePresets.Medium.FuelCapacity}", screenCenter.X, statY + lineSpacing * 2, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Vida: {VehiclePresets.Medium.MaxHealth}", screenCenter.X, statY + lineSpacing * 3, 0.6f, Color.White);

            // Heavy
            game.DrawTextCenteredAtXY("3 - PESADO", screenRight.X, statY, 0.8f, Color.LightCoral);
            game.DrawTextCenteredAtXY($"Velocidad: {VehiclePresets.Heavy.MaxSpeed}", screenRight.X, statY + lineSpacing, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Tanque de Nafta: {VehiclePresets.Heavy.FuelCapacity}", screenRight.X, statY + lineSpacing * 2, 0.6f, Color.White);
            game.DrawTextCenteredAtXY($"Vida: {VehiclePresets.Heavy.MaxHealth}", screenRight.X, statY + lineSpacing * 3, 0.6f, Color.White);
        }
        else if (CurrentState == MenuState.SelectingDifficulty)
        {
            game.DrawCenterTextY("DIFICULTAD", height * 0.15f, 3, Color.White);
            game.DrawCenterTextY("M para Facil", height * 0.4f, 1, Color.LimeGreen);
            game.DrawCenterTextY("N para Normal", height * 0.5f, 1, Color.Yellow);
            game.DrawCenterTextY("B para Dificil", height * 0.6f, 1, Color.Red);
            game.DrawCenterTextY("ESC - Volver", height * 0.8f, 1, Color.Gray);
        }
    }
}
