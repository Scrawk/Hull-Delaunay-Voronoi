using UnityEngine;
using System.Collections.Generic;

using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi
{

    public class ExampleVoronoi3D : MonoBehaviour
    {

        public int NumberOfVertices = 100;

        public float size = 5;

        public int seed = 0;

        public bool drawLines;

        public Material material;

        private Material lineMaterial;

        private Matrix4x4 rotation = Matrix4x4.identity;

        private float theta;

        private VoronoiMesh3 voronoi;

        private List<Mesh> meshes;

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

            voronoi = new VoronoiMesh3();
            voronoi.Generate(vertices);

            RegionsToMeshes();

        }

        private void RegionsToMeshes()
        {

            meshes = new List<Mesh>();

            foreach (VoronoiRegion<Vertex3> region in voronoi.Regions)
            {
                bool draw = true;

                List<Vertex3> verts = new List<Vertex3>();

                foreach (DelaunayCell<Vertex3> cell in region.Cells)
                {
                    if (!InBound(cell.CircumCenter))
                    {
                        draw = false;
                        break;
                    }
                    else
                    {
                        verts.Add(cell.CircumCenter);
                    }
                }

                if (!draw) continue;

                //If you find the convex hull of the voronoi region it
                //can be used to make a triangle mesh.

                ConvexHull3 hull = new ConvexHull3();
                hull.Generate(verts, false);

                List<Vector3> positions = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<int> indices = new List<int>();

                for (int i = 0; i < hull.Simplexs.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector3 v = new Vector3();
                        v.x = hull.Simplexs[i].Vertices[j].X;
                        v.y = hull.Simplexs[i].Vertices[j].Y;
                        v.z = hull.Simplexs[i].Vertices[j].Z;

                        positions.Add(v);
                    }

                    Vector3 n = new Vector3();
                    n.x = hull.Simplexs[i].Normal[0];
                    n.y = hull.Simplexs[i].Normal[1];
                    n.z = hull.Simplexs[i].Normal[2];

                    if (hull.Simplexs[i].IsNormalFlipped)
                    {
                        indices.Add(i * 3 + 2);
                        indices.Add(i * 3 + 1);
                        indices.Add(i * 3 + 0);
                    }
                    else
                    {
                        indices.Add(i * 3 + 0);
                        indices.Add(i * 3 + 1);
                        indices.Add(i * 3 + 2);
                    }

                    normals.Add(n);
                    normals.Add(n);
                    normals.Add(n);
                }

                Mesh mesh = new Mesh();
                mesh.SetVertices(positions);
                mesh.SetNormals(normals);
                mesh.SetTriangles(indices, 0);

                mesh.RecalculateBounds();
                //mesh.RecalculateNormals();

                meshes.Add(mesh);

            }

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

            if (meshes != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();

                foreach (Mesh mesh in meshes)
                    Graphics.DrawMesh(mesh, rotation, material, 0, Camera.main, 0, block, true, true);
            }

        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, Screen.width, Screen.height), "Numpad +/- to Rotate");
        }

        private void OnPostRender()
        {
            if (!drawLines) return;

            if (voronoi == null || voronoi.Regions.Count == 0) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(GetComponent<Camera>().worldToCameraMatrix * rotation);
            GL.LoadProjectionMatrix(GetComponent<Camera>().projectionMatrix);

            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            foreach (VoronoiRegion<Vertex3> region in voronoi.Regions)
            {
                bool draw = true;

                foreach (DelaunayCell<Vertex3> cell in region.Cells)
                {
                    if (!InBound(cell.CircumCenter))
                    {
                        draw = false;
                        break;
                    }
                }

                if (!draw) continue;

                foreach (VoronoiEdge<Vertex3> edge in region.Edges)
                {
                    Vertex3 v0 = edge.From.CircumCenter;
                    Vertex3 v1 = edge.To.CircumCenter;

                    DrawLine(v0, v1);
                }
            }

            GL.End();
            GL.PopMatrix();
        }

        private void DrawLine(Vertex3 v0, Vertex3 v1)
        {
            GL.Vertex3(v0.X, v0.Y, v0.Z);
            GL.Vertex3(v1.X, v1.Y, v1.Z);
        }

        private bool InBound(Vertex3 v)
        {
            if (v.X < -size || v.X > size) return false;
            if (v.Y < -size || v.Y > size) return false;
            if (v.Z < -size || v.Z > size) return false;

            return true;
        }

    }

}



















