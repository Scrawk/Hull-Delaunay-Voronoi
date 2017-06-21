using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{

    /// <summary>
    /// Used to effectively store vertices beyond.
    /// </summary>
    internal class VertexBuffer<VERTEX>
        where VERTEX : IVertex, new() 
    {

        private VERTEX[] items;

        private int count;

        private int capacity;

        /// <summary>
        /// Number of elements present in the buffer.
        /// </summary>
        internal int Count { get { return count; } }

        internal int Capacity {  get { return capacity;  } }

        internal VertexBuffer()
        {

        }

        internal VertexBuffer(int _capacity)
        {

            capacity = _capacity;
            items = new VERTEX[capacity];

        }

        /// <summary>
        /// Get the i-th element.
        /// </summary>
        internal VERTEX this[int i]
        {
            get { return items[i]; }
        }

        /// <summary>
        /// Size matters.
        /// </summary>
        private void EnsureCapacity()
        {
            if (count + 1 > capacity)
            {
                if (capacity == 0) capacity = 4;
                else capacity = 2 * capacity;
                Array.Resize(ref items, capacity);
            }
        }

        /// <summary>
        /// Adds a vertex to the buffer.
        /// </summary>
        internal void Add(VERTEX item)
        {
            EnsureCapacity();
            items[count++] = item;
        }

        /// <summary>
        /// Sets the Count to 0, otherwise does nothing.
        /// </summary>
        internal void Clear()
        {
            count = 0;
        }
    }

}