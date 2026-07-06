namespace TGC.MonoGame.TP;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class BoundingVolumeFactory
{
    public static BoundingSphere CreateWorldBoundingSphere(WorldObject obj)
    {
        BoundingSphere? mergedSphere = null;

        foreach (ModelMesh mesh in obj.Model.Model.Meshes)
        {
            BoundingSphere worldSphere = mesh.BoundingSphere.Transform(obj.World);
            mergedSphere = mergedSphere.HasValue
                ? BoundingSphere.CreateMerged(mergedSphere.Value, worldSphere)
                : worldSphere;
        }

        return mergedSphere ?? new BoundingSphere(obj.World.Translation, 0f);
    }

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
