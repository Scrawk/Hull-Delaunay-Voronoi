using UnityEngine;
using System.Collections.Generic;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi
{

    public class ExampleDelaunay3D : MonoBehaviour
    {

        public int NumberOfVertices = 100;

        public float size = 5;

        public int seed = 0;

        private Material lineMaterial;

        private Matrix4x4 rotation = Matrix4x4.identity;

        private float theta;

        private DelaunayTriangulation3 delaunay;

        private void Start()
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            Vertex3[] vertices = new Vertex3[NumberOfVertices];

            Random.InitState(seed);
            for (int i = 0; i < NumberOfVertices; i++)
            {
                float x = size * Random.Range(-1.0f, 1.0f);
                float y = size * Random.Range(-1.0f, 1.0f);
                float z = size * Random.Range(-1.0f, 1.0f);

                vertices[i] = new Vertex3(x, y, z);
            }

            delaunay = new DelaunayTriangulation3();
            delaunay.Generate(vertices);

        }

        private void Update()
        {

            if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.KeypadMinus))
            {
                theta += (Input.GetKey(KeyCode.KeypadPlus)) ? 0.005f : -0.005f;

                rotation[0, 0] = Mathf.Cos(theta);
                rotation[0, 2] = Mathf.Sin(theta);
                rotation[2, 0] = -Mathf.Sin(theta);
                rotation[2, 2] = Mathf.Cos(theta);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, Screen.width, Screen.height), "Numpad +/- to Rotate");
        }

        private void OnPostRender()
        {

            if (delaunay == null || delaunay.Cells.Count == 0 || delaunay.Vertices.Count == 0) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix * rotation);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            foreach (DelaunayCell<Vertex3> cell in delaunay.Cells)
            {
                DrawSimplex(cell.Simplex);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawSimplex(Simplex<Vertex3> f)
        {
            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y,f.Vertices[1].Z);

            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);

            GL.Vertex3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
            GL.Vertex3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z);

            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);

            GL.Vertex3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z);
            GL.Vertex3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z);

            GL.Vertex3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z);
            GL.Vertex3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);


        }

    }

}



















