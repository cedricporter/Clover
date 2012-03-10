﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace Clover
{
    public class FoldingSystem
    {
        #region 属性
        List<Face> lastTimeMovedFaces;

        #region get/set
        public List<Face> GetLastTimeMovedFace()
        {
            return lastTimeMovedFaces;
        }
        #endregion

        #endregion
        #region 一些辅助计算的函数
        /// <summary>
        /// 将一堆面的周围的顶点找出并返回一个list
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        List<Vertex> UnionVertex(params Face[] faces)
        {
            List<Vertex> vertices = new List<Vertex>();
            Dictionary<Vertex, int> dict = new Dictionary<Vertex, int>();

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i].UpdateVertices();
                foreach (Vertex v in faces[i].Vertices)
                {
                    dict[v] = 0;
                }
            }

            foreach (KeyValuePair<Vertex, int> pair in dict)
            {
                vertices.Add(pair.Key);
            }

            return vertices;
        }

        /// <summary>
        /// 计算顶点纹理坐标
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool CalculateTexcoord(Vertex vertex, Edge edge)
        {
            // 判断该点是否在直线上
            if (!CloverMath.IsPointInTwoPoints(vertex.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.001))
                return false;

            // 取中间点到其中一点的距离，以及直线长度
            Vector3D v1 = vertex.GetPoint3D() - edge.Vertex1.GetPoint3D();
            Vector3D v2 = edge.Vertex2.GetPoint3D() - edge.Vertex1.GetPoint3D();

            double proportion = v1.Length / v2.Length;

            vertex.u = edge.Vertex1.u + proportion * (edge.Vertex2.u - edge.Vertex1.u);
            vertex.v = edge.Vertex1.v + proportion * (edge.Vertex2.v - edge.Vertex1.v);

            return true; 
        }

        /// <summary>
        /// 计算顶点纹理坐标
        /// </summary>
        /// <param name="vertex">需要计算纹理坐标的点</param>
        /// <param name="edge">该点所在的边</param>
        /// <param name = "length">该边根节点的总长度</param>>
        public bool CalculateTexcoord(Vertex vertex, Edge edge, double length)
        {
            // 确认该点在该直线上
            double a = vertex.X - edge.Vertex1.X;
            double b = edge.Vertex2.X - edge.Vertex1.X;
            double c = vertex.Y - edge.Vertex1.Y;
            double d = edge.Vertex2.Y - edge.Vertex1.Y;
            double e = vertex.Z = edge.Vertex1.Z;
            double f = edge.Vertex2.Z - edge.Vertex1.Z;

            if (Math.Abs(a / b - c / d) > 0.1)
                return false;

            if (Math.Abs(a / b - e / f) > 0.1)
                return false;


            // 取edge两边纹理坐标的较小值
            double u, v;
            if (edge.Vertex1.u == edge.Vertex2.u)
            {
                u = edge.Vertex1.u;
            }
            else
            {
                if (edge.Vertex1.u < edge.Vertex2.u)
                {
                    // 求两点之间的距离
                    Vector3D vd = edge.Vertex1.GetPoint3D() - vertex.GetPoint3D();
                    u = edge.Vertex1.u + vd.Length / length;
                }
                else
                {
                    Vector3D vd = edge.Vertex2.GetPoint3D() - vertex.GetPoint3D();
                    u = edge.Vertex2.u + vd.Length / length;
                }
            }

            if (edge.Vertex1.v == edge.Vertex2.v)
            {
                v = edge.Vertex2.v;
            }
            else
            {
                if (edge.Vertex1.v <= edge.Vertex2.v)
                {
                    Vector3D vd = edge.Vertex1.GetPoint3D() - vertex.GetPoint3D();
                    v = edge.Vertex1.v + vd.Length / length;
                }
                else
                {
                    Vector3D vd = edge.Vertex2.GetPoint3D() - vertex.GetPoint3D();
                    v = edge.Vertex2.v + vd.Length / length;
                }
            }
            vertex.u = u;
            vertex.v = v;

            return true;
        }

        /// <summary>
        /// 在一个面中，通过两个顶点找到包含这两个顶点的边
        /// </summary>
        /// <param name="face"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        Edge FindEdgeByTwoVertexInAFace(Face face, Vertex v1, Vertex v2)
        {
            foreach (Edge e in face.Edges)
            {
                if (e.IsVerticeIn(v1) && e.IsVerticeIn(v2))
                {
                    return e;
                }
            }
            return null;
        }
        #endregion

        #region 分割面

        /// <summary>
        /// 当两个切割点都为原来的顶点时，切割一个面为两个面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithoutAddedVertex(Face face, Edge edge)
        {
            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            EdgeLayer edgeLayer = controller.EdgeLayer;
            FaceLayer faceLayer = controller.FaceLayer;
            ShadowSystem shadowSystem = controller.ShadowSystem;
            Vertex cutVertex1 = null;
            Vertex cutVertex2 = null;

            foreach (Vertex v in face.Vertices)
            {
                if (v.GetPoint3D() == edge.Vertex1.GetPoint3D())
                    cutVertex1 = v;

                if (v.GetPoint3D() == edge.Vertex2.GetPoint3D())
                    cutVertex2 = v;
            }

            // 生成折线边
            Edge foldingEdge = new Edge(cutVertex1, cutVertex2);

            // 分割面
            Face newFace1 = new Face(face.Layer);
            Face newFace2 = new Face(face.Layer);

            face.LeftChild = newFace1;
            face.RightChild = newFace2;

            foldingEdge.Face1 = newFace1;
            foldingEdge.Face2 = newFace2;

            Vertex currentVertex = null;
            Edge currentEdge = null;
            Edge startEdge = null;

            // 为新面1注册边
            newFace1.AddEdge(foldingEdge);

            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == foldingEdge.Vertex1)
                {
                    startEdge = e;
                    currentEdge = e;
                    currentVertex = e.Vertex2;
                    newFace1.AddEdge(e);
                    e.Face1 = newFace1;
                }
                else if (e.Vertex2 == foldingEdge.Vertex1)
                {
                    startEdge = e;
                    currentEdge = e;
                    currentVertex = e.Vertex1;
                    newFace1.AddEdge(e);
                    e.Face1 = newFace1;
                }
            }

            while (currentVertex != foldingEdge.Vertex2)
            {
                foreach (Edge e in face.Edges)
                {
                    if (e.Vertex1 == currentVertex && e != currentEdge)
                    {
                        currentVertex = e.Vertex2;
                        currentEdge = e;
                        newFace1.AddEdge(e);
                        e.Face1 = newFace1;
                        break;
                    }
                    else if (e.Vertex2 == currentVertex && e != currentEdge)
                    {
                        currentVertex = e.Vertex1;
                        currentEdge = e;
                        newFace1.AddEdge(e);
                        e.Face1 = newFace1;
                        break;
                    }
                }
            }

            // 为新面2注册边
            newFace2.AddEdge(foldingEdge);

            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == foldingEdge.Vertex1 && e != startEdge)
                {
                    currentEdge = e;
                    currentVertex = e.Vertex2;
                    newFace2.AddEdge(e);
                    e.Face2 = newFace2;
                }
                else if (e.Vertex2 == foldingEdge.Vertex1 && e != startEdge)
                {
                    currentEdge = e;
                    currentVertex = e.Vertex1;
                    newFace2.AddEdge(e);
                    e.Face2 = newFace2;
                }
            }

            while (currentVertex != foldingEdge.Vertex2)
            {
                foreach (Edge e in face.Edges)
                {
                    if (e.Vertex1 == currentVertex && e != currentEdge)
                    {
                        currentVertex = e.Vertex2;
                        currentEdge = e;
                        newFace2.AddEdge(e);
                        e.Face2 = newFace2;
                        break;
                    }
                    else if (e.Vertex2 == currentVertex && e != currentEdge)
                    {
                        currentVertex = e.Vertex1;
                        currentEdge = e;
                        newFace2.AddEdge(e);
                        e.Face2 = newFace2;
                        break;
                    }
                }
            }

            // 更新当前面中顶点序
            newFace1.UpdateVertices();
            newFace2.UpdateVertices();


            // 找到所有需要保存到VertexLayer历史的顶点
            List<Vertex> oldVertexList = UnionVertex(newFace1, newFace2);
            oldVertexList.Remove(cutVertex1);
            oldVertexList.Remove(cutVertex2);

            // 为所有的顶点生成一个副本插到历史中。
            shadowSystem.SaveVertices(oldVertexList);

            // 更新新的面的顶点到最新版
            UpdateFaceVerticesToLastedVersion(newFace1);
            UpdateFaceVerticesToLastedVersion(newFace2);

            // 更新渲染层的部分
            render.Delete(face);
            render.New(newFace1);
            render.New(newFace2);

            //renderController.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            faceLayer.UpdateLeaves();
            return;
        }

        /// <summary>
        /// 当割点在一条边上时，另一割点为原来顶点时，切割一个面为两个面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedOneVertex(Face face, Edge edge, Edge cuttedEdge, Vertex cutVertex)
        {
            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            EdgeLayer edgeLayer = controller.EdgeLayer;
            FaceLayer faceLayer = controller.FaceLayer;
            ShadowSystem shadowSystem = controller.ShadowSystem;
            // 找出需要添加的点和另外一个已经存在的点 
            Vertex newVertex; Vertex otherVertex;

            newVertex = cutVertex.Clone() as Vertex;
            CalculateTexcoord(newVertex, cuttedEdge);
            vertexLayer.InsertVertex(newVertex);

            if (edge.Vertex1 != cutVertex)
            {
                otherVertex = edge.Vertex1;
            }
            else
            {
                otherVertex = edge.Vertex2; 
            }
            foreach (Vertex v in face.Vertices)
            {
                if (v.GetPoint3D() == otherVertex.GetPoint3D())
                {
                    otherVertex = v;
                    break;
                }
            }

            // 生成折线边
            Edge foldingEdge = new Edge(newVertex, otherVertex);
            edgeLayer.AddTree(new EdgeTree(foldingEdge));

            // 切割边
            Edge newEdge1 = new Edge(cuttedEdge.Vertex1, newVertex);
            Edge newEdge2 = new Edge(newVertex, cuttedEdge.Vertex2);

            // 将新生成的边添加到原来边的左右子树
            cuttedEdge.LeftChild = newEdge1;
            cuttedEdge.RightChild = newEdge2;

            // 分割面
            Face newFace1 = new Face(face.Layer);
            Face newFace2 = new Face(face.Layer);

            face.LeftChild = newFace1;
            face.RightChild = newFace2;

            Vertex currentVertex = null;
            Edge currentEdge = null;

            // 为折线边注册面
            foldingEdge.Face1 = newFace1;
            foldingEdge.Face2 = newFace2;

            // 为一个面注册相应的边
            newFace1.AddEdge(foldingEdge);
            foldingEdge.Face1 = newFace1;
            newFace1.AddEdge(newEdge1);
            newEdge1.Face1 = newFace1;

            // 添加face1中的边
            // 从新生成的边开始环绕面
            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == newEdge1.Vertex1)
                {
                    currentVertex = e.Vertex2;
                    currentEdge = e;
                    newFace1.AddEdge(e);
                    e.Face1 = newFace1;
                    break;
                }
                else if (e.Vertex2 == newEdge1.Vertex1)
                {
                    currentVertex = e.Vertex1;
                    currentEdge = e;
                    newFace1.AddEdge(e);
                    e.Face1 = newFace1;
                    break;
                }
            }

            while (currentVertex != otherVertex)
            {
                foreach (Edge e in face.Edges)
                {

                    if (e.Vertex1 == currentVertex && e != currentEdge)
                    {
                        //添加该边到新增面的边表
                        e.Face1 = newFace1;
                        newFace1.AddEdge(e);
                        currentVertex = e.Vertex2;
                        currentEdge = e;
                        break;
                    }
                    else if (e.Vertex2 == currentVertex && e != currentEdge)
                    {
                        e.Face1 = newFace1;
                        newFace1.AddEdge(e);
                        currentVertex = e.Vertex1;
                        currentEdge = e;
                        break;
                    }
                }
            }

            // 添加face2中的边
            // 为一个面注册相应的边
            newFace2.AddEdge(foldingEdge);
            edge.Face2 = newFace2;
            newFace2.AddEdge(newEdge2);
            edge.Face2 = newFace2;

            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == newEdge2.Vertex2)
                {
                    e.Face2 = newFace2;
                    newFace2.AddEdge(e);
                    currentVertex = e.Vertex2;
                    currentEdge = e;
                    break;
                }
                else if (e.Vertex2 == newEdge1.Vertex2)
                {
                    e.Face2 = newFace2;
                    newFace2.AddEdge(e);
                    currentVertex = e.Vertex1;
                    currentEdge = e;
                    break;
                }
            }

            while (currentVertex != otherVertex)
            {
                foreach (Edge e in face.Edges)
                {

                    if (e.Vertex1 == currentVertex && e != currentEdge)
                    {
                        //添加该边到新增面的边表
                        e.Face2 = newFace2;
                        newFace2.AddEdge(e);
                        currentVertex = e.Vertex2;
                        currentEdge = e;
                        break;
                    }
                    else if (e.Vertex2 == currentVertex && e != currentEdge)
                    {
                        e.Face2 = newFace2;
                        newFace2.AddEdge(e);
                        currentVertex = e.Vertex1;
                        currentEdge = e;
                        break;
                    }
                }
            }

            // 更新当前面中顶点序
            newFace1.UpdateVertices();
            newFace2.UpdateVertices();


            // 找到所有需要保存到VertexLayer历史的顶点
            List<Vertex> oldVertexList = UnionVertex(newFace1, newFace2);
            oldVertexList.Remove(newVertex);
            oldVertexList.Remove(otherVertex);

            // 为所有的顶点生成一个副本插到历史中。
            shadowSystem.SaveVertices(oldVertexList);

            // 更新新的面的顶点到最新版
            UpdateFaceVerticesToLastedVersion(newFace1);
            UpdateFaceVerticesToLastedVersion(newFace2);

            // 更新渲染层的部分
            render.Delete(face);
            render.New(newFace1);
            render.New(newFace2);

            newVertex.Update(newVertex, null);
            //renderController.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            faceLayer.UpdateLeaves();
        }

        /// <summary>
        /// 当割点在两条边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public Edge CutAFaceWithAddedTwoVertices(Face face, Edge edge)
        {
            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            ShadowSystem shadowSystem = controller.ShadowSystem;

            //// 快照
            //shadowSystem.Snapshot();

            Vertex newVertex1 = edge.Vertex1.Clone() as Vertex;
            Vertex newVertex2 = edge.Vertex2.Clone() as Vertex;
            Vertex newVertexOld1 = newVertex1;
            Vertex newVertexOld2 = newVertex2;

            // 生成一个面的周围的顶点的环形表
            face.UpdateVertices();
            List<Vertex> vertexList = new List<Vertex>();
            vertexList.AddRange(face.Vertices);
            int count = vertexList.Count + 1;
            while (true)
            {
                if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[0].GetPoint3D(), vertexList[1].GetPoint3D(), 0.001)
                    || CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[0].GetPoint3D(), vertexList[1].GetPoint3D(), 0.001))
                {
                    break;
                }
                vertexList.Add(vertexList[0]);
                vertexList.RemoveAt(0);

                if (count-- == 0)
                    return null;
            }
            vertexList.Add(vertexList[0]);

            // 要被分割的边
            Edge beCutEdge1 = null;     
            Edge beCutEdge2 = null;

            // 原始边
            List<Edge> rangeA = new List<Edge>();
            List<Edge> rangeB = new List<Edge>();

            // 分割面
            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            face.LeftChild = f1;
            face.RightChild = f2;

            List<Vertex> ignoreAddHistoryVertexList = new List<Vertex>();

            List<Edge> currentEdgeList = null;
            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                Edge currentEdge = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);
                if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    currentEdgeList = rangeA;

                    beCutEdge1 = currentEdge;
                    Edge cutEdge1 = null;
                    Edge cutEdge2 = null;

                    // 两个孩子为空，新建两条边
                    if (beCutEdge1.LeftChild == null && beCutEdge1.RightChild == null)
                    {
                        Debug.Assert(newVertex1 != null);
                        // 分割一条边生成两条新的边
                        cutEdge1 = new Edge(vertexList[i], newVertex1);
                        cutEdge2 = new Edge(newVertex1, vertexList[i + 1]);
                        beCutEdge1.LeftChild = cutEdge1;
                        beCutEdge1.RightChild = cutEdge2;
                        rangeB.Add(cutEdge1);
                        rangeA.Add(cutEdge2);
                    }
                    else
                    {
                        // 对于已经割过的边，我们“必须”使用它原来的顶点
                        if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.LeftChild.Vertex1.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.LeftChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.LeftChild.Vertex2.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.LeftChild.Vertex2;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.RightChild.Vertex1.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.RightChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.RightChild.Vertex2.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.RightChild.Vertex2;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("fuck2");
                        }

                        //ignoreAddHistoryVertexList.Add(beCutEdge1.LeftChild.Vertex1);
                        //ignoreAddHistoryVertexList.Add(beCutEdge1.LeftChild.Vertex2);
                        //ignoreAddHistoryVertexList.Add(beCutEdge1.RightChild.Vertex1);
                        //ignoreAddHistoryVertexList.Add(beCutEdge1.RightChild.Vertex2);

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeB.Add(beCutEdge1.LeftChild);
                            rangeA.Add(beCutEdge1.RightChild);
                        }
                        else
                        {
                            rangeA.Add(beCutEdge1.LeftChild);
                            rangeB.Add(beCutEdge1.RightChild);
                        }
                    }

                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex1, beCutEdge1);
                    continue;
                }
                else if (CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    currentEdgeList = rangeB;

                    beCutEdge2 = currentEdge;

                    // 两个孩子为空，新建两条边
                    if (beCutEdge2.LeftChild == null && beCutEdge2.RightChild == null)
                    {
                        Debug.Assert(newVertex2 != null);
                        // 分割一条边生成两条新的边
                        Edge cutEdge1 = new Edge(vertexList[i], newVertex2);
                        Edge cutEdge2 = new Edge(newVertex2, vertexList[i + 1]);
                        beCutEdge2.LeftChild = cutEdge1;
                        beCutEdge2.RightChild = cutEdge2;

                        rangeA.Add(cutEdge1);
                        rangeB.Add(cutEdge2);
                    }
                    else
                    {
                        // 对于已经割过的边，我们“必须”使用它原来的顶点
                        if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.LeftChild.Vertex1.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.LeftChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.LeftChild.Vertex2.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.LeftChild.Vertex2;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.RightChild.Vertex1.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.RightChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.RightChild.Vertex2.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.RightChild.Vertex2;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("fuck");
                        }

                        //ignoreAddHistoryVertexList.Add(beCutEdge2.LeftChild.Vertex1);
                        //ignoreAddHistoryVertexList.Add(beCutEdge2.LeftChild.Vertex2);
                        //ignoreAddHistoryVertexList.Add(beCutEdge2.RightChild.Vertex1);
                        //ignoreAddHistoryVertexList.Add(beCutEdge2.RightChild.Vertex2);

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeA.Add(beCutEdge2.LeftChild);
                            rangeB.Add(beCutEdge2.RightChild);
                        }
                        else
                        {
                            rangeB.Add(beCutEdge2.LeftChild);
                            rangeA.Add(beCutEdge2.RightChild);
                        }
                    }
                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex2, beCutEdge2);
                    continue;
                }
                currentEdgeList.Add(currentEdge);
            }

            Edge newEdge = new Edge(newVertex1, newVertex2);

            // 是否是新的顶点
            if (newVertex1 == newVertexOld1)
            {
                vertexLayer.InsertVertex(newVertex1);
                //render.AddVisualInfoToVertex(newVertex1);
                //ignoreAddHistoryVertexList.Add(newVertex1);
            }
            if (newVertex2 == newVertexOld2)
            {
                vertexLayer.InsertVertex(newVertex2);
                //render.AddVisualInfoToVertex(newVertex2);
                //ignoreAddHistoryVertexList.Add(newVertex2);
            }


            // 更新新边的Faces指针 
            newEdge.Face1 = f1;
            newEdge.Face2 = f2;

            rangeA.Add(newEdge);
            rangeB.Add(newEdge);

            foreach (Edge e in rangeA)
            {
                f1.AddEdge(e);
            }
            foreach (Edge e in rangeB)
            {
                f2.AddEdge(e);
            }

            f1.UpdateVertices();
            f2.UpdateVertices();

            #region history
            //foreach (Edge e in rangeA)
            //{
            //    f1.AddEdge(e);
            //    foreach (Vertex v in ignoreAddHistoryVertexList)
            //    {
            //        if (e.Vertex1.Index == v.Index)
            //            e.Vertex1 = v;
            //        if (e.Vertex2.Index == v.Index)
            //            e.Vertex2 = v;
            //    }
            //}
            //foreach (Edge e in rangeB)
            //{
            //    f2.AddEdge(e);
            //    foreach (Vertex v in ignoreAddHistoryVertexList)
            //    {
            //        if (e.Vertex1.Index == v.Index)
            //            e.Vertex1 = v;
            //        if (e.Vertex2.Index == v.Index)
            //            e.Vertex2 = v;
            //    }
            //}

            //f1.AddEdge(newEdge);
            //f2.AddEdge(newEdge);

            //f1.UpdateVertices();
            //f2.UpdateVertices();

            //// 找到所有需要保存到VertexLayer历史的顶点
            //List<Vertex> oldVertexList = UnionVertex(f1, f2);
            ////oldVertexList = oldVertexList.Except(ignoreAddHistoryVertexList).ToList();
            //foreach (Vertex v in ignoreAddHistoryVertexList)
            //{
            //    oldVertexList.Remove(v);
            //}

            ////// 为所有的顶点生成一个副本插到历史中。
            ////shadowSystem.SaveVertices(oldVertexList);

            ////// 更新新的面的顶点到最新版
            ////UpdateFaceVerticesToLastedVersion(f1);
            ////UpdateFaceVerticesToLastedVersion(f2);

            //controller.Table.DeleteFace(face);
            //controller.Table.AddFace(f1);
            //controller.Table.AddFace(f2);
            #endregion

            // 更新渲染层的部分
            render.Delete(face);
            render.New(f1);
            render.New(f2);

            //render.AntiOverlap();

            //newVertex1.Update(newVertex1, null);
            //newVertex2.Update(newVertex2, null);
            //render.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            controller.FaceLayer.UpdateLeaves();

            return newEdge;
        }

        #region 辅助函数
        /// <summary>
        /// 更新面的所有的顶点到在vertexLayer中最新的版本。
        /// </summary>
        public void UpdateFaceVerticesToLastedVersion(Face face)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            foreach (Edge e in face.Edges)
            {
                e.Vertex1 = vertexLayer.GetVertex(e.Vertex1.Index);
                e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);
            }
            face.UpdateVertices();
        }
        #endregion

        //int counterFUcker = 1;
        public void CutFaces(List<Face> faceList, Edge foldingLine)
        {
            List<Edge> newEdges = new List<Edge>();

            foreach (Face face in faceList)
            {
                Edge edge = GetFoldingLineOnAFace(face, foldingLine);

                Debug.Assert(edge != null);

                newEdges.Add(CutAFaceWithAddedTwoVertices(face, edge));
            }

            // 拍快照
            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;
            SnapshotNode node = new SnapshotNode(CloverController.GetInstance().FaceLayer.Leaves);
            node.NewEdges = newEdges;
            node.Type = SnapshotNodeKind.CutKind;

            shadowSystem.Snapshot(node);
        }

        #endregion

        #region 有关折线的函数

        /// <summary>
        /// 获取折线
        /// </summary>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="pickedPoint">选中的点</param>
        /// <param name="projectionPoint">投影点</param>
        /// <returns></returns>
        public Edge GetFoldingLine(Face pickedFace, Point3D pickedPoint, Point3D projectionPoint)
        {
            return CloverMath.GetPerpendicularBisector3D(pickedFace, pickedPoint, projectionPoint);
        }
        /// <summary>
        /// 找到折线穿过面的那条线段
        /// </summary>
        /// <param name="face">要测试的面</param>
        /// <param name="currentFoldingLine">当前的折线</param>
        /// <returns>对于测试面的折线</returns>
        public Edge GetFoldingLineOnAFace(Face face, Edge currentFoldingLine)
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
                    else if ( crossPoint != vertex1.GetPoint3D())
                    {
                        vertex2.SetPoint3D(crossPoint);
                        Edge foldingLine = new Edge(vertex1, vertex2);
                        return foldingLine;
                    }
                }
            }
            return null;
        }

        #endregion

        #region 有关折叠的函数

        /// <summary>
        /// 直接折叠到投影点上
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="pickedVertex"></param>
        /// <param name="projectionPoint"></param>
        public bool FoldingUpToPoint(Face pickedFace, Vertex pickedVertex, Point3D projectionPoint, Edge currentFoldingLine)
        {
            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;
            shadowSystem.SaveOriginState();

            // 查找所有需要移动的面
            List<Face> rotateFaces = AddMovedFace(pickedVertex, pickedFace, currentFoldingLine);
            lastTimeMovedFaces = rotateFaces;

            // 计算所需旋转角度
            Point3D crossPoint = new Point3D();
            Edge segmentFromOriginToPro = new Edge(pickedVertex, new Vertex(projectionPoint));

            if (1 != CloverMath.GetIntersectionOfTwoSegments(currentFoldingLine, segmentFromOriginToPro, ref crossPoint))
            {
                // System.Windows.MessageBox.Show("Fuck 木有交点");
                return false;
            }

            Vector3D v1 = pickedVertex.GetPoint3D() - crossPoint;
            Vector3D v2 = projectionPoint - crossPoint;

            double angle = Vector3D.AngleBetween(v1, v2);

            // 根据旋转角度对所有移动面进行旋转
            RotateFaces(rotateFaces, currentFoldingLine, angle);

            return true;
        }

        /// <summary>
        /// 测试要移动的面
        /// </summary>
        /// <param name="face">待测试的面</param>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="pickedVertex">选中的点</param>
        /// <returns></returns>
        public bool TestMovedFace(Face face, Face pickedFace, Vertex pickedVertex)
        {
            // 选定的面一定是移动面
            if (face == pickedFace)
                return true;

            // 所有和移动面有共同边的面都是移动面,即拥有选择点的面
            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == pickedVertex || e.Vertex2 == pickedVertex)
                {
                    return true; 
                }
            }

            // 若有面覆盖在该面上，也为移动面
            // 需要面分组中的层次信息
            // bla bla bla.

            return false;
        }
        
        /// <summary>
        /// 判断折线是否通过该平面
        /// </summary>
        /// <param name="face">当前判定平面</param>
        /// <param name="currentFoldingLine">折线亮点坐标</param>
        /// <returns></returns>
        public bool TestFoldingLineCrossed(Face face, Edge currentFoldingLine)
        {
            int crossCount = 0;
            foreach (Edge e in face.Edges)
            { 
                Point3D crossPoint = new Point3D();
                if (CloverMath.GetIntersectionOfTwoSegments(e, currentFoldingLine, ref crossPoint) == 1)
                    crossCount++;
            }
            return crossCount >= 2;
        }

        /// <summary>
        /// 判定移动的面
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public List<Face> AddMovedFace(Vertex pickedVertex, Face pickedFace, Edge foldingLine)
        {
            FaceLayer faceLayer = CloverController.GetInstance().FaceLayer;
            LookupTable table = CloverController.GetInstance().Table;
            table.UpdateLookupTable();

            List<Face> faceWithFoldingLine = new List<Face>();
            List<Face> faceWithoutFoldingLine = new List<Face>();

            // 根据面组遍历所有面，判定是否属于移动面并分组插入
            foreach (Face face in faceLayer.Leaves)
            {
                if (TestMovedFace(face, pickedFace, pickedVertex))
                {
                    if (TestFoldingLineCrossed(face, foldingLine))
                    {
                        faceWithFoldingLine.Add(face);
                    }
                    else
                    {
                        faceWithoutFoldingLine.Add(face);
                    }
                }
            }

            CutFaces(faceWithFoldingLine, foldingLine);

            foreach (Face face in faceWithFoldingLine)
            {
                faceWithoutFoldingLine.Add(face.LeftChild);
                faceWithoutFoldingLine.Add(face.RightChild);
            }

            faceWithoutFoldingLine = CloverTreeHelper.FindFacesFromVertex(faceWithoutFoldingLine, pickedVertex);
            return faceWithoutFoldingLine;
        }
        #endregion

        #region 有关旋转的函数

        /// <summary>
        /// 克隆和更新一个新的顶点到VertexLayer
        /// </summary>
        /// <param name="v"></param>
        void CloneAndUpdateVertex(Vertex v)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            vertexLayer.UpdateVertex(v.Clone() as Vertex, v.Index);
        }

        /// <summary>
        /// 旋转一个面表中除去折痕的所有点 
        /// </summary>
        /// <param name="beRotatedFaceList">待旋转的面表</param>
        /// <param name="foldingLine">折线</param>
        /// <param name="angle">角度</param>
        public void RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            RenderController render = CloverController.GetInstance().RenderController;
            LookupTable table = CloverController.GetInstance().Table;

            List<Vertex> movedVertexList = new List<Vertex>();

            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.Vertex1.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex1.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex1.Moved )
                    {
                        CloneAndUpdateVertex(e.Vertex1);
                        e.Vertex1 = vertexLayer.GetVertex(e.Vertex1.Index);

                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;
                        e.Vertex1.SetPoint3D(rotateTransform.Transform(e.Vertex1.GetPoint3D()));
                        e.Vertex1.Moved = true;
                        movedVertexList.Add(e.Vertex1);
                    }

                    if (e.Vertex2.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex2.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex2.Moved)
                    {
                        CloneAndUpdateVertex(e.Vertex2);
                        e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);

                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        //TranslateTransform3D translateToOrigin = new TranslateTransform3D( -e.Vertex1.X, -e.Vertex1.Y, -e.Vertex1.Z);
                        //TranslateTransform3D translateBack = new TranslateTransform3D(e.Vertex1.X, e.Vertex1.Y, e.Vertex1.Z);
                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;

                        //e.Vertex2.SetPoint3D(translateToOrigin.Transform(e.Vertex2.GetPoint3D()));
                        e.Vertex2.SetPoint3D(rotateTransform.Transform(e.Vertex2.GetPoint3D()));
                        //e.Vertex2.SetPoint3D(translateBack.Transform(e.Vertex2.GetPoint3D()));
                        e.Vertex2.Moved = true;
                    }
                }
            }

            foreach (Face f in CloverController.GetInstance().FaceLayer.Leaves)
            {
                CloverTreeHelper.UpdateFaceVerticesToLastedVersion(f);
            }

            // 修正所有点的移动属性
            foreach (Vertex v in vertexLayer.Vertices)
            {
                v.Moved = false; 
            }

            // 必须先更新group后更新render
            table.UpdateLookupTable();
            render.UpdateAll();

            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;
            SnapshotNode node = new SnapshotNode(CloverController.GetInstance().FaceLayer.Leaves);
            node.MovedVertexList = movedVertexList;
            node.Type = SnapshotNodeKind.RotateKind;
            shadowSystem.Snapshot(node);
        }

        #endregion
    }
}
