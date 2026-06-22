using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP;

public class Skybox
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Effect _effect;
    private readonly List<SkyboxFace> _faces;

    private struct SkyboxFace
    {
        public Texture2D Texture;
        public VertexPositionTexture[] Vertices;
    }

    public Skybox(GraphicsDevice graphicsDevice, Effect effect, Dictionary<string, Texture2D> textures, float size)
    {
        _graphicsDevice = graphicsDevice;
        _effect = effect;
        _faces = new List<SkyboxFace>();

        InitializeFaces(textures, size);
    }

    private void InitializeFaces(Dictionary<string, Texture2D> textures, float size)
    {
        // Front Face (Z = -size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["front"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(-size, size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, size, -size), new Vector2(1f, 0f)),
                new(new Vector3(size, -size, -size), new Vector2(1f, 1f)),

                new(new Vector3(-size, size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, -size, -size), new Vector2(1f, 1f)),
                new(new Vector3(-size, -size, -size), new Vector2(0f, 1f))
            }
        });

        // Back Face (Z = size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["back"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(-size, size, size), new Vector2(1f, 0f)),
                new(new Vector3(-size, -size, size), new Vector2(1f, 1f)),

                new(new Vector3(size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(-size, -size, size), new Vector2(1f, 1f)),
                new(new Vector3(size, -size, size), new Vector2(0f, 1f))
            }
        });

        // Left Face (X = -size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["left"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(-size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(-size, size, -size), new Vector2(1f, 0f)),
                new(new Vector3(-size, -size, -size), new Vector2(1f, 1f)),

                new(new Vector3(-size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(-size, -size, -size), new Vector2(1f, 1f)),
                new(new Vector3(-size, -size, size), new Vector2(0f, 1f))
            }
        });

        // Right Face (X = size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["right"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(size, size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, size, size), new Vector2(1f, 0f)),
                new(new Vector3(size, -size, size), new Vector2(1f, 1f)),

                new(new Vector3(size, size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, -size, size), new Vector2(1f, 1f)),
                new(new Vector3(size, -size, -size), new Vector2(0f, 1f))
            }
        });

        // Up Face (Y = size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["up"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(-size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(size, size, size), new Vector2(1f, 0f)),
                new(new Vector3(size, size, -size), new Vector2(1f, 1f)),

                new(new Vector3(-size, size, size), new Vector2(0f, 0f)),
                new(new Vector3(size, size, -size), new Vector2(1f, 1f)),
                new(new Vector3(-size, size, -size), new Vector2(0f, 1f))
            }
        });

        // Down Face (Y = -size)
        _faces.Add(new SkyboxFace
        {
            Texture = textures["down"],
            Vertices = new VertexPositionTexture[]
            {
                new(new Vector3(-size, -size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, -size, -size), new Vector2(1f, 0f)),
                new(new Vector3(size, -size, size), new Vector2(1f, 1f)),

                new(new Vector3(-size, -size, -size), new Vector2(0f, 0f)),
                new(new Vector3(size, -size, size), new Vector2(1f, 1f)),
                new(new Vector3(-size, -size, size), new Vector2(0f, 1f))
            }
        });
    }

    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
    {
        // Center the skybox around the camera
        Matrix world = Matrix.CreateTranslation(cameraPosition);

        // Save original states
        var originalDepthState = _graphicsDevice.DepthStencilState;
        var originalRasterizerState = _graphicsDevice.RasterizerState;

        // Disable writing to the depth buffer and culling
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        _effect.Parameters["World"]?.SetValue(world);
        _effect.Parameters["View"]?.SetValue(view);
        _effect.Parameters["Projection"]?.SetValue(projection);
        _effect.Parameters["WorldInverseTranspose"]?.SetValue(world);
        foreach (var face in _faces)
        {
            _effect.Parameters["ModelTexture"]?.SetValue(face.Texture);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, face.Vertices, 0, 2);
            }
        }

        // Restore original states
        _graphicsDevice.DepthStencilState = originalDepthState;
        _graphicsDevice.RasterizerState = originalRasterizerState;
    }
}
