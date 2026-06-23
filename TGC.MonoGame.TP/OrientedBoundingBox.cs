using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public struct OrientedBoundingBox
    {
        public Vector3 Center;
        public Vector3 Extents;
        public Matrix Orientation;

        public OrientedBoundingBox(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
            Orientation = Matrix.Identity;
        }

        public OrientedBoundingBox(Vector3 center, Vector3 extents, Matrix orientation)
        {
            Center = center;
            Extents = extents;
            Orientation = orientation;
        }

        public OrientedBoundingBox(BoundingBox aabb)
        {
            Center = (aabb.Min + aabb.Max) * 0.5f;
            Extents = (aabb.Max - aabb.Min) * 0.5f;
            Orientation = Matrix.Identity;
        }

        public void Transform(Matrix transform)
        {
            Center = Vector3.Transform(Center, transform);
            
            // Extract the rotation scale part
            Matrix rotationTransform = transform;
            rotationTransform.Translation = Vector3.Zero;
            
            Orientation *= rotationTransform;
        }

        public Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];
            Vector3 xAxis = Orientation.Right * Extents.X;
            Vector3 yAxis = Orientation.Up * Extents.Y;
            Vector3 zAxis = Orientation.Backward * Extents.Z;

            corners[0] = Center - xAxis - yAxis - zAxis;
            corners[1] = Center + xAxis - yAxis - zAxis;
            corners[2] = Center + xAxis - yAxis + zAxis;
            corners[3] = Center - xAxis - yAxis + zAxis;
            corners[4] = Center - xAxis + yAxis - zAxis;
            corners[5] = Center + xAxis + yAxis - zAxis;
            corners[6] = Center + xAxis + yAxis + zAxis;
            corners[7] = Center - xAxis + yAxis + zAxis;

            return corners;
        }

        public bool Intersects(BoundingBox aabb)
        {
            return Intersects(new OrientedBoundingBox(aabb));
        }

        public bool Intersects(OrientedBoundingBox other)
        {
            // Separating Axis Theorem (SAT)
            Vector3[] axesA = { Orientation.Right, Orientation.Up, Orientation.Backward };
            Vector3[] axesB = { other.Orientation.Right, other.Orientation.Up, other.Orientation.Backward };

            // Normalize axes (in case orientation matrix contains scale)
            for(int i=0; i<3; i++)
            {
                if(axesA[i].LengthSquared() > 0.0001f) axesA[i].Normalize();
                if(axesB[i].LengthSquared() > 0.0001f) axesB[i].Normalize();
            }

            Vector3 t = other.Center - Center;

            // Test 15 separating axes
            // 3 axes of A
            for (int i = 0; i < 3; i++)
            {
                if (IsSeparatingAxis(t, axesA[i], this, other)) return false;
            }

            // 3 axes of B
            for (int i = 0; i < 3; i++)
            {
                if (IsSeparatingAxis(t, axesB[i], this, other)) return false;
            }

            // 9 cross products of axes A and B
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 axis = Vector3.Cross(axesA[i], axesB[j]);
                    if (axis.LengthSquared() > 0.0001f)
                    {
                        axis.Normalize();
                        if (IsSeparatingAxis(t, axis, this, other)) return false;
                    }
                }
            }

            return true;
        }

        private static bool IsSeparatingAxis(Vector3 t, Vector3 axis, OrientedBoundingBox a, OrientedBoundingBox b)
        {
            float rA = a.Extents.X * Math.Abs(Vector3.Dot(a.Orientation.Right, axis)) +
                       a.Extents.Y * Math.Abs(Vector3.Dot(a.Orientation.Up, axis)) +
                       a.Extents.Z * Math.Abs(Vector3.Dot(a.Orientation.Backward, axis));

            float rB = b.Extents.X * Math.Abs(Vector3.Dot(b.Orientation.Right, axis)) +
                       b.Extents.Y * Math.Abs(Vector3.Dot(b.Orientation.Up, axis)) +
                       b.Extents.Z * Math.Abs(Vector3.Dot(b.Orientation.Backward, axis));

            return Math.Abs(Vector3.Dot(t, axis)) > (rA + rB);
        }
    }
}
