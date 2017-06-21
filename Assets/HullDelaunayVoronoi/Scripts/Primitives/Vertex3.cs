using System;
using System.Collections;

namespace HullDelaunayVoronoi.Primitives
{

    public class Vertex3 : Vertex
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

        public Vertex3() : base(3)
        {

        }

        public Vertex3(int id) : base(3, id)
        {

        }

        public Vertex3(float x, float y, float z)
        {
            Position = new float[] { x, y, z };
        }

        public Vertex3(float x, float y, float z, int id)
        {
            Position = new float[] { x, y, z };
            Id = id;
        }

        public float Distance(float px, float py, float pz)
        {
            return (float)Math.Sqrt(SqrDistance(px, py, pz));
        }

        public float SqrDistance(float px, float py, float pz)
        {
            float x = Position[0] - px;
            float y = Position[1] - py;
            float z = Position[2] - pz;

            return x * x + y * y + z * z;
        }


    }
}
