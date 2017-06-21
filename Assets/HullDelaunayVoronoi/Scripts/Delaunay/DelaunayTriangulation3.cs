using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Delaunay
{

    public class DelaunayTriangulation3 : DelaunayTriangulation3<Vertex3>
    {

    }

    public class DelaunayTriangulation3<VERTEX> : DelaunayTriangulation<VERTEX>
        where VERTEX : class, IVertex, new()
    {

        private float[,] m_matrixBuffer;

        public DelaunayTriangulation3() : base(3)
        {

            m_matrixBuffer = new float[4, 4];

        }

        public override void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false)
        {

            Clear();
            if (input.Count <= Dimension + 1) return;

            int count = input.Count;
            for (int i = 0; i < count; i++)
            {
                float lenSq = input[i].SqrMagnitude;

                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension + 1);
                input[i].Position = v;

                input[i].Position[Dimension] = (float)lenSq;
            }

            var hull = new ConvexHull<VERTEX>(Dimension + 1);
            hull.Generate(input, assignIds, checkInput);

            for (int i = 0; i < count; i++)
            {
                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension);
                input[i].Position = v;
            }

            Vertices = new List<VERTEX>(hull.Vertices);

            Centroid.Position[0] = hull.Centroid[0];
            Centroid.Position[1] = hull.Centroid[1];
            Centroid.Position[2] = hull.Centroid[2];

            count = hull.Simplexs.Count;

            for (int i = 0; i < count; i++)
            {

                Simplex<VERTEX> simplex = hull.Simplexs[i];

                if (simplex.Normal[Dimension] >= 0.0f)
                {
                    for (int j = 0; j < simplex.Adjacent.Length; j++)
                    {
                        if (simplex.Adjacent[j] != null)
                        {
                            simplex.Adjacent[j].Remove(simplex);
                        }
                    }
                }
                else
                {
                    DelaunayCell<VERTEX> cell = CreateCell(simplex);
                    cell.CircumCenter.Id = i;
                    Cells.Add(cell);
                }

            }

        }

        private float MINOR(int r0, int r1, int r2, int c0, int c1, int c2)
        {
            return m_matrixBuffer[r0, c0] * (m_matrixBuffer[r1, c1] * m_matrixBuffer[r2, c2] - m_matrixBuffer[r2, c1] * m_matrixBuffer[r1, c2]) -
                    m_matrixBuffer[r0, c1] * (m_matrixBuffer[r1, c0] * m_matrixBuffer[r2, c2] - m_matrixBuffer[r2, c0] * m_matrixBuffer[r1, c2]) +
                    m_matrixBuffer[r0, c2] * (m_matrixBuffer[r1, c0] * m_matrixBuffer[r2, c1] - m_matrixBuffer[r2, c0] * m_matrixBuffer[r1, c1]);
        }

        private float Determinant()
        {
            return (m_matrixBuffer[0, 0] * MINOR(1, 2, 3, 1, 2, 3) -
                    m_matrixBuffer[0, 1] * MINOR(1, 2, 3, 0, 2, 3) +
                    m_matrixBuffer[0, 2] * MINOR(1, 2, 3, 0, 1, 3) -
                    m_matrixBuffer[0, 3] * MINOR(1, 2, 3, 0, 1, 2));
        }

        private DelaunayCell<VERTEX> CreateCell(Simplex<VERTEX> simplex)
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumsphere.html

            VERTEX[] verts = simplex.Vertices;

            // x, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 0] = verts[i].Position[0];
                m_matrixBuffer[i, 1] = verts[i].Position[1];
                m_matrixBuffer[i, 2] = verts[i].Position[2];
                m_matrixBuffer[i, 3] = 1;
            }
            float a = Determinant();

            // size, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 0] = verts[i].SqrMagnitude;
            }
            float dx = Determinant();

            // size, x, z, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 1] = verts[i].Position[0];
            }
            float dy = -Determinant();

            // size, x, y, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 2] = verts[i].Position[1];
            }
            float dz = Determinant();

            //size, x, y, z
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 3] = verts[i].Position[2];
            }
            float c = Determinant();

            float s = -1.0f / (2.0f * a);
            float radius = Math.Abs(s) * (float)Math.Sqrt(dx * dx + dy * dy + dz * dz - 4 * a * c);

            float[] circumCenter = new float[3];
            circumCenter[0] = s * dx;
            circumCenter[1] = s * dy;
            circumCenter[2] = s * dz;

            return new DelaunayCell<VERTEX>(simplex, circumCenter, radius);
        }

    }

}












