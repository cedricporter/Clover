using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Clover;

namespace Clover.IO
{

    /// <summary>
    /// Clover文件分析器，分析方式和编译器中的语法分析过程类似
    /// </summary>
    class FileParser
    {
        BinaryReader reader;
        FileStream fs;
        VertexLayer vertexLayer;
        EdgeLayer edgeLayer;
        FaceLayer faceLayer;
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

            Clover();

            //faceLayer.UpdateLeaves();

            fs.Close();
        }

        void Clover()
        {
            VertexLayer();

            EdgeLayer();

            FaceLayer();
        }

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

            Edge root = new Edge(vertexIDDict[vid1], vertexIDDict[vid2]);
            root.ID = edgeID;
            edgeIDDict[root.ID] = root;

            root.LeftChild = EdgeTree();
            root.RightChild = EdgeTree();

            return root;
        }

        void VertexLayer()
        {
            if (!ReadHeader("Vertex Layer"))
                return;

            BinaryFormatter bf = new BinaryFormatter();

            vertexLayer = bf.Deserialize(fs) as VertexLayer;

            Debug.Assert(vertexLayer != null);

            foreach (List<Vertex> vList in vertexLayer.VertexCellTable)
            {
                foreach (Vertex v in vList)
                {
                    vertexIDDict[v.ID] = v;
                }
            }
        }

        void EdgeLayer()
        {
            if (!ReadHeader("Edge Layer"))
                return;

            int number = reader.ReadInt32();

            for (int i = 0; i < number; i++)
            {
                EdgeTree tree = new EdgeTree(EdgeTree());
            }
        }

        void FaceLayer()
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

            int edgeCount = reader.ReadInt32();

            Face face = new Face(0);
            for (int i = 0; i < edgeCount; i++)
            {
                int edgeID = reader.ReadInt32();
                face.AddEdge(edgeIDDict[edgeID]);
            }

            face.LeftChild = FaceTree();
            face.RightChild = FaceTree();
            return null;
        }


    }
}
