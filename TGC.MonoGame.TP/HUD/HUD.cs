using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP;

public class HUD
{
    private CustomModel _fuelTankModel;
    private CustomModel _wrenchModel;
    private CustomModel _coinModel;

    private Matrix _worldFuelTank;
    private Matrix _worldWrench;
    private Matrix _worldCoin;

    private Texture2D _blankTexture;
    private SpriteFont _font;

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SpriteFont font, Texture2D blankTexture)
    {
        _font = font;
        _blankTexture = blankTexture;

        var survivalKitColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "survival-kit/Textures/colormap");
        var toyCarKitColormap = content.Load<Texture2D>(AssetPaths.ContentFolder3D + "toy-car-kit/Textures/colormap");

        _fuelTankModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "survival-kit/barrel"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            survivalKitColormap
        );

        _wrenchModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "survival-kit/tool-hammer"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            survivalKitColormap
        );

        _coinModel = new CustomModel(
            content.Load<Model>(AssetPaths.ContentFolder3D + "toy-car-kit/item-coin-gold"),
            content.Load<Effect>(AssetPaths.ContentFolderEffects + "TexturedShader"),
            toyCarKitColormap
        );
    }

    public void Initialize(int screenWidth, int screenHeight)
    {
        _worldFuelTank = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(-screenWidth / 17f, -screenHeight / 17f, 0f);
        _worldWrench = Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-screenWidth / 19f, -screenHeight / 17f, 0f);
        _worldCoin = Matrix.CreateScale(0.2f) * Matrix.CreateRotationZ(MathHelper.PiOver4) * Matrix.CreateTranslation(screenWidth / 20f, screenHeight / 19f, 0f);
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vehicle playerVehicle, CameraStc cameraMenu)
    {
        _fuelTankModel.DrawUnlit(_worldFuelTank, cameraMenu.View, cameraMenu.Projection);
        _wrenchModel.DrawUnlit(_worldWrench, cameraMenu.View, cameraMenu.Projection);
        _coinModel.DrawUnlit(_worldCoin, cameraMenu.View, cameraMenu.Projection);

        DrawFuelBar(spriteBatch, graphicsDevice, playerVehicle, cameraMenu);
        DrawHealthBar(spriteBatch, graphicsDevice, playerVehicle, cameraMenu);
        DrawScoreUI(spriteBatch, graphicsDevice, playerVehicle, cameraMenu);
        DrawSpeedometer(spriteBatch, graphicsDevice, playerVehicle);
    }

    private void DrawFuelBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vehicle playerVehicle, CameraStc cameraMenu)
    {
        float fuelPercentage = Math.Max(0, playerVehicle.CurrentFuel / playerVehicle.MaxFuel);
        int barWidth = 20;
        int barHeight = 150;
        
        Vector3 screenPos = graphicsDevice.Viewport.Project(Vector3.Zero, cameraMenu.Projection, cameraMenu.View, _worldFuelTank);
        
        int x = (int)screenPos.X - (barWidth / 2);
        int y = (int)screenPos.Y - barHeight - 50;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, barHeight), Color.DarkGray);
        
        Color fuelColor = fuelPercentage > 0.2f ? Color.Orange : Color.Red;
        int fillHeight = (int)(barHeight * fuelPercentage);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - fillHeight, barWidth, fillHeight), fuelColor);
        
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - 2, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, 2, barHeight), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x + barWidth - 2, y, 2, barHeight), Color.Black);
        
        spriteBatch.End();
    }

    private void DrawHealthBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vehicle playerVehicle, CameraStc cameraMenu)
    {
        float healthPercentage = Math.Max(0, playerVehicle.CurrentHealth / playerVehicle.MaxHealth);
        int barWidth = 20;
        int barHeight = 150;
        
        Vector3 screenPos = graphicsDevice.Viewport.Project(Vector3.Zero, cameraMenu.Projection, cameraMenu.View, _worldWrench);
        
        int x = (int)screenPos.X - (barWidth / 2);
        int y = (int)screenPos.Y - barHeight - 50;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, barHeight), Color.DarkGray);
        
        Color healthColor = healthPercentage > 0.3f ? Color.LimeGreen : Color.Red;
        int fillHeight = (int)(barHeight * healthPercentage);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - fillHeight, barWidth, fillHeight), healthColor);
        
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y + barHeight - 2, barWidth, 2), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x, y, 2, barHeight), Color.Black);
        spriteBatch.Draw(_blankTexture, new Rectangle(x + barWidth - 2, y, 2, barHeight), Color.Black);
        
        spriteBatch.End();
    }

    private void DrawScoreUI(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vehicle playerVehicle, CameraStc cameraMenu)
    {
        Vector3 screenPos = graphicsDevice.Viewport.Project(Vector3.Zero, cameraMenu.Projection, cameraMenu.View, _worldCoin);
        
        int x = (int)screenPos.X + 40;
        int y = (int)screenPos.Y - 20; 
        
        string scoreText = playerVehicle.Score.ToString();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        Vector2 position = new Vector2(x, y);
        spriteBatch.DrawString(_font, scoreText, position + new Vector2(2, 0), Color.Black);
        spriteBatch.DrawString(_font, scoreText, position + new Vector2(-2, 0), Color.Black);
        spriteBatch.DrawString(_font, scoreText, position + new Vector2(0, 2), Color.Black);
        spriteBatch.DrawString(_font, scoreText, position + new Vector2(0, -2), Color.Black);
        
        spriteBatch.DrawString(_font, scoreText, position, Color.White);
        
        spriteBatch.End();
    }

    private void DrawSpeedometer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vehicle playerVehicle)
    {
        int W = graphicsDevice.Viewport.Width;
        int H = graphicsDevice.Viewport.Height;
        
        Vector2 center = new Vector2(W - 150, H - 80);
        float radius = 100f;
        
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, DepthStencilState.Default, null, null, null);
        
        for (int i = 0; i <= 180; i += 2)
        {
            float a = MathHelper.ToRadians(180 + i);
            Vector2 tickPos = center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * (radius - 20);
            spriteBatch.Draw(_blankTexture, tickPos, null, new Color(0, 0, 0, 100), a, Vector2.Zero, new Vector2(20, 2), SpriteEffects.None, 0);
        }

        int maxSpeedDisplay = 200; 
        for (int i = 0; i <= maxSpeedDisplay; i += 20)
        {
            float a = MathHelper.ToRadians(180 + (180f * (i / (float)maxSpeedDisplay)));
            Vector2 tickPos = center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * radius;
            
            int tickLength = (i % 40 == 0) ? 15 : 8;
            int tickThickness = (i % 40 == 0) ? 3 : 1;
            Color tickColor = (i > 150) ? Color.Red : Color.White;
            
            spriteBatch.Draw(_blankTexture, tickPos, null, tickColor, a, new Vector2(1, 0.5f), new Vector2(tickLength, tickThickness), SpriteEffects.None, 0);
            
            if (i % 40 == 0)
            {
                Vector2 textPos = center + new Vector2((float)Math.Cos(a), (float)Math.Sin(a)) * (radius - 30);
                string markText = i.ToString();
                Vector2 textSize = _font.MeasureString(markText) * 0.3f;
                spriteBatch.DrawString(_font, markText, textPos - textSize / 2, Color.LightGray, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
            }
        }

        float speedMapped = Math.Min(Math.Max(Math.Abs(playerVehicle._speed) / 10f, 0), maxSpeedDisplay); 
        float needleAngle = MathHelper.ToRadians(180 + (180f * (speedMapped / maxSpeedDisplay)));
        
        spriteBatch.Draw(_blankTexture, center, null, Color.Red, needleAngle, new Vector2(0, 0.5f), new Vector2(radius - 10, 4), SpriteEffects.None, 0);
        
        spriteBatch.Draw(_blankTexture, new Rectangle((int)center.X - 8, (int)center.Y - 8, 16, 16), Color.DarkRed);

        string speedText = ((int)speedMapped).ToString() + " km/h";
        Vector2 speedTextSize = _font.MeasureString(speedText) * 0.5f;
        
        spriteBatch.DrawString(_font, speedText, center + new Vector2(-speedTextSize.X / 2 + 1, 21), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
        spriteBatch.DrawString(_font, speedText, center + new Vector2(-speedTextSize.X / 2, 20), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

        spriteBatch.End();
    }
}
