using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{
    /// <summary>
    /// Holds all the objects needed to create a convex hull.
    /// Helps keep the Convex hull class clean and could maybe recyle them.
    /// </summary>
    internal class ObjectBuffer<VERTEX>
        where VERTEX : class, IVertex, new()
    {

        internal int CONNECTOR_TABLE_SIZE { get { return 2017; } }

        internal int Dimension { get; private set; }

        internal List<VERTEX> InputVertices { get; private set; }

        internal List<SimplexWrap<VERTEX>> ConvexSimplexs { get; private set; }

        internal VERTEX CurrentVertex { get; set; }

        internal ObjectManager<VERTEX> ObjectManager { get; private set; }

        internal float MaxDistance { get; set; }

        internal VERTEX FurthestVertex { get; set; }

        internal SimplexList<VERTEX> UnprocessedFaces { get; private set; }

        internal List<SimplexWrap<VERTEX>> AffectedFaceBuffer { get; private set; }

        internal Stack<SimplexWrap<VERTEX>> TraverseStack { get; private set; }

        internal HashSet<VERTEX> SingularVertices { get; private set; }

        internal List<DeferredSimplex<VERTEX>> ConeFaceBuffer { get; private set; }

        internal SimplexWrap<VERTEX>[] UpdateBuffer { get; private set; }

        internal int[] UpdateIndices { get; private set; }

        internal ConnectorList<VERTEX>[] ConnectorTable { get; private set; }

        internal VertexBuffer<VERTEX> EmptyBuffer { get; private set; }

        internal VertexBuffer<VERTEX> BeyondBuffer { get; set; }

        public ObjectBuffer(int dimension)
        {

            Dimension = dimension;

            ConvexSimplexs = new List<SimplexWrap<VERTEX>>();

            MaxDistance = float.NegativeInfinity;
            UnprocessedFaces = new SimplexList<VERTEX>();
            AffectedFaceBuffer = new List<SimplexWrap<VERTEX>>();
            TraverseStack = new Stack<SimplexWrap<VERTEX>>();
            SingularVertices = new HashSet<VERTEX>();
            ConeFaceBuffer = new List<DeferredSimplex<VERTEX>>();
            UpdateBuffer = new SimplexWrap<VERTEX>[Dimension];
            UpdateIndices = new int[Dimension];
            ObjectManager = new ObjectManager<VERTEX>(Dimension);
            EmptyBuffer = new VertexBuffer<VERTEX>();
            BeyondBuffer = new VertexBuffer<VERTEX>();

            ConnectorTable = new ConnectorList<VERTEX>[CONNECTOR_TABLE_SIZE];

            for (int i = 0; i < CONNECTOR_TABLE_SIZE; i++)
                ConnectorTable[i] = new ConnectorList<VERTEX>();

        }

        public void Clear()
        {

            UpdateBuffer = new SimplexWrap<VERTEX>[Dimension];
            UpdateIndices = new int[Dimension];

            InputVertices = null;
            CurrentVertex = null;
            FurthestVertex = null;
            MaxDistance = float.NegativeInfinity;

            ConvexSimplexs.Clear();
            AffectedFaceBuffer.Clear();
            TraverseStack.Clear();
            SingularVertices.Clear();
            ConeFaceBuffer.Clear();
            ObjectManager.Clear();
            UnprocessedFaces.Clear();
            EmptyBuffer.Clear();
            BeyondBuffer.Clear();

            for (int i = 0; i < CONNECTOR_TABLE_SIZE; i++)
                ConnectorTable[i].Clear();

        }

        public void AddInput(IList<VERTEX> input, bool assignIds, bool checkInput)
        {
            int count = input.Count;
            InputVertices = new List<VERTEX>(input);

            if (assignIds)
            {
                for (int i = 0; i < count; i++)
                    InputVertices[i].Id = i;
            }

            if (checkInput)
            {
                HashSet<int> set = new HashSet<int>();

                for (int i = 0; i < count; i++)
                {
                    if (input[i] == null)
                        throw new ArgumentException("Input has a null vertex");

                    if (input[i].Dimension != Dimension)
                        throw new ArgumentException("Input vertex is not the correct dimension", input[i].Dimension.ToString());

                    if (set.Contains(input[i].Id))
                        throw new ArgumentException("Input vertex id is not unique", input[i].Id.ToString());
                    else
                        set.Add(input[i].Id);
                }
            }

        }

    }
}
