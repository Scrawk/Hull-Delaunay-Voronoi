using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{

    internal class SimplexWrap<VERTEX>
        where VERTEX : IVertex, new() 
    {

        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        internal SimplexWrap<VERTEX>[] AdjacentFaces { get; private set; }

        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        internal VertexBuffer<VERTEX> VerticesBeyond { get; set; }

        /// <summary>
        /// The furthest vertex.
        /// </summary>
        internal VERTEX FurthestVertex { get; set; }

        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        internal VERTEX[] Vertices { get; set; }

        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        internal float[] Normal { get; private set; }

        /// <summary>
        /// Is the normal flipped?
        /// </summary>
        internal bool IsNormalFlipped { get; set; }

        /// <summary>
        /// Face plane constant element.
        /// </summary>
        internal float Offset { get; set; }

        /// <summary>
        /// Used to traverse affected faces.
        /// </summary>
        internal int Tag { get; set; }

        /// <summary>
        /// Prev node in the list.
        /// </summary>
        internal SimplexWrap<VERTEX> Previous { get; set; }

        /// <summary>
        /// Next node in the list.
        /// </summary>
        internal SimplexWrap<VERTEX> Next { get; set; }

        /// <summary>
        /// Is it present in the list.
        /// </summary>
        internal bool InList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal SimplexWrap(int dimension, VertexBuffer<VERTEX> beyondList)
        {
            AdjacentFaces = new SimplexWrap<VERTEX>[dimension];
            VerticesBeyond = beyondList;
            Normal = new float[dimension];
            Vertices = new VERTEX[dimension];
        }
    }

}