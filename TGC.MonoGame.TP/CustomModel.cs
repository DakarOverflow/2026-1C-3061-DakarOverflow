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

    public CustomModel(Model model, Effect effect, Color diffusionColor)
    {
        _model = model;
        _effect = effect;
        _diffusionColor = diffusionColor;

        foreach (var mesh in _model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }
    }

    public void Draw(Matrix world, Matrix view, Matrix projection)
    {
        _effect.Parameters["View"].SetValue(view);
        _effect.Parameters["Projection"].SetValue(projection);
        _effect.Parameters["DiffuseColor"].SetValue(_diffusionColor.ToVector3());

        var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

        foreach (var mesh in _model.Meshes)
        {
            // Obtain the world matrix for that mesh (relative to the parent).
            var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
            _effect.Parameters["World"].SetValue(meshWorld * world);
                // Draw the mesh.
                mesh.Draw();
            }
    }
}