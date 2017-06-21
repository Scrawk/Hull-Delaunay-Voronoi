using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;

namespace HullDelaunayVoronoi.Voronoi
{

    public class VoronoiEdge<VERTEX> : IEquatable<VoronoiEdge<VERTEX>>
        where VERTEX : class, IVertex, new() 
	{

        public DelaunayCell<VERTEX> From { get; private set; }

        public DelaunayCell<VERTEX> To { get; private set; }

        public VoronoiEdge(DelaunayCell<VERTEX> from, DelaunayCell<VERTEX> to)
		{

			From = from;
			To = to;

		}

		/// <summary>
		/// Are these keys equal.
		/// </summary>
        public static bool operator ==(VoronoiEdge<VERTEX> k1, VoronoiEdge<VERTEX> k2)
		{
			
			// If both are null, or both are same instance, return true.
			if (Object.ReferenceEquals(k1, k2))
			{
				return true;
			}
			
			// If one is null, but not both, return false.
			if (((object)k1 == null) || ((object)k2 == null))
			{
				return false;
			}

			return object.ReferenceEquals(k1.From, k2.To);
		}
		
		/// <summary>
		/// Are these keys not equal.
		/// </summary>
		public static bool operator !=(VoronoiEdge<VERTEX> k1, VoronoiEdge<VERTEX> k2)
		{
			return !(k1 == k2);
		}
		
		/// <summary>
		/// Is the key equal to another key.
		/// </summary>
		public override bool Equals(object o)
		{
            VoronoiEdge<VERTEX> k = o as VoronoiEdge<VERTEX>;
			return k != null && k == this;
		}
		
		/// <summary>
		/// Is the key equal to another key.
		/// </summary>
		public bool Equals(VoronoiEdge<VERTEX> k)
		{
			return k == this;
		}
		
		/// <summary>
		/// The keys hash code.
		/// </summary>
		public override int GetHashCode()
		{
			int hashcode = 23;

			hashcode = (hashcode * 37) + From.GetHashCode();
			hashcode = (hashcode * 37) + To.GetHashCode();
			
			return hashcode;
		}
		
	}
	
}
