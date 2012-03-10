using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;

namespace Clover
{
    /// <summary>
    /// 提供一些使用的查询折纸结构的工具函数
    /// </summary>
    class CloverTreeHelper
    {

        /// <summary>
        /// 更新面的所有的顶点到在vertexLayer中最新的版本。
        /// </summary>
        public static void UpdateFaceVerticesToLastedVersion(Face face)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            foreach (Edge e in face.Edges)
            {
                e.Vertex1 = vertexLayer.GetVertex(e.Vertex1.Index);
                e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);
            }

            face.StartVertex1 = vertexLayer.GetVertex(face.StartVertex1.Index);
            face.StartVertex2 = vertexLayer.GetVertex(face.StartVertex2.Index);

            face.UpdateVertices();
        }

        /// <summary>
        /// 通过点来找面
        /// </summary>
        /// <param name="faceList">传入一个面表</param>
        /// <param name="vertex"></param>
        /// <returns>返回找到的面</returns>
        public static List<Face> FindFacesFromVertex(List<Face> faceList, Vertex vertex)
        {
            List<Face> list = new List<Face>();

            foreach (Face face in faceList)
            {
                foreach (Edge edge in face.Edges)
                {
                    //if ( CloverMath.IsTwoPointsEqual(edge.Vertex1.GetPoint3D(), vertex.GetPoint3D(), 0.001)
                    //    || CloverMath.IsTwoPointsEqual(edge.Vertex2.GetPoint3D(), vertex.GetPoint3D(), 0.001))
                    if (edge.IsVerticeIn(vertex))
                    {
                        list.Add(face);
                        break;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 判断一个面是否在一个组中
        /// </summary>
        /// <param name="face"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static Boolean IsFaceInGroup(Face face, FaceGroup group)
        {
            // 等待杨旭瑜写完group，。。。
            // todo
            return false;
        }

        /// <summary>
        /// 判断一个点是否在一个面中
        /// </summary>
        /// <param name="vervex"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public static Boolean IsVertexInFace(Vertex vervex, Face face)
        {
            foreach (Vertex v in face.Vertices)
            {
                if (v == vervex)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 返回所有拥有该点的叶子面
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static List<Face> GetReferenceFaces(Vertex v)
        {
            List<Face> facelist = new List<Face>();
            foreach (Face face in CloverController.GetInstance().FaceLeaves)
            {
                if (IsVertexInFace(v, face))
                    facelist.Add(face);
            }
            return facelist;
        }

        /// <summary>
        /// 返回两面的共边，如果没有则返回null
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns></returns>
        public static Edge GetSharedEdge(Face face1, Face face2)
        {
            foreach (Edge edge1 in face1.Edges)
            {
                foreach (Edge edge2 in face2.Edges)
                {
                    if (edge1 == edge2)
                        return edge1;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 返回一个组的基面，用以计算其他面的渲染偏差量
        /// </summary>
        /// <param name="group"></param>
        public static Face FindBaseFace(FaceGroup group)
        {
            //todo 

            return null;
        }


    }
}
