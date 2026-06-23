using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class CustomModel
{
    private Effect _effect;
    private Model _model;

    public Model Model => _model;

    private Color _diffusionColor;
    private Texture2D _texture;

    private Texture2D _overlayTexture;

    public CustomModel(Model model, Effect effect, Color diffusionColor)
    {
        _model = model;
        _effect = effect;
        _diffusionColor = diffusionColor;
        _texture = null;
        _overlayTexture = null;

        ApplyEffectToMeshParts();
    }

    public CustomModel(Model model, Effect effect, Texture2D texture)
    {
        _model = model;
        _effect = effect;
        _texture = texture;
        _diffusionColor = Color.White;
        _overlayTexture = null;

        ApplyEffectToMeshParts();
    }

        public CustomModel(Model model, Effect effect, Texture2D texture, Texture2D overlayTexture)
    {
        _model = model;
        _effect = effect;
        _texture = texture;
        _diffusionColor = Color.White;
        _overlayTexture = overlayTexture;

        ApplyEffectToMeshParts();
    }

    private void ApplyEffectToMeshParts()
    {
        foreach (var mesh in _model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }
    }

    public void SetShadowMap(Texture2D shadowMap, Matrix lightViewProjection)
    {
        _effect.Parameters["ShadowMap"]?.SetValue(shadowMap);
        _effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjection);
    }

    public void DrawDepth(Matrix world, Matrix lightViewProjection)
    {
        EffectTechnique shadowMapTechnique = _effect.Techniques["ShadowMap"];
        if (shadowMapTechnique == null) return;

        _effect.CurrentTechnique = shadowMapTechnique;
        _effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjection);

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        var graphicsDevice = _model.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice;

        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index] * world;
            _effect.Parameters["World"]?.SetValue(meshWorld);

            // FIX: Dibujado manual explícito por partes para asegurar que la técnica NO se sobrescriba
            foreach (var meshPart in mesh.MeshParts)
            {
                if (meshPart.PrimitiveCount == 0) continue;

                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;

                foreach (var pass in shadowMapTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        meshPart.VertexOffset,
                        0,
                        meshPart.NumVertices,
                        meshPart.StartIndex,
                        meshPart.PrimitiveCount
                    );
                }
            }
        }
    }

    public void Draw(Matrix world, Matrix view, Matrix projection)
    {
        DrawInternal(world, view, projection, true);
    }

    public void DrawUnlit(Matrix world, Matrix view, Matrix projection)
    {
        DrawInternal(world, view, projection, false);
    }

    private void DrawInternal(Matrix world, Matrix view, Matrix projection, bool useLighting)
    {
        // 1. Configurar los parámetros globales de la cámara
        _effect.Parameters["View"]?.SetValue(view);
        _effect.Parameters["CameraPosition"]?.SetValue(Matrix.Invert(view).Translation);
        _effect.Parameters["Projection"]?.SetValue(projection);

        _effect.Parameters["UseLighting"]?.SetValue(useLighting);

        // 2. Determinar y asignar la técnica correcta basándonos en la textura
        EffectTechnique activeTechnique;
        if (_texture != null)
        {
            _effect.Parameters["ModelTexture"]?.SetValue(_texture);
            _effect.Parameters["UseOverlay"]?.SetValue(_overlayTexture != null);
            activeTechnique = useLighting
                ? _effect.Techniques["TexturedDrawing"]
                : _effect.Techniques["TexturedUnlitDrawing"] ?? _effect.Techniques["TexturedDrawing"];
        }
        else
        {
            _effect.Parameters["DiffuseColor"]?.SetValue(_diffusionColor.ToVector3());
            activeTechnique = _effect.Techniques["BasicColorDrawing"] ?? _effect.Techniques["TexturedDrawing"];
        }

        if(_overlayTexture != null)
        {
            _effect.Parameters["OverlayTexture"]?.SetValue(_overlayTexture);
        }

        if (activeTechnique != null)
        {
            _effect.CurrentTechnique = activeTechnique;
        }

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        var graphicsDevice = _model.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice;

        // 3. Renderizar las mallas usando el pipeline manual de pases del shader
        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
            var finalWorld = meshWorld * world;

            _effect.Parameters["World"]?.SetValue(finalWorld);

            var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(finalWorld));
            _effect.Parameters["WorldInverseTranspose"]?.SetValue(worldInverseTranspose);

            foreach (var meshPart in mesh.MeshParts)
            {
                if (meshPart.PrimitiveCount == 0) continue;

                // Enlazar los buffers de la geometría a la GPU
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;

                // Aplicar cada pase definido en tu técnica (.fx) de forma estricta
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply(); // <--- Esto congela los parámetros del shader (incluyendo el ShadowMap) para esta parte

                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        meshPart.VertexOffset,
                        0,
                        meshPart.NumVertices,
                        meshPart.StartIndex,
                        meshPart.PrimitiveCount
                    );
                }
            }
        }
    }
}