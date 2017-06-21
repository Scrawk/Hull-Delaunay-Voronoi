using UnityEngine;
using System.Collections.Generic;

using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi
{

    public class ExampleConvexHull2D : MonoBehaviour
    {

        public int NumberOfVertices = 1000;

        public float size = 5;

        public int seed = 0;

        private Material lineMaterial;

        private ConvexHull2 hull;

        private List<Vertex2> vertices;

        private void Start()
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            vertices = new List<Vertex2>();

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);

                vertices.Add(new Vertex2(x, y));
            }

            hull = new ConvexHull2();
            hull.Generate(vertices);

        }

        private void OnPostRender()
        {

            if (hull == null || hull.Simplexs.Count == 0 || hull.Vertices.Count == 0) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            foreach (Simplex<Vertex2> f in hull.Simplexs)
            {
                DrawLine(f.Vertices[0], f.Vertices[1]);
            }

            GL.End();
            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);

            foreach (Vertex2 v in vertices)
            {
                DrawPoint(v);
            }

            GL.Color(Color.green);

            foreach (Vertex2 v in hull.Vertices)
            {
                DrawPoint(v);
            }

            GL.End();
            GL.PopMatrix();
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

        private void DrawLine(Vertex2 v0, Vertex2 v1)
        {
            GL.Vertex3(v0.X, v0.Y, 0.0f);
            GL.Vertex3(v1.X, v1.Y, 0.0f);
        }
    }

}



















