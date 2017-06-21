using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{

    /// <summary>
    /// A priority based linked list.
    /// </summary>
    internal class SimplexList<VERTEX>
        where VERTEX : IVertex, new() 
    {
        internal SimplexWrap<VERTEX> First { get; private set; }

        private SimplexWrap<VERTEX> Last { get; set; }


        internal void Clear()
        {

            First = null;
            Last = null;

        }

        /// <summary>
        /// Adds the element to the beginning.
        /// </summary>
        private void AddFirst(SimplexWrap<VERTEX> face)
        {
            face.InList = true;
            First.Previous = face;
            face.Next = First;
            First = face;
        }

        /// <summary>
        /// Adds a face to the list.
        /// </summary>
        internal void Add(SimplexWrap<VERTEX> face)
        {
            if (face.InList)
            {
                if (First.VerticesBeyond.Count < face.VerticesBeyond.Count)
                {
                    Remove(face);
                    AddFirst(face);
                }
                return;
            }

            face.InList = true;

            if (First != null && First.VerticesBeyond.Count < face.VerticesBeyond.Count)
            {
                First.Previous = face;
                face.Next = First;
                First = face;
            }
            else
            {
                if (Last != null)
                {
                    Last.Next = face;
                }

                face.Previous = Last;
                Last = face;

                if (First == null)
                {
                    First = face;
                }
            }
        }

        /// <summary>
        /// Removes the element from the list.
        /// </summary>
        internal void Remove(SimplexWrap<VERTEX> face)
        {
            if (!face.InList) return;

            face.InList = false;

            if (face.Previous != null)
            {
                face.Previous.Next = face.Next;
            }
            else if (face.Previous == null)
            {
                First = face.Next;
            }

            if (face.Next != null)
            {
                face.Next.Previous = face.Previous;
            }
            else if (face.Next == null)
            {
                Last = face.Previous;
            }

            face.Next = null;
            face.Previous = null;
        }
    }

}