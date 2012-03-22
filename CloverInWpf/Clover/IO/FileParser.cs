using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace Clover.IO
{

    /// <summary>
    /// Clover文件分析器，分析方式和编译器中的语法分析过程类似
    /// 采用自顶向下的分析方法。
    /// </summary>
    /// <remarks>
    /// 文法
    /// Clover           –> VertexLayer EdgeLayer FaceLayer
    /// VertexLayer      -> TrunkName VertexTable
    /// EdgeLayer        -> TrunkName num EdgeTree+
    /// EdgeTree         –> Edge+
    /// FaceLayer        –> TrunkName FaceTree
    /// FaceTree         –> Face+
    /// Edge             –> edge_id vertex1_id vertex2_id
    ///                     | –1
    /// Face             –> face_id start_vertex1 start_vertex2 num edge_id+
    ///                     | –1
    /// face_id          -> positive_number
    /// edge_id          -> positive_number
    /// </remarks>
    class FileParser
    {
        BinaryReader reader;
        FileStream fs;

        VertexLayer vertexLayer;
        EdgeLayer edgeLayer;
        FaceLayer faceLayer;
        ShadowSystem shadowSystem;

        #region get/set
        public Clover.ShadowSystem ShadowSystem
        {
            get { return shadowSystem; }
        }
        public Clover.VertexLayer VertexLayer
        {
            get { return vertexLayer; }
        }
        public Clover.EdgeLayer EdgeLayer
        {
            get { return edgeLayer; }
        }
        public Clover.FaceLayer FaceLayer
        {
            get { return faceLayer; }
        }
        #endregion

        /// <summary>
        /// 用于重新组装整个数据结构
        /// </summary>
        Dictionary<int, Vertex> vertexIDDict;
        Dictionary<int, Edge> edgeIDDict;
        Dictionary<int, Face> faceIDDict;

        public void LoadFile(string filename)
        {
            fs = new FileStream(filename, FileMode.Open);

            reader = new BinaryReader(fs);

            int magicNumber = reader.ReadInt32();

            vertexIDDict = new Dictionary<int, Vertex>();
            edgeIDDict = new Dictionary<int, Edge>();
            faceIDDict = new Dictionary<int, Face>();

            vertexLayer = new VertexLayer();
            edgeLayer = new EdgeLayer();
            faceLayer = new FaceLayer();
            shadowSystem = new ShadowSystem();

            Clover();

            faceLayer.UpdateLeaves();

            fs.Close();
        }

        #region 分析
        void Clover()
        {
            _VertexLayer();

            _EdgeLayer();

            _FaceLayer();

            _ShadowSystem();
        }

        #region ShadowSystem
        void _ShadowSystem()
        {
            if (!ReadHeader("ShadowSystem"))
                return;

            int snapshotNodeCount = reader.ReadInt32();

            for (int i = 0; i < snapshotNodeCount; i++)
            {
                SnapshotNode node = SnapshotNode();
                shadowSystem.Snapshot(node);
            }
        }

        SnapshotNode SnapshotNode()
        {
            SnapshotNode node = new SnapshotNode();
            node.Type = (SnapshotNodeKind)reader.ReadInt32();

            List<Face> leaves = new List<Face>();
            int faceCount = reader.ReadInt32();
            for (int i = 0; i < faceCount; i++)
            {
                int faceID = reader.ReadInt32();
                leaves.Add(faceIDDict[faceID]);
            }
            node.FaceLeaves = leaves;

            List<Edge> newEdgeList = new List<Edge>();
            int newEdgeCount = reader.ReadInt32();
            for (int i = 0; i < newEdgeCount; i++)
            {
                int edgeID = reader.ReadInt32();
                newEdgeList.Add(edgeIDDict[edgeID]);
            }
            node.NewEdges = newEdgeList;

            List<Vertex> vertexList = new List<Vertex>();
            int vertexCount = reader.ReadInt32();
            for (int i = 0; i < vertexCount; i++)
            {
                int vertexID = reader.ReadInt32();
                vertexList.Add(vertexIDDict[vertexID]);
            }
            node.MovedVertexList = vertexList;

            int originVertexListCount = reader.ReadInt32();
            node.OriginVertexListCount = originVertexListCount;
            int originEdgeListCount = reader.ReadInt32();
            node.OriginEdgeListCount = originEdgeListCount;

            return node;
        }
        #endregion

        bool ReadHeader(string headerName)
        {
            string header = reader.ReadString();
            if (header != headerName)
                return false;
            return true;
        }

        Edge EdgeTree()
        {
            int edgeID = reader.ReadInt32();
            if (edgeID == -1)
                return null;

            int vid1 = reader.ReadInt32();
            int vid2 = reader.ReadInt32();

            Edge edge = new Edge(vertexIDDict[vid1], vertexIDDict[vid2]);
            edge.ID = edgeID;
            edgeIDDict[edge.ID] = edge;

            edge.LeftChild = EdgeTree();
            edge.RightChild = EdgeTree();

            return edge;
        }

        void _VertexLayer()
        {
            if (!ReadHeader("Vertex Layer"))
                return;

            BinaryFormatter bf = new BinaryFormatter();

            vertexLayer = bf.Deserialize(fs) as VertexLayer;

            Debug.Assert(vertexLayer != null);

            int vertex_count = reader.ReadInt32();
            Vertex.Vertex_count = vertex_count;

            foreach (List<Vertex> vList in vertexLayer.VertexCellTable)
            {
                foreach (Vertex v in vList)
                {
                    vertexIDDict[v.ID] = v;
                }
            }
        }

        void _EdgeLayer()
        {
            if (!ReadHeader("Edge Layer"))
                return;

            int number = reader.ReadInt32();

            for (int i = 0; i < number; i++)
            {
                EdgeTree tree = new EdgeTree(EdgeTree());
                edgeLayer.AddTree(tree);
            }
        }

        void _FaceLayer()
        {
            if (!ReadHeader("Face Layer"))
                return;

            Face root = FaceTree();
            faceLayer.Initliaze(root);
        }

        Face FaceTree()
        {
            int faceID = reader.ReadInt32();
            if (faceID == -1)
                return null;

            int startVertex1ID = reader.ReadInt32();
            int startVertex2ID = reader.ReadInt32();

            int edgeCount = reader.ReadInt32();

            Face face = new Face(0);
            face.ID = faceID;
            faceIDDict[face.ID] = face;

            for (int i = 0; i < edgeCount; i++)
            {
                int edgeID = reader.ReadInt32();
                face.AddEdge(edgeIDDict[edgeID]);
            }

            face.StartVertex1 = vertexIDDict[startVertex1ID];
            face.StartVertex2 = vertexIDDict[startVertex2ID];

            face.LeftChild = FaceTree();
            face.RightChild = FaceTree();

            face.UpdateVertices();

            return face;
        }
        #endregion


    }
}
