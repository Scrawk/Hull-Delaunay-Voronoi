using System;
using System.Collections.Generic;
using System.Linq;

using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi.Hull
{
    public class SingularInputException : Exception
    {
        public SingularInputException(string msg) : base(msg) { }
    }

    public class ConvexHull2 : ConvexHull<Vertex2>
    {
        public ConvexHull2() : base(2) { }
    }

    public class ConvexHull3 : ConvexHull<Vertex3>
    {
        public ConvexHull3() : base(3) { }
    }

    public class ConvexHull4 : ConvexHull<Vertex4>
    {
        public ConvexHull4() : base(4) { }
    }

    public class ConvexHull<VERTEX> : IConvexHull<VERTEX>
        where VERTEX : class, IVertex, new()
    {

        private const float PLANE_DISTANCE_TOLERANCE = 1e-7f;

        public int Dimension { get; private set; }

        public IList<VERTEX> Vertices { get; private set; }

        public IList<Simplex<VERTEX>> Simplexs { get; private set; }

        public float[] Centroid { get; private set; }

        private ObjectBuffer<VERTEX> Buffer { get; set; }

        public ConvexHull(int dimension)
        {

            Dimension = dimension;

            Centroid = new float[Dimension];
            Vertices = new List<VERTEX>();
            Simplexs = new List<Simplex<VERTEX>>();

        }

        public bool Contains(VERTEX vertex)
        {
            int count = Simplexs.Count;
            for (int i = 0; i < count; i++)
            {
                if (MathHelper<VERTEX>.GetVertexDistance(vertex, Simplexs[i]) >= PLANE_DISTANCE_TOLERANCE) return false;
            }

            return true;
        }

        #region Generate

        public void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false)
        {

            Clear();

            Buffer = new ObjectBuffer<VERTEX>(Dimension);

            int inputCount = input.Count;
            if (inputCount < Dimension + 1) return;

            Buffer.AddInput(input, assignIds, checkInput);

            InitConvexHull();

            // Expand the convex hull and faces.
            while (Buffer.UnprocessedFaces.First != null)
            {
                SimplexWrap<VERTEX> currentFace = Buffer.UnprocessedFaces.First;
                Buffer.CurrentVertex = currentFace.FurthestVertex;

                UpdateCenter();

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                if (!Buffer.SingularVertices.Contains(Buffer.CurrentVertex) && CreateCone())
                    CommitCone();
                else
                    HandleSingular();

                // Need to reset the tags
                int count = Buffer.AffectedFaceBuffer.Count;
                for (int i = 0; i < count; i++)
                    Buffer.AffectedFaceBuffer[i].Tag = 0;
            }

            for (int i = 0; i < Buffer.ConvexSimplexs.Count; i++)
            {
                SimplexWrap<VERTEX> wrap = Buffer.ConvexSimplexs[i];
                wrap.Tag = i;

                Simplexs.Add(new Simplex<VERTEX>(Dimension));
            }

            for (int i = 0; i < Buffer.ConvexSimplexs.Count; i++)
            {

                SimplexWrap<VERTEX> wrap = Buffer.ConvexSimplexs[i];
                Simplex<VERTEX> simplex = Simplexs[i];

                simplex.IsNormalFlipped = wrap.IsNormalFlipped;
                simplex.Offset = wrap.Offset;

                for (int j = 0; j < Dimension; j++)
                {
                    simplex.Normal[j] = wrap.Normal[j];
                    simplex.Vertices[j] = wrap.Vertices[j];

                    if (wrap.AdjacentFaces[j] != null)
                        simplex.Adjacent[j] = Simplexs[wrap.AdjacentFaces[j].Tag];
                    else
                        simplex.Adjacent[j] = null;
                }

                simplex.CalculateCentroid();
            }

            Buffer.Clear();
            Buffer = null;

        }

        public void Clear()
        {
            Centroid = new float[Dimension];
            Simplexs.Clear();
            Vertices.Clear();
        }

        #endregion

        #region Initilization

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// </summary>
        private void InitConvexHull()
        {
            List<VERTEX> extremes = FindExtremes();
            List<VERTEX> initialPoints = FindInitialPoints(extremes);

            int numPoints = initialPoints.Count;

            // Add the initial points to the convex hull.
            for (int i = 0; i < numPoints; i++)
            {
                Buffer.CurrentVertex = initialPoints[i];
                // update center must be called before adding the vertex.
                UpdateCenter();
                Vertices.Add(Buffer.CurrentVertex);
                Buffer.InputVertices.Remove(initialPoints[i]);

                // Because of the AklTou heuristic.
                extremes.Remove(initialPoints[i]);
            }

            // Create the initial simplexes.
            SimplexWrap<VERTEX>[] faces = InitiateFaceDatabase();

            int numFaces = faces.Length;

            // Init the vertex beyond buffers.
            for (int i = 0; i < numFaces; i++)
            {
                FindBeyondVertices(faces[i]);
                if (faces[i].VerticesBeyond.Count == 0)
                    Buffer.ConvexSimplexs.Add(faces[i]); // The face is on the hull
                else
                    Buffer.UnprocessedFaces.Add(faces[i]);
            }
        }

        /// <summary>
        /// Finds the extremes in all dimensions.
        /// </summary>
        private List<VERTEX> FindExtremes()
        {
            List<VERTEX> extremes = new List<VERTEX>(2 * Dimension);

            int vCount = Buffer.InputVertices.Count;
            for (int i = 0; i < Dimension; i++)
            {
                float min = float.MaxValue, max = float.MinValue;
                int minInd = 0, maxInd = 0;

                for (int j = 0; j < vCount; j++)
                {
                    float v = Buffer.InputVertices[j].Position[i];

                    if (v < min)
                    {
                        min = v;
                        minInd = j;
                    }
                    if (v > max)
                    {
                        max = v;
                        maxInd = j;
                    }
                }

                if (minInd != maxInd)
                {
                    extremes.Add(Buffer.InputVertices[minInd]);
                    extremes.Add(Buffer.InputVertices[maxInd]);
                }
                else extremes.Add(Buffer.InputVertices[minInd]);
            }

            return extremes;
        }

        /// <summary>
        /// Computes the sum of square distances to the initial points.
        /// </summary>
        private float GetSquaredDistanceSum(VERTEX pivot, List<VERTEX> initialPoints)
        {
            int initPtsNum = initialPoints.Count;
            float sum = 0.0f;

            for (int i = 0; i < initPtsNum; i++)
            {
                VERTEX initPt = initialPoints[i];

                for (int j = 0; j < Dimension; j++)
                {
                    float t = (initPt.Position[j] - pivot.Position[j]);
                    sum += t * t;
                }
            }

            return sum;
        }

        /// <summary>
        /// Finds (dimension + 1) initial points.
        /// </summary>
        private List<VERTEX> FindInitialPoints(List<VERTEX> extremes)
        {
            List<VERTEX> initialPoints = new List<VERTEX>();

            VERTEX first = null, second = null;
            float maxDist = 0.0f;
            float[] temp = new float[Dimension];

            for (int i = 0; i < extremes.Count - 1; i++)
            {
                VERTEX a = extremes[i];
                for (int j = i + 1; j < extremes.Count; j++)
                {
                    VERTEX b = extremes[j];

                    MathHelper<VERTEX>.SubtractFast(a.Position, b.Position, temp);

                    float dist = MathHelper<VERTEX>.LengthSquared(temp);

                    if (dist > maxDist)
                    {
                        first = a;
                        second = b;
                        maxDist = dist;
                    }
                }
            }

            initialPoints.Add(first);
            initialPoints.Add(second);

            for (int i = 2; i <= Dimension; i++)
            {
                float maximum = 0.000001f;
                VERTEX maxPoint = default(VERTEX);

                for (int j = 0; j < extremes.Count; j++)
                {
                    VERTEX extreme = extremes[j];
                    if (initialPoints.Contains(extreme)) continue;

                    float val = GetSquaredDistanceSum(extreme, initialPoints);

                    if (val > maximum)
                    {
                        maximum = val;
                        maxPoint = extreme;
                    }
                }

                if (maxPoint != null)
                {
                    initialPoints.Add(maxPoint);
                }
                else
                {
                    int vCount = Buffer.InputVertices.Count;
                    for (int j = 0; j < vCount; j++)
                    {
                        VERTEX point = Buffer.InputVertices[j];
                        if (initialPoints.Contains(point)) continue;

                        float val = GetSquaredDistanceSum(point, initialPoints);

                        if (val > maximum)
                        {
                            maximum = val;
                            maxPoint = point;
                        }
                    }

                    if (maxPoint != null)
                        initialPoints.Add(maxPoint);
                    else
                        new SingularInputException("Singular input data error");
                }
            }

            return initialPoints;
        }

        /// <summary>
        /// Create the first faces from (dimension + 1) vertices.
        /// </summary>
        private SimplexWrap<VERTEX>[] InitiateFaceDatabase()
        {
            SimplexWrap<VERTEX>[] faces = new SimplexWrap<VERTEX>[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++)
            {
                var vertices = Vertices.Where((_, j) => i != j).ToArray(); // Skips the i-th vertex
                var newFace = new SimplexWrap<VERTEX>(Dimension, new VertexBuffer<VERTEX>());

                newFace.Vertices = vertices;
                Array.Sort(vertices, new VertexIdComparer<VERTEX>());

                CalculateFacePlane(newFace);
                faces[i] = newFace;
            }

            // update the adjacency (check all pairs of faces)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = i + 1; j < Dimension + 1; j++)
                    UpdateAdjacency(faces[i], faces[j]);
            }

            return faces;
        }

        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        private bool CalculateFacePlane(SimplexWrap<VERTEX> face)
        {
            VERTEX[] vertices = face.Vertices;
            float[] normal = face.Normal;
            MathHelper<VERTEX>.FindNormalVector(vertices, normal);

            if (float.IsNaN(normal[0]))
            {
                return false;
            }

            float offset = 0.0f;
            float centerDistance = 0.0f;
            float[] fi = vertices[0].Position;

            for (int i = 0; i < Dimension; i++)
            {
                float n = normal[i];
                offset += n * fi[i];
                centerDistance += n * Centroid[i];
            }

            face.Offset = -offset;
            centerDistance -= offset;

            if (centerDistance > 0)
            {
                for (int i = 0; i < Dimension; i++)
                    normal[i] = -normal[i];

                face.Offset = offset;
                face.IsNormalFlipped = true;
            }
            else face.IsNormalFlipped = false;

            return true;
        }

        /// <summary>
        /// Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        private void UpdateAdjacency(SimplexWrap<VERTEX> l, SimplexWrap<VERTEX> r)
        {
            VERTEX[] lv = l.Vertices;
            VERTEX[] rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < Dimension; i++) lv[i].Tag = 0;

            // mark all vertices on the 2nd face
            for (i = 0; i < Dimension; i++) rv[i].Tag = 1;

            // find the 1st false index
            for (i = 0; i < Dimension; i++) if (lv[i].Tag == 0) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < Dimension; j++) if (lv[j].Tag == 0) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < Dimension; i++) lv[i].Tag = 0;
            for (i = 0; i < Dimension; i++)
            {
                if (rv[i].Tag == 1) break;
            }
            r.AdjacentFaces[i] = l;
        }

        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        private void FindBeyondVertices(SimplexWrap<VERTEX> face)
        {
            VertexBuffer<VERTEX> beyondVertices = face.VerticesBeyond;

            Buffer.MaxDistance = float.NegativeInfinity;
            Buffer.FurthestVertex = default(VERTEX);

            int count = Buffer.InputVertices.Count;

            for (int i = 0; i < count; i++)
                IsBeyond(face, beyondVertices, Buffer.InputVertices[i]);

            face.FurthestVertex = Buffer.FurthestVertex;
        }

        #endregion

        #region Process

        /// <summary>
        /// Tags all faces seen from the current vertex with 1.
        /// </summary>
        private void TagAffectedFaces(SimplexWrap<VERTEX> currentFace)
        {
            Buffer.AffectedFaceBuffer.Clear();
            Buffer.AffectedFaceBuffer.Add(currentFace);
            TraverseAffectedFaces(currentFace);
        }

        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        private void TraverseAffectedFaces(SimplexWrap<VERTEX> currentFace)
        {

            Buffer.TraverseStack.Clear();
            Buffer.TraverseStack.Push(currentFace);
            currentFace.Tag = 1;

            while (Buffer.TraverseStack.Count > 0)
            {
                SimplexWrap<VERTEX> top = Buffer.TraverseStack.Pop();

                for (int i = 0; i < Dimension; i++)
                {
                    SimplexWrap<VERTEX> adjFace = top.AdjacentFaces[i];

                    if (adjFace == null) throw new NullReferenceException("(2) Adjacent Face should never be null");

                    if (adjFace.Tag == 0 && MathHelper<VERTEX>.GetVertexDistance(Buffer.CurrentVertex, adjFace) >= PLANE_DISTANCE_TOLERANCE)
                    {
                        Buffer.AffectedFaceBuffer.Add(adjFace);
                        adjFace.Tag = 1;
                        Buffer.TraverseStack.Push(adjFace);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        private bool CreateCone()
        {
            int currentVertexIndex = Buffer.CurrentVertex.Id;
            Buffer.ConeFaceBuffer.Clear();

            for (int fIndex = 0; fIndex < Buffer.AffectedFaceBuffer.Count; fIndex++)
            {
                SimplexWrap<VERTEX> oldFace = Buffer.AffectedFaceBuffer[fIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    SimplexWrap<VERTEX> af = oldFace.AdjacentFaces[i];

                    if (af == null) throw new NullReferenceException("(3) Adjacent Face should never be null");

                    if (af.Tag == 0) // Tag == 0 when oldFaces does not contain af
                    {
                        Buffer.UpdateBuffer[updateCount] = af;
                        Buffer.UpdateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                for (int i = 0; i < updateCount; i++)
                {
                    SimplexWrap<VERTEX> adjacentFace = Buffer.UpdateBuffer[i];

                    int oldFaceAdjacentIndex = 0;
                    SimplexWrap<VERTEX>[] adjFaceAdjacency = adjacentFace.AdjacentFaces;

                    for (int j = 0; j < Dimension; j++)
                    {
                        if (object.ReferenceEquals(oldFace, adjFaceAdjacency[j]))
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    // Index of the face that corresponds to this adjacent face
                    int forbidden = Buffer.UpdateIndices[i];

                    SimplexWrap<VERTEX> newFace;

                    int oldVertexIndex;
                    VERTEX[] vertices;

                    newFace = Buffer.ObjectManager.GetFace();
                    vertices = newFace.Vertices;

                    for (int j = 0; j < Dimension; j++)
                        vertices[j] = oldFace.Vertices[j];

                    oldVertexIndex = vertices[forbidden].Id;

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j].Id > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        orderedPivotIndex = Dimension - 1;
                        for (int j = forbidden + 1; j < Dimension; j++)
                        {
                            if (vertices[j].Id < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }

                    vertices[orderedPivotIndex] = Buffer.CurrentVertex;

                    if (!CalculateFacePlane(newFace))
                    {
                        return false;
                    }

                    Buffer.ConeFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new deferred face.
        /// </summary>
        private DeferredSimplex<VERTEX> MakeDeferredFace(SimplexWrap<VERTEX> face, int faceIndex, SimplexWrap<VERTEX> pivot, int pivotIndex, SimplexWrap<VERTEX> oldFace)
        {
            DeferredSimplex<VERTEX> ret = Buffer.ObjectManager.GetDeferredSimplex();

            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        /// Commits a cone and adds a vertex to the convex hull.
        /// </summary>
        private void CommitCone()
        {
            // Add the current vertex.
            Vertices.Add(Buffer.CurrentVertex);

            // Fill the adjacency.
            for (int i = 0; i < Buffer.ConeFaceBuffer.Count; i++)
            {
                DeferredSimplex<VERTEX> face = Buffer.ConeFaceBuffer[i];

                SimplexWrap<VERTEX> newFace = face.Face;
                SimplexWrap<VERTEX> adjacentFace = face.Pivot;
                SimplexWrap<VERTEX> oldFace = face.OldFace;
                int orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace;

                // let there be a connection.
                for (int j = 0; j < Dimension; j++)
                {
                    if (j == orderedPivotIndex) continue;
                    SimplexConnector<VERTEX> connector = Buffer.ObjectManager.GetConnector();
                    connector.Update(newFace, j, Dimension);
                    ConnectFace(connector);
                }

                // This could slightly help...
                if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
                {
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                }
                else
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);
                }

                // This face will definitely lie on the hull
                if (newFace.VerticesBeyond.Count == 0)
                {
                    Buffer.ConvexSimplexs.Add(newFace);
                    Buffer.UnprocessedFaces.Remove(newFace);
                    Buffer.ObjectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = Buffer.EmptyBuffer;
                }
                else // Add the face to the list
                {
                    Buffer.UnprocessedFaces.Add(newFace);
                }

                // recycle the object.
                Buffer.ObjectManager.DepositDeferredSimplex(face);
            }

            // Recycle the affected faces.
            for (int fIndex = 0; fIndex < Buffer.AffectedFaceBuffer.Count; fIndex++)
            {
                var face = Buffer.AffectedFaceBuffer[fIndex];
                Buffer.UnprocessedFaces.Remove(face);
                Buffer.ObjectManager.DepositFace(face);
            }
        }

        /// <summary>
        /// Connect faces using a connector.
        /// </summary>
        void ConnectFace(SimplexConnector<VERTEX> connector)
        {
            var index = connector.HashCode % Buffer.CONNECTOR_TABLE_SIZE;
            var list = Buffer.ConnectorTable[index];

            for (var current = list.First; current != null; current = current.Next)
            {
                if (SimplexConnector<VERTEX>.AreConnectable(connector, current, Dimension))
                {
                    list.Remove(current);
                    SimplexConnector<VERTEX>.Connect(current, connector);

                    Buffer.ObjectManager.DepositConnector(current);
                    Buffer.ObjectManager.DepositConnector(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Used by update faces.
        /// </summary>
        private void FindBeyondVertices(SimplexWrap<VERTEX> face, VertexBuffer<VERTEX> beyond, VertexBuffer<VERTEX> beyond1)
        {
            var beyondVertices = Buffer.BeyondBuffer;

            Buffer.MaxDistance = float.NegativeInfinity;
            Buffer.FurthestVertex = null;
            VERTEX v;

            int count = beyond1.Count;

            for (int i = 0; i < count; i++)
                beyond1[i].Tag = 1;

            Buffer.CurrentVertex.Tag = 0;

            count = beyond.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond[i];
                if (ReferenceEquals(v, Buffer.CurrentVertex)) continue;
                v.Tag = 0;
                IsBeyond(face, beyondVertices, v);
            }

            count = beyond1.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond1[i];
                if (v.Tag == 1) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = Buffer.FurthestVertex;

            // Pull the old switch a roo
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            Buffer.BeyondBuffer = temp;
        }

        /// <summary>
        /// Handles singular vertex.
        /// </summary>
        private void HandleSingular()
        {
            RollbackCenter();
            Buffer.SingularVertices.Add(Buffer.CurrentVertex);

            // This means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
            for (int fIndex = 0; fIndex < Buffer.AffectedFaceBuffer.Count; fIndex++)
            {
                var face = Buffer.AffectedFaceBuffer[fIndex];
                var vb = face.VerticesBeyond;
                for (int i = 0; i < vb.Count; i++)
                {
                    Buffer.SingularVertices.Add(vb[i]);
                }

                Buffer.ConvexSimplexs.Add(face);
                Buffer.UnprocessedFaces.Remove(face);
                Buffer.ObjectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = Buffer.EmptyBuffer;
            }
        }

        #endregion

        /// <summary>
        /// Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        private void IsBeyond(SimplexWrap<VERTEX> face, VertexBuffer<VERTEX> beyondVertices, VERTEX v)
        {
            float distance = MathHelper<VERTEX>.GetVertexDistance(v, face);

            if (distance >= PLANE_DISTANCE_TOLERANCE)
            {
                if (distance > Buffer.MaxDistance)
                {
                    Buffer.MaxDistance = distance;
                    Buffer.FurthestVertex = v;
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        /// Recalculates the centroid of the current hull.
        /// </summary>
        private void UpdateCenter()
        {
            int count = Vertices.Count + 1;

            for (int i = 0; i < Dimension; i++)
                Centroid[i] *= (count - 1);

            float f = 1.0f / count;

            for (int i = 0; i < Dimension; i++)
                Centroid[i] = f * (Centroid[i] + Buffer.CurrentVertex.Position[i]);
        }

        /// <summary>
        /// Removes the last vertex from the center.
        /// </summary>
        private void RollbackCenter()
        {
            int count = Vertices.Count + 1;

            for (int i = 0; i < Dimension; i++)
                Centroid[i] *= count;

            float f = 1.0f / (count - 1);

            for (int i = 0; i < Dimension; i++)
                Centroid[i] = f * (Centroid[i] - Buffer.CurrentVertex.Position[i]);
        }

    }

}