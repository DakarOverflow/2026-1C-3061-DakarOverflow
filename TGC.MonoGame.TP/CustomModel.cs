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

    private Color _diffusionColor;
    private Texture2D _texture;

    public CustomModel(Model model, Effect effect, Color diffusionColor)
    {
        _model = model;
        _effect = effect;
        _diffusionColor = diffusionColor;
        _texture = null;

        ApplyEffectToMeshParts();
    }

    public CustomModel(Model model, Effect effect, Texture2D texture)
    {
        _model = model;
        _effect = effect;
        _texture = texture;
        _diffusionColor = Color.White;

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
        _effect.Parameters["View"]?.SetValue(view);
        _effect.Parameters["Projection"]?.SetValue(projection);

        if (_texture != null)
        {
            _effect.Parameters["ModelTexture"]?.SetValue(_texture);
        }
        else
        {
            _effect.Parameters["DiffuseColor"]?.SetValue(_diffusionColor.ToVector3());
        }

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        foreach (var mesh in _model.Meshes)
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