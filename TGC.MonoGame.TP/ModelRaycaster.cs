namespace TGC.MonoGame.TP;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public readonly struct ModelRaycastHit
{
    public readonly WorldObject Object;
    public readonly Vector3 LocalPoint;
    public readonly Vector3 WorldPoint;
    public readonly float Distance;

    public ModelRaycastHit(WorldObject obj, Vector3 localPoint, Vector3 worldPoint, float distance)
    {
        Object = obj;
        LocalPoint = localPoint;
        WorldPoint = worldPoint;
        Distance = distance;
    }
}

public static class ModelRaycaster
{
    private static readonly Dictionary<Model, List<Triangle>> LocalTriangleCache = new();

    public static Ray CreateRayFromScreenPoint(Point screenPoint, GraphicsDevice graphicsDevice, Camera camera)
    {
        Vector3 nearPoint = graphicsDevice.Viewport.Unproject(
            new Vector3(screenPoint.X, screenPoint.Y, 0f),
            camera.GetProjection(),
            camera.GetView(),
            Matrix.Identity
        );

        Vector3 farPoint = graphicsDevice.Viewport.Unproject(
            new Vector3(screenPoint.X, screenPoint.Y, 1f),
            camera.GetProjection(),
            camera.GetView(),
            Matrix.Identity
        );

        return new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
    }

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

    public static bool TryIntersectObject(Ray worldRay, WorldObject obj, out ModelRaycastHit hit)
    {
        hit = default;

        if (!CreateWorldBoundingSphere(obj).Intersects(worldRay).HasValue)
        {
            return false;
        }

        Matrix inverseWorld = Matrix.Invert(obj.World);
        Ray localRay = TransformRay(worldRay, inverseWorld);
        float? closestDistance = null;
        Vector3 closestLocalPoint = Vector3.Zero;

        foreach (Triangle triangle in GetCachedLocalTriangles(obj.Model.Model))
        {
            float? distance = IntersectsTriangle(localRay, triangle);

            if (distance.HasValue && (!closestDistance.HasValue || distance.Value < closestDistance.Value))
            {
                closestDistance = distance.Value;
                closestLocalPoint = localRay.Position + localRay.Direction * distance.Value;
            }
        }

        if (!closestDistance.HasValue)
        {
            return false;
        }

        Vector3 worldPoint = Vector3.Transform(closestLocalPoint, obj.World);
        float worldDistance = Vector3.Distance(worldRay.Position, worldPoint);
        hit = new ModelRaycastHit(obj, closestLocalPoint, worldPoint, worldDistance);
        return true;
    }

    private static List<Triangle> GetCachedLocalTriangles(Model model)
    {
        if (!LocalTriangleCache.TryGetValue(model, out var triangles))
        {
            triangles = ModelMeshExtractor.GetLocalTrianglesFromModel(model);
            LocalTriangleCache.Add(model, triangles);
        }

        return triangles;
    }

    private static Ray TransformRay(Ray ray, Matrix transform)
    {
        Vector3 position = Vector3.Transform(ray.Position, transform);
        Vector3 direction = Vector3.Normalize(Vector3.TransformNormal(ray.Direction, transform));
        return new Ray(position, direction);
    }

    private static float? IntersectsTriangle(Ray ray, Triangle triangle)
    {
        const float epsilon = 0.000001f;

        Vector3 edge1 = triangle.B - triangle.A;
        Vector3 edge2 = triangle.C - triangle.A;
        Vector3 p = Vector3.Cross(ray.Direction, edge2);
        float determinant = Vector3.Dot(edge1, p);

        if (determinant > -epsilon && determinant < epsilon)
        {
            return null;
        }

        float inverseDeterminant = 1f / determinant;
        Vector3 t = ray.Position - triangle.A;
        float u = Vector3.Dot(t, p) * inverseDeterminant;

        if (u < 0f || u > 1f)
        {
            return null;
        }

        Vector3 q = Vector3.Cross(t, edge1);
        float v = Vector3.Dot(ray.Direction, q) * inverseDeterminant;

        if (v < 0f || u + v > 1f)
        {
            return null;
        }

        float distance = Vector3.Dot(edge2, q) * inverseDeterminant;
        return distance >= 0f ? distance : null;
    }
}
