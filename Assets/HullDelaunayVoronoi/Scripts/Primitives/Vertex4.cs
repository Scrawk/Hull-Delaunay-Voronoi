using System;
using System.Collections;

namespace HullDelaunayVoronoi.Primitives
{

    public class Vertex4 : Vertex
    {

        public float X
        {
            get { return Position[0]; }
            set { Position[0] = value; }
        }

        public float Y
        {
            get { return Position[1]; }
            set { Position[1] = value; }
        }

        public float Z
        {
            get { return Position[2]; }
            set { Position[2] = value; }
        }

        public float W
        {
            get { return Position[3]; }
            set { Position[3] = value; }
        }

        public Vertex4() : base(4)
        {

        }

        public Vertex4(int id) : base(4, id)
        {

        }

        public Vertex4(float x, float y, float z, float w)
        {
            Position = new float[] { x, y, z, w };
        }

        public Vertex4(float x, float y, float z, float w, int id)
        {
            Position = new float[] { x, y, z, w };
            Id = id;
        }

        public float Distance(float px, float py, float pz, float pw)
        {
            return (float)Math.Sqrt(SqrDistance(px, py, pz, pw));
        }

        public float SqrDistance(float px, float py, float pz, float pw)
        {
            float x = Position[0] - px;
            float y = Position[1] - py;
            float z = Position[2] - pz;
            float w = Position[3] - pw;

            return x * x + y * y + z * z + w * w;
        }


    }
}
