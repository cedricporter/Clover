﻿using System;
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


        /// <summary>
        /// 重新分配顶点的id号
        /// </summary>
        /// <param name="vertexLayer"></param>
        void ReAllocateVertexID(VertexLayer vertexLayer)
        {
            int vertexID = 0;
            foreach (List<Vertex> vertexList in vertexLayer.VertexCellTable)
            {
                foreach (Vertex v in vertexList)
                {
                    v.ID = vertexID++;
                }
            }
            Vertex.Vertex_count = vertexID;
        }

        public int SaveFile(string filename, FaceLayer faceLayer, EdgeLayer edgeLayer, VertexLayer vertexLayer)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            writer = new BinaryWriter(fs);

            // 魔数
            writer.Write(201203100);

            ReAllocateVertexID(vertexLayer);

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

            writer.Write(Vertex.Vertex_count);
        }

        void SaveFace(Face face)
        {
            writer.Write(face.ID);

            writer.Write(face.StartVertex1.ID);
            writer.Write(face.StartVertex2.ID);

            writer.Write(face.Edges.Count);

            foreach (Edge edge in face.Edges)
            {
                writer.Write(edge.ID);
            }
        }

        void SaveEdge(Edge edge)
        {
            writer.Write(edge.ID);
            writer.Write(edge.Vertex1.ID);
            writer.Write(edge.Vertex2.ID);
        }

        public void SaveEdgeLayer(FileStream fs, EdgeLayer edgeLayer)
        {
            writer.Write("Edge Layer");

            writer.Write(edgeLayer.Count);

            foreach (EdgeTree tree in edgeLayer.EdgeTreeList)
            {
                List<Edge> list = new List<Edge>();
                //TravelEdge(tree.Root, list);

                list.Add(tree.Root);

                // 宽度搜索
                while (list.Count != 0)
                {
                    Edge edge = list[0];
                    list.RemoveAt(0);

                    SaveEdge(edge);
                    
                    if (edge.LeftChild != null && edge.RightChild != null)
                    {
                        list.Add(edge.LeftChild);
                        list.Add(edge.RightChild);
                    }
                    else
                    {
                        writer.Write(-1);
                        writer.Write(-1);
                    }
                }
            }
        }

        public void SaveFaceLayer(FileStream fs, FaceLayer faceLayer)
        {
            writer.Write("Face Layer");

            //TravelFace(faceLayer.Root);
            List<Face> list = new List<Face>();
            list.Add(faceLayer.Root);

            // 宽度搜索
            while (list.Count != 0)
            {
                Face face = list[0];
                list.RemoveAt(0);

                SaveFace(face);

                if (face.LeftChild != null && face.RightChild != null)
                {
                    list.Add(face.LeftChild);
                    list.Add(face.RightChild);
                }
                else
                {
                    writer.Write(-1);
                    writer.Write(-1);
                }
            }
        }

    }
}
