using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class CustomModel
{
    private Effect _effect;
    public Model InnerModel { get; private set; }

    private Color _diffusionColor;
    private Texture2D _texture;

    private Texture2D _overlayTexture;

    public CustomModel(Model model, Effect effect, Color diffusionColor)
    {
        InnerModel = model;
        _effect = effect;
        _diffusionColor = diffusionColor;
        _texture = null;
        _overlayTexture = null;

        ApplyEffectToMeshParts();
    }

    public CustomModel(Model model, Effect effect, Texture2D texture)
    {
        InnerModel = model;
        _effect = effect;
        _texture = texture;
        _diffusionColor = Color.White;
        _overlayTexture = null;

        ApplyEffectToMeshParts();
    }

        public CustomModel(Model model, Effect effect, Texture2D texture, Texture2D overlayTexture)
    {
        InnerModel = model;
        _effect = effect;
        _texture = texture;
        _diffusionColor = Color.White;
        _overlayTexture = overlayTexture;

        ApplyEffectToMeshParts();
    }

    private void ApplyEffectToMeshParts()
    {
        foreach (var mesh in InnerModel.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }
    }

    public void Draw(Matrix world, Matrix view, Matrix projection)
    {
        _effect.Parameters["View"]?.SetValue(view);
        _effect.Parameters["Projection"]?.SetValue(projection);

        if (_texture != null)
        {
            _effect.Parameters["ModelTexture"]?.SetValue(_texture);
            _effect.Parameters["UseOverlay"]?.SetValue(_overlayTexture != null);
        }
        else
        {
            _effect.Parameters["DiffuseColor"]?.SetValue(_diffusionColor.ToVector3());
        }

        if(_overlayTexture != null)
        {
            _effect.Parameters["OverlayTexture"]?.SetValue(_overlayTexture);
        }

        var modelMeshesBaseTransforms = new Matrix[InnerModel.Bones.Count];
        InnerModel.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        foreach (var mesh in InnerModel.Meshes)
        {
            var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
            _effect.Parameters["World"]?.SetValue(meshWorld * world);
            try
            {
                mesh.Draw();
            }
            catch (System.Exception ex)
            {
                System.IO.File.WriteAllText("crash.log", ex.ToString());
                throw;
            }
        }
    }
}