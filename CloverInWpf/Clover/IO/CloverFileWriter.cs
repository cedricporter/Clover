using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Clover.IO
{
    class CloverFileWriter
    {
        BinaryWriter writer;

        public int SaveFile(string filename, FaceLayer faceLayer, EdgeLayer edgeLayer, VertexLayer vertexLayer)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            writer = new BinaryWriter(fs);

            // 魔数
            writer.Write(201203100);

            SaveVertexLayer(fs, vertexLayer);

            SaveEdgeLayer(fs, edgeLayer);

            SaveFaceLayer(fs, faceLayer);

            fs.Close();

            return 0;
        }

        /// <summary>
        /// 保存顶点层
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="vertexLayer"></param>
        public void SaveVertexLayer(FileStream fs, VertexLayer vertexLayer)
        {
            writer.Write("Vertex Layer");

            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(fs, vertexLayer);
        }

        int edgeCounter = 0;

        void TravelFace(Face root)
        {
            if (root == null)
            {
                writer.Write(-1);
                return;
            }

            writer.Write(root.ID);

            writer.Write(root.Edges.Count);

            foreach (Edge edge in root.Edges)
            {
                writer.Write(edge.ID);
            }

            TravelFace(root.LeftChild);
            TravelFace(root.RightChild);
        }

        void TravelEdge(Edge root)
        {
            if (root == null)
            {
                writer.Write(-1);
                return;
            }

            writer.Write(root.ID);

            writer.Write(root.Vertex1.ID);
            writer.Write(root.Vertex2.ID);

            TravelEdge(root.LeftChild);
            TravelEdge(root.RightChild);
        }

        public void SaveEdgeLayer(FileStream fs, EdgeLayer edgeLayer)
        {
            writer.Write("Edge Layer");

            writer.Write(edgeLayer.Count);

            foreach (EdgeTree tree in edgeLayer.EdgeTreeList)
            {
                TravelEdge(tree.Root);
            }
        }

        public void SaveFaceLayer(FileStream fs, FaceLayer faceLayer)
        {
            writer.Write("Face Layer");

            TravelFace(faceLayer.Root);
        }

    }
}
