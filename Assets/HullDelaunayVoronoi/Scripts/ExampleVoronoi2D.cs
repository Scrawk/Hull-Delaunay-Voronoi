using UnityEngine;
using System.Collections.Generic;

using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi
{

    public class ExampleVoronoi2D : MonoBehaviour
    {

        public int NumberOfVertices = 1000;

        public float size = 10;

        public int seed = 0;

        private VoronoiMesh2 voronoi;

        private Material lineMaterial;

        private void Start()
        {

            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            List<Vertex2> vertices = new List<Vertex2>();

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);

                vertices.Add(new Vertex2(x, y));
            }

            voronoi = new VoronoiMesh2();
            voronoi.Generate(vertices);

        }

        private void OnPostRender()
        {

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            GL.Color(Color.red);

            foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
            {
                bool draw = true;

                foreach (DelaunayCell<Vertex2> cell in region.Cells)
                {
                    if (!InBound(cell.CircumCenter))
                    {
                        draw = false;
                        break;
                    }
                }

                if (!draw) continue;

                foreach (VoronoiEdge<Vertex2> edge in region.Edges)
                {
                    Vertex2 v0 = edge.From.CircumCenter;
                    Vertex2 v1 = edge.To.CircumCenter;

                    DrawLine(v0, v1);
                }
            }

            GL.End();
            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);

            foreach (DelaunayCell<Vertex2> cell in voronoi.Cells)
            {
                if (!InBound(cell.CircumCenter)) continue;

                DrawPoint(cell.CircumCenter);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawLine(Vertex2 v0, Vertex2 v1)
        {
            GL.Vertex3(v0.X, v0.Y, 0.0f);
            GL.Vertex3(v1.X, v1.Y, 0.0f);
        }

        private void DrawPoint(Vertex2 v)
        {
            float x = v.X;
            float y = v.Y;
            float s = 0.05f;

            GL.Vertex3(x + s, y + s, 0.0f);
            GL.Vertex3(x + s, y - s, 0.0f);
            GL.Vertex3(x - s, y - s, 0.0f);
            GL.Vertex3(x - s, y + s, 0.0f);
        }

        private bool InBound(Vertex2 v)
        {
            if (v.X < -size || v.X > size) return false;
            if (v.Y < -size || v.Y > size) return false;

            return true;
        }

    }

}



















