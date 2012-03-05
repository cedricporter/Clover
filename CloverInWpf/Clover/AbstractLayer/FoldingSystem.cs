using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace Clover
{
    public class FoldingSystem
    {
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
        bool CalculateTexcoord(Vertex vertex, Edge edge)
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
        bool CalculateTexcoord(Vertex vertex, Edge edge, double length)
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
        /// 当割点在一条边上时，另一割点为原来顶点时，切割一个面为两个面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedOneVertex(Face face, Edge edge, Edge cuttedEdge, Vertex cutVertex)
        {
            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
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

            // 为一个面注册相应的边
            newFace1.AddEdge(edge);
            edge.Face1 = newFace1;
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
            newFace2.AddEdge(edge);
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
            shadowSystem.UpdateFaceVerticesToLastedVersion(newFace1);
            shadowSystem.UpdateFaceVerticesToLastedVersion(newFace2);

            // 更新渲染层的部分
            render.Delete(face);
            render.New(newFace1);
            render.New(newFace2);

            newVertex.Update(newVertex, null);
            //renderController.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            controller.FaceLayer.UpdateLeaves();
            

        }

        /// <summary>
        /// 当割点在两条边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedTwoVertices(Face face, Edge edge)
        {
            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            ShadowSystem shadowSystem = controller.ShadowSystem;

            Vertex newVertex1 = edge.Vertex1.Clone() as Vertex;
            Vertex newVertex2 = edge.Vertex2.Clone() as Vertex;

            render.AddVisualInfoToVertex(newVertex1);
            render.AddVisualInfoToVertex(newVertex2);

            Edge newEdge = new Edge(newVertex1, newVertex2);

            vertexLayer.InsertVertex(newVertex1);
            vertexLayer.InsertVertex(newVertex2);

            // 生成一个面的周围的顶点的环形表
            face.UpdateVertices();
            List<Vertex> vertexList = new List<Vertex>();
            vertexList.AddRange(face.Vertices);
            vertexList.Add(face.Vertices[0]);

            // 割点的要插入的下标
            int indexV1 = -1;
            int indexV2 = -1;
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

            // 更新新边的Faces指针 
            newEdge.Face1 = f1;
            newEdge.Face2 = f2;

            // 将边分成两组，一个面得到一组
            bool bFirstRange = true;
            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                if (bFirstRange)
                {
                    rangeA.Add(FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]));
                }
                else
                {
                    rangeB.Add(FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]));
                }

                if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    bFirstRange = false;

                    indexV1 = i;
                    beCutEdge1 = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);
                    rangeA.RemoveAt(rangeA.Count - 1);

                    // 分割一条边生成两条新的边
                    Edge cutEdge1 = new Edge(vertexList[i], newVertex1);
                    Edge cutEdge2 = new Edge(newVertex1, vertexList[i + 1]);
                    beCutEdge1.LeftChild = cutEdge1;
                    beCutEdge1.RightChild = cutEdge2;

                    rangeA.Add(cutEdge1);
                    rangeA.Add(newEdge);
                    rangeB.Add(cutEdge2);

                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex1, beCutEdge1);
                }
                if (CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    if (bFirstRange)
                    {
                        bFirstRange = false;
                        List<Edge> temp = rangeA;
                        rangeA = rangeB;
                        rangeB = temp;
                    }
                    else
                    {
                        bFirstRange = true;
                    }

                    indexV2 = i;
                    beCutEdge2 = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);
                    rangeB.RemoveAt(rangeB.Count - 1);

                    // 分割一条边生成两条新的边
                    Edge cutEdge1 = new Edge(vertexList[i], newVertex2);
                    Edge cutEdge2 = new Edge(newVertex2, vertexList[i + 1]);
                    beCutEdge2.LeftChild = cutEdge1;
                    beCutEdge2.RightChild = cutEdge2;

                    rangeB.Add(cutEdge1);
                    rangeB.Add(newEdge);
                    rangeA.Add(cutEdge2);

                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex2, beCutEdge2);
                }
            }

            Debug.Assert(indexV1 != -1 && indexV2 != -1);

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

            // 找到所有需要保存到VertexLayer历史的顶点
            List<Vertex> oldVertexList = UnionVertex(f1, f2);
            oldVertexList.Remove(newVertex1);
            oldVertexList.Remove(newVertex2);

            // 为所有的顶点生成一个副本插到历史中。
            shadowSystem.SaveVertices(oldVertexList);

            // 更新新的面的顶点到最新版
            shadowSystem.UpdateFaceVerticesToLastedVersion(f1);
            shadowSystem.UpdateFaceVerticesToLastedVersion(f2);

            // 更新渲染层的部分
            render.Delete(face);
            render.New(f1);
            render.New(f2);

            newVertex1.Update(newVertex1, null);
            newVertex2.Update(newVertex2, null);
            render.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            controller.FaceLayer.UpdateLeaves();
        }
        #endregion

       
    }
}
