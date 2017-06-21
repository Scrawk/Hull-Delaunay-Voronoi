using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;

namespace HullDelaunayVoronoi.Voronoi
{

    public class VoronoiRegion<VERTEX>
        where VERTEX : class, IVertex, new() 
    {

        public int Id { get; set; }

        public IList<DelaunayCell<VERTEX>> Cells { get; private set; }

        public IList<VoronoiEdge<VERTEX>> Edges { get; private set; }

        public VoronoiRegion()
        {

            Cells = new List<DelaunayCell<VERTEX>>();

            Edges = new List<VoronoiEdge<VERTEX>>();

        }

        public override string ToString()
        {
            return string.Format("[VoronoiRegion: Id={0}, Cells={1}, Edges={2}]", Id, Cells.Count, Edges.Count);
        }

    }

}
