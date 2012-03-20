using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;
using System.Windows.Media.Media3D;

namespace Clover
{
    /// <summary>
    /// 面层的层比较器
    /// </summary>
    public class layerComparer:IComparer<Face>
    {
            public int Compare(Face x, Face y) 
            {
                return (x.Layer.CompareTo(y.Layer));
            }
    };

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
                if (face.Vertices.Contains(vertex))
                    list.Add(face);

                //foreach (Edge edge in face.Edges)
                //{
                //    //if ( CloverMath.IsTwoPointsEqual(edge.Vertex1.GetPoint3D(), vertex.GetPoint3D(), 0.001)
                //    //    || CloverMath.IsTwoPointsEqual(edge.Vertex2.GetPoint3D(), vertex.GetPoint3D(), 0.001))
                //    if (edge.IsVerticeIn(vertex))
                //    {
                //        list.Add(face);
                //        break;
                //    }
                //}
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
        public static Boolean IsVertexInFace(Vertex vertex, Face face)
        {
            foreach (Vertex v in face.Vertices)
            {
                //if (v.GetPoint3D() == vertex.GetPoint3D())
                if(v == vertex)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断一个点是否在一条边上
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static Boolean IsVertexInEdge(Vertex vertex, Edge edge)
        {
            if (vertex.GetPoint3D() == edge.Vertex1.GetPoint3D())
                return true;
            if (vertex.GetPoint3D() == edge.Vertex2.GetPoint3D())
                return true;
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

        /// <summary>
        /// 返回自输入面开始遍历得到的叶子节点
        /// </summary>
        /// <param name="root"></param>
        /// <returns>输入节点的叶子节点</returns>
        public static List<Face> GetLeaves(Face root)
        {
            List<Face> leaves = new List<Face>();
            Travel(root, leaves);
            return leaves;
        }

        /// <summary>
        /// 后续遍历
        /// </summary>
        /// <param name="r">根节点</param>
        static void Travel(Face r, List<Face> leaves)
        {
            if (r == null)
                return;

            Travel(r.LeftChild, leaves);
            Travel(r.RightChild, leaves);

            if (IsLeave(r))
                leaves.Add(r);
        }

        /// <summary>
        /// 判断一个face是否叶子节点
        /// </summary>
        /// <param name="face">face</param>
        /// <returns></returns>
        static bool IsLeave(Face face)
        {
            return face.LeftChild == null && face.RightChild == null;
        }

        /// <summary>
        /// 判断线段是否通过一个Face
        /// </summary>
        /// <param name="face">当前判定Face</param>
        /// <param name="edge">线段端点坐标</param>
        /// <returns></returns>
        public static bool IsEdgeCrossedFace(Face face, Edge edge)
        {
            int crossCount = 0;
            Point3D lastCrossPoint = new Point3D();
            Point3D crossPoint = new Point3D();
            bool isFirst = true;
            foreach (Edge e in face.Edges)
            {
                if (CloverMath.GetIntersectionOfTwoSegments(e, edge, ref crossPoint) == 1)
                { 
                    if (isFirst)
                    {
                        lastCrossPoint.X = crossPoint.X;
                        lastCrossPoint.Y = crossPoint.Y;
                        lastCrossPoint.Z = crossPoint.Z;
                        crossCount++;
                        isFirst = false;
                    }
                    else
                    {
                        if (!CloverMath.AreTwoPointsSameWithDeviation(lastCrossPoint, crossPoint))
                        {
                            crossCount++;
                            lastCrossPoint.X = crossPoint.X;
                            lastCrossPoint.Y = crossPoint.Y;
                            lastCrossPoint.Z = crossPoint.Z;
                        }
                    }
                }
                if (crossCount >= 2)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 找到折线穿过面的那条线段
        /// </summary>
        /// <param name="face">要测试的面</param>
        /// <param name="currentFoldingLine">当前的折线</param>
        /// <returns>对于测试面的折线</returns>
        public static Edge GetEdgeCrossedFace(Face face, Edge currentFoldingLine)
        {
            Vertex vertex1 = new Vertex();
            Vertex vertex2 = new Vertex();
            bool findFirst = false;

            foreach (Edge e in face.Edges)
            {
                Point3D crossPoint = new Point3D();
                if (CloverMath.GetIntersectionOfTwoSegments(e, currentFoldingLine, ref crossPoint) == 1)
                {
                    if (!findFirst)
                    {
                        vertex1.SetPoint3D(crossPoint);
                        findFirst = true;
                    }
                    else if (crossPoint != vertex1.GetPoint3D())
                    {
                        vertex2.SetPoint3D(crossPoint);
                        Edge foldingLine = new Edge(vertex1, vertex2);
                        return foldingLine;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 在一个面中，通过两个顶点找到包含这两个顶点的边
        /// </summary>
        /// <param name="face"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Edge FindEdge(Face face, Vertex v1, Vertex v2)
        {
            foreach (Edge e in face.Edges)
            {
                if (IsVertexInEdge(v1, e) && IsVertexInEdge(v2, e))
                {
                    return e;
                }
            }
            return null;
        }

    }
}
