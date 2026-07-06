namespace TGC.MonoGame.TP;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class BoundingVolumeFactory
{


    public static BoundingBox CreateWorldBoundingBox(WorldObject obj)
    {
        BoundingBox? mergedBox = null;

        foreach (ModelMesh mesh in obj.Model.Model.Meshes)
        {
            BoundingSphere worldSphere = mesh.BoundingSphere.Transform(obj.World);
            Vector3 min = worldSphere.Center - new Vector3(worldSphere.Radius);
            Vector3 max = worldSphere.Center + new Vector3(worldSphere.Radius);
            BoundingBox worldBox = new BoundingBox(min, max);

            mergedBox = mergedBox.HasValue
                ? BoundingBox.CreateMerged(mergedBox.Value, worldBox)
                : worldBox;
        }

        return mergedBox ?? new BoundingBox(obj.World.Translation, obj.World.Translation);
    }
}
