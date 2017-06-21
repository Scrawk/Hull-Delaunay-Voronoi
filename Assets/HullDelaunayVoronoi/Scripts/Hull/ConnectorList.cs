using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{
    /// <summary>
    /// Connector list.
    /// </summary>
    internal class ConnectorList<VERTEX>
        where VERTEX : IVertex, new()
    {
        private SimplexConnector<VERTEX> first, last;

        /// <summary>
        /// Get the first element.
        /// </summary>
        internal SimplexConnector<VERTEX> First { get { return first; } }

        internal void Clear()
        {
            first = null;
            last = null;
        }

        /// <summary>
        /// Adds the element to the beginning.
        /// </summary>
        void AddFirst(SimplexConnector<VERTEX> connector)
        {
            this.first.Previous = connector;
            connector.Next = this.first;
            this.first = connector;
        }

        /// <summary>
        /// Adds a face to the list.
        /// </summary>
        internal void Add(SimplexConnector<VERTEX> element)
        {
            if (this.last != null)
            {
                this.last.Next = element;
            }
            element.Previous = this.last;
            this.last = element;
            if (this.first == null)
            {
                this.first = element;
            }
        }

        /// <summary>
        /// Removes the element from the list.
        /// </summary>
        internal void Remove(SimplexConnector<VERTEX> connector)
        {
            if (connector.Previous != null)
            {
                connector.Previous.Next = connector.Next;
            }
            else if (connector.Previous == null)
            {
                this.first = connector.Next;
            }

            if (connector.Next != null)
            {
                connector.Next.Previous = connector.Previous;
            }
            else if (connector.Next == null)
            {
                this.last = connector.Previous;
            }

            connector.Next = null;
            connector.Previous = null;
        }
    }
}