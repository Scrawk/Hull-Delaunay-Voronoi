using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{
    /// <summary>
    /// A helper class mostly for normal computation. If convex hulls are computed
    /// in higher dimensions, it might be a good idea to add a specific
    /// FindNormalVectorND function.
    /// </summary>
    internal static class MathHelper<VERTEX>
        where VERTEX : class, IVertex, new() 
    {

        private static float[] ntX = new float[4];
        private static float[] ntY = new float[4];
        private static float[] ntZ = new float[4];

        /// <summary>
        /// Squared length of the vector.
        /// </summary>
        internal static float LengthSquared(float[] x)
        {
            float norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                float t = x[i];
                norm += t * t;
            }
            return norm;
        }

        /// <summary>
        /// Subtracts vectors x and y and stores the result to target.
        /// </summary>
        internal static void SubtractFast(float[] x, float[] y, float[] target)
        {
            int d = x.Length;
            for (int i = 0; i < d; i++)
            {
                target[i] = x[i] - y[i];
            }
        }

        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        private static void FindNormalVector4D(VERTEX[] vertices, float[] normal)
        {
            SubtractFast(vertices[1].Position, vertices[0].Position, ntX);
            SubtractFast(vertices[2].Position, vertices[1].Position, ntY);
            SubtractFast(vertices[3].Position, vertices[2].Position, ntZ);

            var x = ntX;
            var y = ntY;
            var z = ntZ;

            // This was generated using Mathematica
            var nx = x[3] * (y[2] * z[1] - y[1] * z[2])
                   + x[2] * (y[1] * z[3] - y[3] * z[1])
                   + x[1] * (y[3] * z[2] - y[2] * z[3]);
            var ny = x[3] * (y[0] * z[2] - y[2] * z[0])
                   + x[2] * (y[3] * z[0] - y[0] * z[3])
                   + x[0] * (y[2] * z[3] - y[3] * z[2]);
            var nz = x[3] * (y[1] * z[0] - y[0] * z[1])
                   + x[1] * (y[0] * z[3] - y[3] * z[0])
                   + x[0] * (y[3] * z[1] - y[1] * z[3]);
            var nw = x[2] * (y[0] * z[1] - y[1] * z[0])
                   + x[1] * (y[2] * z[0] - y[0] * z[2])
                   + x[0] * (y[1] * z[2] - y[2] * z[1]);

            float norm = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);

            float f = 1.0f / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
            normal[3] = f * nw;
        }

        /// <summary>
        /// Finds 3D normal vector.
        /// </summary>
        private static void FindNormalVector3D(VERTEX[] vertices, float[] normal)
        {
            SubtractFast(vertices[1].Position, vertices[0].Position, ntX);
            SubtractFast(vertices[2].Position, vertices[1].Position, ntY);

            var x = ntX;
            var y = ntY;

            var nx = x[1] * y[2] - x[2] * y[1];
            var ny = x[2] * y[0] - x[0] * y[2];
            var nz = x[0] * y[1] - x[1] * y[0];

            float norm = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);

            float f = 1.0f / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
        }

        /// <summary>
        /// Finds 2D normal vector.
        /// </summary>
        private static void FindNormalVector2D(VERTEX[] vertices, float[] normal)
        {
            SubtractFast(vertices[1].Position, vertices[0].Position, ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            float norm = (float)Math.Sqrt(nx * nx + ny * ny);

            float f = 1.0f / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }

        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        internal static void FindNormalVector(VERTEX[] vertices, float[] normalData)
        {
            switch (vertices[0].Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); return;
                case 3: FindNormalVector3D(vertices, normalData); return;
                case 4: FindNormalVector4D(vertices, normalData); return;
            }
        }

        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is > Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        internal static float GetVertexDistance(VERTEX v, SimplexWrap<VERTEX> f)
        {
            float[] normal = f.Normal;
            float[] p = v.Position;
            float distance = f.Offset;
            for (int i = 0; i < v.Dimension; i++) distance += normal[i] * p[i];
            return distance;
        }

        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is > Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        internal static float GetVertexDistance(VERTEX v, Simplex<VERTEX> f)
        {
            float[] normal = f.Normal;
            float[] p = v.Position;
            float distance = f.Offset;
            for (int i = 0; i < v.Dimension; i++) distance += normal[i] * p[i];
            return distance;
        }
    }


}