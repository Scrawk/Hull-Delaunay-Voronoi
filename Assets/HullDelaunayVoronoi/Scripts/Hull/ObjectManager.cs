using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{

    /// <summary>
    /// A helper class for object allocation/storage. 
    /// This helps the GC a lot as it prevents the creation of about 75% of 
    /// new face objects (in the case of SimplexWrap). In the case of
    /// SimplexConnectors and DefferedFaces, the difference is even higher (in most
    /// cases O(1) vs O(number of created faces)). 
    /// </summary>
    internal class ObjectManager<VERTEX>
        where VERTEX : IVertex, new() 
    {
        readonly int Dimension;

        private Stack<SimplexWrap<VERTEX>> RecycledFaceStack;
        private Stack<SimplexConnector<VERTEX>> ConnectorStack;
        private Stack<VertexBuffer<VERTEX>> EmptyBufferStack;
        private Stack<DeferredSimplex<VERTEX>> DeferredSimplexStack;

        internal ObjectManager(int dimension)
        {
            Dimension = dimension;

            RecycledFaceStack = new Stack<SimplexWrap<VERTEX>>();
            ConnectorStack = new Stack<SimplexConnector<VERTEX>>();
            EmptyBufferStack = new Stack<VertexBuffer<VERTEX>>();
            DeferredSimplexStack = new Stack<DeferredSimplex<VERTEX>>();
        }

        internal void Clear()
        {

            RecycledFaceStack.Clear();
            ConnectorStack.Clear();
            EmptyBufferStack.Clear();
            DeferredSimplexStack.Clear();

        }

        internal void DepositFace(SimplexWrap<VERTEX> face)
        {

            face.Previous = null;
            face.Next = null;

            for (int i = 0; i < Dimension; i++)
            {
                face.AdjacentFaces[i] = null;
            }
            RecycledFaceStack.Push(face);
        }

        internal SimplexWrap<VERTEX> GetFace()
        {
            return RecycledFaceStack.Count != 0
                    ? RecycledFaceStack.Pop()
                    : new SimplexWrap<VERTEX>(Dimension, GetVertexBuffer());
        }

        internal void DepositConnector(SimplexConnector<VERTEX> connector)
        {
            connector.Face = null;
            connector.Previous = null;
            connector.Next = null;
            ConnectorStack.Push(connector);
        }

        internal SimplexConnector<VERTEX> GetConnector()
        {
            return ConnectorStack.Count != 0
                    ? ConnectorStack.Pop()
                    : new SimplexConnector<VERTEX>(Dimension);
        }

        internal void DepositVertexBuffer(VertexBuffer<VERTEX> buffer)
        {
            buffer.Clear();
            EmptyBufferStack.Push(buffer);
        }

        internal VertexBuffer<VERTEX> GetVertexBuffer()
        {
            return EmptyBufferStack.Count != 0 ? EmptyBufferStack.Pop() : new VertexBuffer<VERTEX>();
        }

        internal void DepositDeferredSimplex(DeferredSimplex<VERTEX> face)
        {
            face.Face = null;
            face.Pivot = null;
            face.OldFace = null;
            DeferredSimplexStack.Push(face);
        }

        internal DeferredSimplex<VERTEX> GetDeferredSimplex()
        {
            return DeferredSimplexStack.Count != 0 ? DeferredSimplexStack.Pop() : new DeferredSimplex<VERTEX>();
        }

    }

}