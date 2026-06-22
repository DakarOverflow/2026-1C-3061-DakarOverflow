namespace TGC.MonoGame.TP;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public struct Triangle
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
    }
}

public static class ModelMeshExtractor
{
    public static List<Triangle> GetTrianglesFromModel(Model model, Matrix world)
    {
        var triangles = GetLocalTrianglesFromModel(model);

        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i] = new Triangle(
                Vector3.Transform(triangles[i].A, world),
                Vector3.Transform(triangles[i].B, world),
                Vector3.Transform(triangles[i].C, world)
            );
        }

        return triangles;
    }

    public static List<Triangle> GetLocalTrianglesFromModel(Model model)
    {
        var triangles = new List<Triangle>();
        
        // Del modelo, no del mundo
        Matrix[] boneTransforms = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(boneTransforms);

        foreach (ModelMesh mesh in model.Meshes)
        {
            Matrix meshTransform = boneTransforms[mesh.ParentBone.Index];

            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                VertexBuffer vertexBuffer = part.VertexBuffer;
                IndexBuffer indexBuffer = part.IndexBuffer;

                int vertexStride = vertexBuffer.VertexDeclaration.VertexStride;
                int positionOffset = GetPositionOffset(vertexBuffer.VertexDeclaration);

                if (positionOffset < 0)
                    throw new Exception("No existe la posicion.");

                // Extraer la posicion
                Vector3[] vertices = new Vector3[vertexBuffer.VertexCount];

                vertexBuffer.GetData(
                    positionOffset,
                    vertices,
                    0,
                    vertexBuffer.VertexCount,
                    vertexStride
                );

                int[] indices = GetIndices(indexBuffer);

                for (int i = 0; i < part.PrimitiveCount; i++)
                {
                    int indexPosition = part.StartIndex + i * 3;

                    int i0 = indices[indexPosition + 0] + part.VertexOffset;
                    int i1 = indices[indexPosition + 1] + part.VertexOffset;
                    int i2 = indices[indexPosition + 2] + part.VertexOffset;

                    Vector3 a = Vector3.Transform(vertices[i0], meshTransform);
                    Vector3 b = Vector3.Transform(vertices[i1], meshTransform);
                    Vector3 c = Vector3.Transform(vertices[i2], meshTransform);

                    triangles.Add(new Triangle(a, b, c));
                }
            }
        }

        return triangles;
    }

    private static int GetPositionOffset(VertexDeclaration vertexDeclaration)
    {
        foreach (VertexElement element in vertexDeclaration.GetVertexElements())
        {
            if (element.VertexElementUsage == VertexElementUsage.Position)
                return element.Offset;
        }

        return -1;
    }

    private static int[] GetIndices(IndexBuffer indexBuffer)
    {
        if (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
        {
            ushort[] rawIndices = new ushort[indexBuffer.IndexCount];
            indexBuffer.GetData(rawIndices);

            int[] indices = new int[rawIndices.Length];

            for (int i = 0; i < rawIndices.Length; i++)
                indices[i] = rawIndices[i];

            return indices;
        }
        else
        {
            int[] indices = new int[indexBuffer.IndexCount];
            indexBuffer.GetData(indices);
            return indices;
        }
    }
}
