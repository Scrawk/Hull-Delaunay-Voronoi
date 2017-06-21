using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{

    /// <summary>
    /// A helper class used to connect faces.
    /// </summary>
    internal class SimplexConnector<VERTEX>
        where VERTEX : IVertex, new() 
    {
        /// <summary>
        /// The face.
        /// </summary>
        internal SimplexWrap<VERTEX> Face { get; set; }

        /// <summary>
        /// The edge to be connected.
        /// </summary>
        internal int EdgeIndex { get; private set; }

        /// <summary>
        /// The vertex indices.
        /// </summary>
        internal int[] Vertices { get; private set; }

        /// <summary>
        /// The hash code computed from indices.
        /// </summary>
        internal uint HashCode { get; private set; }

        /// <summary>
        /// Prev node in the list.
        /// </summary>
        internal SimplexConnector<VERTEX> Previous { get; set; }

        /// <summary>
        /// Next node in the list.
        /// </summary>
        internal SimplexConnector<VERTEX> Next { get; set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        internal SimplexConnector(int dimension)
        {
            Vertices = new int[dimension - 1];
        }

        /// <summary>
        /// Updates the connector.
        /// </summary>
        internal void Update(SimplexWrap<VERTEX> face, int edgeIndex, int dim)
        {
            Face = face;
            EdgeIndex = edgeIndex;

            uint hashCode = 31;

            var vs = face.Vertices;
            for (int i = 0, c = 0; i < dim; i++)
            {
                if (i != edgeIndex)
                {
                    int v = vs[i].Id;
                    Vertices[c++] = v;
                    hashCode += unchecked(23 * hashCode + (uint)v);
                }
            }

            HashCode = hashCode;
        }

        /// <summary>
        /// Can two faces be connected.
        /// </summary>
        internal static bool AreConnectable(SimplexConnector<VERTEX> a, SimplexConnector<VERTEX> b, int dim)
        {
            if (a.HashCode != b.HashCode) return false;

            var n = dim - 1;
            var av = a.Vertices;
            var bv = b.Vertices;
            for (int i = 0; i < n; i++)
            {
                if (av[i] != bv[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Connect two faces.
        /// </summary>
        internal static void Connect(SimplexConnector<VERTEX> a, SimplexConnector<VERTEX> b)
        {
            a.Face.AdjacentFaces[a.EdgeIndex] = b.Face;
            b.Face.AdjacentFaces[b.EdgeIndex] = a.Face;
        }
    }
}