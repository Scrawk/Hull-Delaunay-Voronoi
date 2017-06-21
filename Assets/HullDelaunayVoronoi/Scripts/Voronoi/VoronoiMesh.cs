using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Voronoi
{

    public abstract class VoronoiMesh<VERTEX> : IVoronoiMesh<VERTEX>
        where VERTEX : class, IVertex, new()
    {

        public int Dimension { get; private set; }

        public IList<DelaunayCell<VERTEX>> Cells { get; private set; }

        public IList<VoronoiRegion<VERTEX>> Regions { get; private set; }

        public VoronoiMesh(int dimension)
        {

            Dimension = dimension;

            Cells = new List<DelaunayCell<VERTEX>>();

            Regions = new List<VoronoiRegion<VERTEX>>();

        }

        public virtual void Clear()
        {

            Cells.Clear();
            Regions.Clear();

        }

        public abstract void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false);

        protected void Generate(IList<VERTEX> input, IDelaunayTriangulation<VERTEX> delaunay, bool assignIds, bool checkInput = false)
        {

            Clear();

            delaunay.Generate(input, assignIds, checkInput);

            for (int i = 0; i < delaunay.Vertices.Count; i++)
            {
                delaunay.Vertices[i].Tag = i;
            }

            for (int i = 0; i < delaunay.Cells.Count; i++)
            {
                delaunay.Cells[i].CircumCenter.Id = i;
                delaunay.Cells[i].Simplex.Tag = i;
                Cells.Add(delaunay.Cells[i]);
            }

            List<DelaunayCell<VERTEX>> cells = new List<DelaunayCell<VERTEX>>();
            Dictionary<int, DelaunayCell<VERTEX>> neighbourCell = new Dictionary<int, DelaunayCell<VERTEX>>();

            for (int i = 0; i < delaunay.Vertices.Count; i++)
            {

                cells.Clear();

                VERTEX vertex = delaunay.Vertices[i];

                for (int j = 0; j < delaunay.Cells.Count; j++)
                {
                    Simplex<VERTEX> simplex = delaunay.Cells[j].Simplex;

                    for (int k = 0; k < simplex.Vertices.Length; k++)
                    {
                        if (simplex.Vertices[k].Tag == vertex.Tag)
                        {
                            cells.Add(delaunay.Cells[j]);
                            break;
                        }
                    }
                }

                if (cells.Count > 0)
                {
                    VoronoiRegion<VERTEX> region = new VoronoiRegion<VERTEX>();

                    for (int j = 0; j < cells.Count; j++)
                    {
                        region.Cells.Add(cells[j]);
                    }

                    neighbourCell.Clear();

                    for (int j = 0; j < cells.Count; j++)
                    {
                        neighbourCell.Add(cells[j].CircumCenter.Id, cells[j]);
                    }

                    for (int j = 0; j < cells.Count; j++)
                    {
                        Simplex<VERTEX> simplex = cells[j].Simplex;

                        for (int k = 0; k < simplex.Adjacent.Length; k++)
                        {
                            if (simplex.Adjacent[k] == null) continue;

                            int tag = simplex.Adjacent[k].Tag;

                            if (neighbourCell.ContainsKey(tag))
                            {
                                VoronoiEdge<VERTEX> edge = new VoronoiEdge<VERTEX>(cells[j], neighbourCell[tag]);
                                region.Edges.Add(edge);
                            }
                        }
                    }

                    region.Id = Regions.Count;
                    Regions.Add(region);
                }

            }

        }

    }

}












