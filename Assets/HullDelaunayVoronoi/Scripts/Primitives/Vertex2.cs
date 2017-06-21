using System;
using System.Collections;

namespace HullDelaunayVoronoi.Primitives
{

    public class Vertex2 : Vertex
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

        public Vertex2() : base(2)
        {

        }

        public Vertex2(int id) : base(2, id)
        {

        }

        public Vertex2(float x, float y)
        {
            Position = new float[] { x, y };
        }

        public Vertex2(float x, float y, int id)
        {
            Position = new float[] { x, y };
            Id = id;
        }

        public float Distance(float px, float py)
        {
            return (float)Math.Sqrt(SqrDistance(px, py));
        }

        public float SqrDistance(float px, float py)
        {
            float x = Position[0] - px;
            float y = Position[1] - py;

            return x * x + y * y;
        }


    }
}
