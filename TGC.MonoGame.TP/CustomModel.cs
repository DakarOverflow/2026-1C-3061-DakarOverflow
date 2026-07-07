using System;
using System.Collections.Generic;
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
    private bool _shadowDiagnosticsLogged;


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

    public void Draw(Matrix world, Matrix view, Matrix projection)
    {
        DrawInternal(world, view, projection, true);
    }

    public void DrawUnlit(Matrix world, Matrix view, Matrix projection)
    {
        DrawInternal(world, view, projection, false);
    }

    public void DrawMany(IEnumerable<Matrix> worlds, Matrix view, Matrix projection)
    {
        DrawManyInternal(worlds, view, projection, true);
    }

    public void DrawShadow(Matrix world, Effect shadowEffect, Matrix lightViewProjection)
    {
        DrawManyShadow(new[] { world }, shadowEffect, lightViewProjection);
    }

    public void DrawManyShadow(IEnumerable<Matrix> worlds, Effect shadowEffect, Matrix lightViewProjection, ShadowDiagnostics diagnostics = null)
    {
        shadowEffect.Parameters["LightViewProjection"]?.SetValue(lightViewProjection);
        shadowEffect.CurrentTechnique = shadowEffect.Techniques["ShadowMapDrawing"];

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        var graphicsDevice = _model.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice;
        var meshPartCount = 0;
        var primitiveCount = 0;
        foreach (var mesh in _model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPartCount++;
                primitiveCount += meshPart.PrimitiveCount;
            }
        }
        diagnostics?.RecordModel(_model.Meshes.Count, meshPartCount, primitiveCount);

        foreach (var instanceWorld in worlds)
        {
            foreach (var mesh in _model.Meshes)
            {
                var finalWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index] * instanceWorld;
                shadowEffect.Parameters["World"]?.SetValue(finalWorld);

                foreach (var meshPart in mesh.MeshParts)
                {
                    if (meshPart.PrimitiveCount == 0) continue;

                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    foreach (var pass in shadowEffect.CurrentTechnique.Passes)
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
    }

    private void DrawInternal(Matrix world, Matrix view, Matrix projection, bool useLighting)
    {
        DrawManyInternal(new[] { world }, view, projection, useLighting);
    }

    private void DrawManyInternal(IEnumerable<Matrix> worlds, Matrix view, Matrix projection, bool useLighting)
    {
        // 1. Configurar los parámetros globales de la cámara
        _effect.Parameters["View"]?.SetValue(view);
        _effect.Parameters["CameraPosition"]?.SetValue(Matrix.Invert(view).Translation);
        _effect.Parameters["Projection"]?.SetValue(projection);

        ShadowMapping.ApplyTo(_effect);
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
        if (!_shadowDiagnosticsLogged)
        {
            _shadowDiagnosticsLogged = true;
            ShadowMapping.Diagnostics?.LogMainEffect(_effect, _model.Root?.Name ?? "model", activeTechnique?.Name);
        }

        if (activeTechnique != null)
        {
            _effect.CurrentTechnique = activeTechnique;
        }

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        var graphicsDevice = _model.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice;

        // 3. Renderizar las mallas usando el pipeline manual de pases del shader
        foreach (var instanceWorld in worlds)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                var finalWorld = meshWorld * instanceWorld;

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
    }
}