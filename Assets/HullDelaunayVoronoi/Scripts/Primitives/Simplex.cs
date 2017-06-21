using System;
using System.Collections.Generic;

namespace HullDelaunayVoronoi.Primitives
{
	public class Simplex<VERTEX>
        where VERTEX : IVertex, new() 
	{
		
		public int Dimension{ get; private set; }

        /// <summary>
        /// The simplexs adjacent to this simplex
        /// For 2D a simplex will be a segment and it with have two adjacent segments joining it.
        /// For 3D a simplex will be a triangle and it with have three adjacent triangles joining it.
        /// </summary>
		public Simplex<VERTEX>[] Adjacent { get; private set; }
		
		/// <summary>
        /// The vertices that make up the simplex.
		/// For 2D a face will be 2 vertices making a line.
		/// For 3D a face will be 3 vertices making a triangle.
		/// </summary>
		public VERTEX[] Vertices { get; private set; }
		
		/// <summary>
        /// The simplexs normal.
		/// </summary>
		public float[] Normal { get; private set; }
		
		/// <summary>
        /// The simplexs centroid.
		/// </summary>
		public float[] Centroid { get; private set; }
		
		/// <summary>
        /// The simplexs offset from the origin.
		/// </summary>
		public float Offset { get; set; }

		public int Tag { get; set; }

        public bool IsNormalFlipped { get; set; }

        public Simplex(int dimension)
		{
			
			if(dimension < 2 || dimension > 4)
                throw new ArgumentException("Invalid number of dimension for Simplex", dimension.ToString());
			
			Dimension = dimension;
            Adjacent = new Simplex<VERTEX>[dimension];
            Normal = new float[dimension];
            Centroid = new float[dimension];
            Vertices = new VERTEX[dimension];
			
		}
		
		public float Dot(VERTEX v)
		{
			int d = Dimension;
			if(v == null || v.Dimension != d)
				return 0.0f;
			
			float dp = 0.0f;
			
			for(int i = 0; i < d; i++)
				dp += Normal[i] * v.Position[i];
			
			return dp;
			
		}

        public bool Remove(Simplex<VERTEX> simplex)
		{

			if(simplex == null) return false;
			int n = Adjacent.Length;
			
			for(int i = 0; i < n; i++)
			{
				if(Adjacent[i] == null) continue;
				
                if (Object.ReferenceEquals(Adjacent[i], simplex))
			    {
				    Adjacent[i] = null;
				    return true;
			    }
			}
			
			return false;
		}
		
		public void CalculateNormal()
		{
			
			switch(Dimension)
			{
			case 2:
				CalculateNormal2D();
				break;
				
			case 3:
				CalculateNormal3D();
				break;
				
			case 4:
				CalculateNormal4D();
				break;
				
			default:
				throw new ArgumentException("Invalid number of dimension for Simplex", Dimension.ToString());
			}
			
		}

        private void CalculateNormal2D()
		{
			
			float[] ntX = new float[2];
			Subtract(Vertices[0].Position, Vertices[1].Position, ntX);
			
			float nx = -ntX[1];
			float ny = ntX[0];

            float norm = (float)Math.Sqrt(nx * nx + ny * ny);
			
			float f = 1.0f / norm;
			Normal[0] = f * nx;
			Normal[1] = f * ny;
			
		}

        private void CalculateNormal3D()
		{
			
			float[] ntX = new float[3];
			float[] ntY = new float[3];
			Subtract(Vertices[1].Position, Vertices[0].Position, ntX);
			Subtract(Vertices[2].Position, Vertices[1].Position, ntY);
			
			float nx = ntX[1] * ntY[2] - ntX[2] * ntY[1];
			float ny = ntX[2] * ntY[0] - ntX[0] * ntY[2];
			float nz = ntX[0] * ntY[1] - ntX[1] * ntY[0];

            float norm = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
			
			float f = 1.0f / norm;
			Normal[0] = f * nx;
			Normal[1] = f * ny;
			Normal[2] = f * nz;
			
		}

        private void CalculateNormal4D()
		{
			float[] ntX = new float[4];
			float[] ntY = new float[4];
			float[] ntZ = new float[4];
			Subtract(Vertices[1].Position, Vertices[0].Position, ntX);
			Subtract(Vertices[2].Position, Vertices[1].Position, ntY);
			Subtract(Vertices[3].Position, Vertices[2].Position, ntZ);
			
			var x = ntX;
			var y = ntY;
			var z = ntZ;
			
			// This was generated using Mathematica
			var nx = x[3] * (y[2] * z[1] - y[1] * z[2])
					+ x[2] * (y[1] * z[3] - y[3] * z[1])
					+ x[1] * (y[3] * z[2] - y[2] * z[3]);
			var ny = x[3] * (y[0] * z[2] - y[2] * z[0])
					+ x[2] * (y[3] * z[0] - y[0] * z[3])
					+ x[0] * (y[2] * z[3] - y[3] * z[2]);
			var nz = x[3] * (y[1] * z[0] - y[0] * z[1])
					+ x[1] * (y[0] * z[3] - y[3] * z[0])
					+ x[0] * (y[3] * z[1] - y[1] * z[3]);
			var nw = x[2] * (y[0] * z[1] - y[1] * z[0])
					+ x[1] * (y[2] * z[0] - y[0] * z[2])
					+ x[0] * (y[1] * z[2] - y[2] * z[1]);
			
			float norm = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);
			
			float f = 1.0f / norm;
			Normal[0] = f * nx;
			Normal[1] = f * ny;
			Normal[2] = f * nz;
			Normal[3] = f * nw;

		}
		
		private void Subtract(float[] x, float[] y, float[] target)
		{
			for (int i = 0; i < Dimension; i++)
			{
				target[i] = x[i] - y[i];
			}
		}
		
		public void CalculateCentroid()
		{ 

			switch(Dimension)
			{
			case 2:
				CalculateCentroid2D();
				break;
				
			case 3:
				CalculateCentroid3D();
				break;
				
			case 4:
				CalculateCentroid4D();
				break;
				
			default:
				throw new ArgumentException("Invalid number of dimension for Simplex", Dimension.ToString());
			}

		}

        private void CalculateCentroid2D()
		{
			
			Centroid[0] = (Vertices[0].Position[0] + Vertices[1].Position[0]) / 2.0f;
			Centroid[1] = (Vertices[0].Position[1] + Vertices[1].Position[1]) / 2.0f;
			
		}

        private void CalculateCentroid3D()
		{
			
			Centroid[0] = (Vertices[0].Position[0] + Vertices[1].Position[0] + Vertices[2].Position[0]) / 3.0f;
			Centroid[1] = (Vertices[0].Position[1] + Vertices[1].Position[1] + Vertices[2].Position[1]) / 3.0f;
			Centroid[2] = (Vertices[0].Position[2] + Vertices[1].Position[2] + Vertices[2].Position[2]) / 3.0f;
			
		}

        private void CalculateCentroid4D()
		{
			
			Centroid[0] = (Vertices[0].Position[0] + Vertices[1].Position[0] + Vertices[2].Position[0] + Vertices[3].Position[0]) / 4.0f;
			Centroid[1] = (Vertices[0].Position[1] + Vertices[1].Position[1] + Vertices[2].Position[1] + Vertices[3].Position[1]) / 4.0f;
			Centroid[2] = (Vertices[0].Position[2] + Vertices[1].Position[2] + Vertices[2].Position[2] + Vertices[3].Position[2]) / 4.0f;
			Centroid[3] = (Vertices[0].Position[3] + Vertices[1].Position[3] + Vertices[2].Position[3] + Vertices[3].Position[3]) / 4.0f;
			
		}

        public void UpdateAdjacency(Simplex<VERTEX> simplex)
		{

            VERTEX[] lv = Vertices;
            VERTEX[] rv = simplex.Vertices;
			
			int i;
			int d = Dimension;
			
			// reset marks on the 1st face
			for (i = 0; i < d; i++) lv[i].Tag = 0;
			
			// mark all vertices on the 2nd face
			for (i = 0; i < d; i++) rv[i].Tag = 1;
			
			// find the 1st false index
			for (i = 0; i < d; i++) if (lv[i].Tag == 0) break;
			
			// no vertex was marked
			if (i == d) return;
			
			// check if only 1 vertex wasn't marked
			for (int j = i + 1; j < d; j++) if (lv[j].Tag == 0) return;
			
			// if we are here, the two faces share an edge
            Adjacent[i] = simplex;
			
			// update the adj. face on the other face - find the vertex that remains marked
			for (i = 0; i < d; i++) lv[i].Tag = 0;
			
			for (i = 0; i < d; i++)
			{
				if (rv[i].Tag == 1)
				{
                    simplex.Adjacent[i] = this;
					break;
				}
			}
			
		}
		
		public bool HasNullAdjacency()
		{
			
			int d = Dimension;
			for(int i = 0; i < d; i++)
				if(Adjacent[i] == null) return true;
			
			return false;
		}

		public bool HasAdjacency()
		{
			
			int d = Dimension;
			for(int i = 0; i < d; i++)
				if(Adjacent[i] != null) return true;
			
			return false;
		}
		
		public override string ToString ()
		{
			
			string indexs = "";
			
			int d = Dimension;
			for(int i = 0; i < d; i++)
			{
				indexs += Vertices[i].Id;
				if(i !=d-1) indexs += ",";
			}
			
			return string.Format ("[Simplex: Dimension={0},  Vertices={1}]", Dimension, indexs);
		}
	}
	
}

















