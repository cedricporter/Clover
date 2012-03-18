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
        #region 有关纹理计算的函数

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

        #endregion

        #region 分割面

        /// <summary>
        /// 切割一个面为两个面
        /// </summary>
        /// <param name="face">要切割的面</param>
        /// <param name="edge">割线，不一定要在边层里面的边，只要两个端点的坐标在面的边上即可</param>
        /// <returns>被边引用的割线</returns>
        Edge CutFace(Face face, Edge edge)
        {
            Debug.Assert(edge != null);
            if (edge == null)
            {
                throw new System.ArgumentNullException();
            }

            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            //ShadowSystem shadowSystem = controller.ShadowSystem;

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
                if (
                    (
                    !CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[1].GetPoint3D())
                    && CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[0].GetPoint3D(), vertexList[1].GetPoint3D(), 0.001)
                    )
                    ||
                    (
                     !CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[1].GetPoint3D())
                    && CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[0].GetPoint3D(), vertexList[1].GetPoint3D(), 0.001)
                    )
                    )
                {
                    break;
                }
                vertexList.Add(vertexList[0]);
                vertexList.RemoveAt(0);

                // 防止死循环
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

            // 分割出来的子面
            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            face.LeftChild = f1;
            face.RightChild = f2;

            List<Edge> currentEdgeList = null;
            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                Edge currentEdge = CloverTreeHelper.FindEdge(face, vertexList[i], vertexList[i + 1]);

                // 割线过顶点
                if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D()))
                {
                    currentEdgeList = rangeA;
                    newVertex1 = vertexList[i];
                }
                else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D()))
                {
                    currentEdgeList = rangeB;
                    newVertex2 = vertexList[i];
                }
                // 割线过边
                else if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001)
                    && !CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[i + 1].GetPoint3D()))
                {
                    currentEdgeList = rangeA;

                    beCutEdge1 = currentEdge;
                    Edge cutEdge1 = null;
                    Edge cutEdge2 = null;

                    // 两个孩子为空，新建两条边

                    if (beCutEdge1.LeftChild == null && beCutEdge1.RightChild == null)
                    {
                        // 没人用过的共边
                        Debug.Assert(newVertex1 != null);
                        // 分割一条边生成两条新的边
                        cutEdge1 = new Edge(vertexList[i], newVertex1);
                        cutEdge2 = new Edge(newVertex1, vertexList[i + 1]);
                        beCutEdge1.LeftChild = cutEdge1;
                        beCutEdge1.RightChild = cutEdge2;
                        rangeB.Add(cutEdge1);
                        rangeA.Add(cutEdge2);

                        cutEdge1.Face1 = f2;
                        cutEdge2.Face1 = f1;
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
                            return null;
                        }

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeB.Add(beCutEdge1.LeftChild);
                            rangeA.Add(beCutEdge1.RightChild);

                            beCutEdge1.LeftChild.Face2 = f2;
                            beCutEdge1.RightChild.Face2 = f1;
                        }
                        else
                        {
                            rangeA.Add(beCutEdge1.LeftChild);
                            rangeB.Add(beCutEdge1.RightChild);

                            beCutEdge1.LeftChild.Face2 = f1;
                            beCutEdge1.RightChild.Face2 = f2;
                        }
                    }

                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex1, beCutEdge1);
                   
                    continue;
                }
                else if (CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001)
                    && !CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[i + 1].GetPoint3D()))
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

                        cutEdge1.Face1 = f1;
                        cutEdge2.Face1 = f2;
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
                            return null;
                        }

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeA.Add(beCutEdge2.LeftChild);
                            rangeB.Add(beCutEdge2.RightChild);

                            beCutEdge2.LeftChild.Face2 = f1;
                            beCutEdge2.RightChild.Face2 = f2;
                        }
                        else
                        {
                            rangeB.Add(beCutEdge2.LeftChild);
                            rangeA.Add(beCutEdge2.RightChild);

                            beCutEdge2.LeftChild.Face2 = f2;
                            beCutEdge2.RightChild.Face2 = f1;
                        }
                    }
                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex2, beCutEdge2);
                    continue;
                }
                currentEdgeList.Add(currentEdge);
            }

            Edge newEdge = new Edge(newVertex1, newVertex2);

            controller.EdgeLayer.AddTree(new EdgeTree(newEdge));

            // 是否是新的顶点
            if (newVertex1 == newVertexOld1)
            {
                vertexLayer.InsertVertex(newVertex1);
                render.AddVisualInfoToVertex(newVertex1);
            }
            if (newVertex2 == newVertexOld2)
            {
                vertexLayer.InsertVertex(newVertex2);
                render.AddVisualInfoToVertex(newVertex2);
            }


            // 更新新边的Faces指针 
            newEdge.Face1 = f1;
            newEdge.Face2 = f2;

            // 给两个新面加新的边
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

            // 更新两个新面的法向量标杆
            f1.StartVertex1 = newVertex2;
            f1.StartVertex2 = newVertex1;

            f2.StartVertex1 = newVertex1;
            f2.StartVertex2 = newVertex2;

            // 更新面的都为顶点的顺序
            f1.UpdateVertices();
            f2.UpdateVertices();

            // 更新渲染层的部分
            render.Delete(face);
            render.New(f1);
            render.New(f2);

            controller.FaceLayer.UpdateLeaves();

            return newEdge;
        }

        /// <summary>
        /// 切割多个面
        /// </summary>
        /// <param name="faceList">要切割的面的列表</param>
        /// <param name="foldingLine">分割线</param>
        /// <returns>在每个面上的折线</returns>
        public List<Edge> CutFaces(List<Face> faceList, Edge foldingLine)
        {
            // 切割面
            List<Edge> newEdges = new List<Edge>();
            foreach (Face face in faceList)
            {
                Edge edge = CloverTreeHelper.GetEdgeCrossedFace(face, foldingLine);

                Debug.Assert(edge != null);

                newEdges.Add(CutFace(face, edge));
                
            }

            return newEdges;
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
        public List<Vertex> RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            RenderController render = CloverController.GetInstance().RenderController;
            FaceGroupLookupTable table = CloverController.GetInstance().FaceGroupLookupTable;

            List<Vertex> movedVertexList = new List<Vertex>();
            shadowSystem.CheckUndoTree();

            Dictionary<int, bool> movedVertexDict = new Dictionary<int, bool>();
            foreach (Face f in beRotatedFaceList)
            {
                //foreach (Edge e in f.Edges)
                //{
                    foreach (Vertex v in f.Vertices)
                        movedVertexDict[v.Index] = false;
                //}
            }

            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    if (!CloverTreeHelper.IsVertexInEdge(e.Vertex1, foldingLine) && !movedVertexDict[e.Vertex1.Index] )
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

                        movedVertexDict[e.Vertex1.Index] = true; 
                        movedVertexList.Add(e.Vertex1);
                    }

                    if (!CloverTreeHelper.IsVertexInEdge(e.Vertex2, foldingLine) && !movedVertexDict[e.Vertex2.Index])
                    {
                        CloneAndUpdateVertex(e.Vertex2);

                        e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);

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

                        e.Vertex2.SetPoint3D(rotateTransform.Transform(e.Vertex2.GetPoint3D()));

                        movedVertexDict[e.Vertex2.Index] = true; 
                        movedVertexList.Add(e.Vertex2); 
                    } 
                }
            }

            // 因为顶点克隆过了，所以所有的面的边都要更新到引用最新的顶点
            foreach (Face f in CloverController.GetInstance().FaceLayer.Leaves)
            {
                CloverTreeHelper.UpdateFaceVerticesToLastedVersion(f);
            }

            // 必须先更新group后更新render
            //table.UpdateLookupTable();
            // 你们在滥用UpdateAll…… ---kid
            //render.UpdateAll();
            foreach (Face face in beRotatedFaceList)
            {
                render.Update(face);
            }

            return movedVertexList;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveToANewPosition(Vertex pickedVertex, List<Face> lastTimeMovedFaces, Edge lastFoldingLine,
            Edge foldingLine, Point3D projectionPoint, Point3D lastProjectionPoint)
        {
            CloverController cloverController = CloverController.GetInstance();

            // 用于计算矩阵的两个关键向量
            Vector3D vectorFromLastFoldingLineToOriginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();

            // 更新当前选中点到最新的状态
            Vertex currentVertex = cloverController.VertexLayer.GetVertex(pickedVertex.Index);

            // 从上次移动的面中找到带有选中点的那个面
            Face pickedFaceAfterCutting = null;

            // 从上次移动的面中找到带有选中点的那个面
            foreach (Face face in lastTimeMovedFaces)
            {
                foreach (Vertex v in face.Vertices)
                {
                    if (v == currentVertex)
                    {
                        if (pickedFaceAfterCutting == null || pickedFaceAfterCutting.Layer > face.Layer)
                            pickedFaceAfterCutting = face;
                        break;
                    }
                }
            }

            if (pickedFaceAfterCutting == null)
                return false;

            // 在选定的面上找到与折线边顶点位置相等的那条边
            Edge foldingLineOnFace = null;
            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (CloverMath.AreTwoEdgesEqual(e, lastFoldingLine))
                    foldingLineOnFace = e;
            }

            if (null == foldingLineOnFace)
                return false;

            // 在找到选顶点与折线的一条连线边，计算出计算角度所需的向量1
            Edge edgeBeforeMoving = null;
            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (e.Vertex1 == currentVertex && e.Vertex2 == foldingLineOnFace.Vertex1)
                {
                    edgeBeforeMoving = e;
                    break;
                }
                if (e.Vertex2 == currentVertex && e.Vertex1 == foldingLineOnFace.Vertex1)
                {
                    edgeBeforeMoving = e;
                    break;
                }
                if (e.Vertex1 == currentVertex && e.Vertex2 == foldingLineOnFace.Vertex2)
                {
                    edgeBeforeMoving = e;
                    break;
                }
                if (e.Vertex2 == currentVertex && e.Vertex1 == foldingLineOnFace.Vertex2)
                {
                    edgeBeforeMoving = e;
                    break;
                }
            }

            // 得到关键向量1
            vectorFromLastFoldingLineToOriginVertex = edgeBeforeMoving.Vertex2.GetPoint3D() - edgeBeforeMoving.Vertex1.GetPoint3D();

            // 在原始面中找到折线顶点所在的两条边，并用这两条边计算折线的纹理坐标
            foreach (Edge e in pickedFaceAfterCutting.Parent.Edges)
            {
                // 判断折线的两个顶点，更新坐标并计算纹理
                // 顶点1
                if (CloverMath.IsPointInTwoPoints(foldingLineOnFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                {
                    cloverController.FoldingSystem.CalculateTexcoord(foldingLineOnFace.Vertex1, e);
                    if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    {
                        foldingLineOnFace.Vertex1.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                        foldingLineOnFace.Vertex1.Moved = true;
                    }
                    else
                    {
                        foldingLineOnFace.Vertex1.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                        foldingLineOnFace.Vertex1.Moved = true;
                    }
                }

                // 顶点2
                if (CloverMath.IsPointInTwoPoints(foldingLineOnFace.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                {
                    cloverController.FoldingSystem.CalculateTexcoord(foldingLineOnFace.Vertex2, e);
                    if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    {
                        foldingLineOnFace.Vertex2.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                        foldingLineOnFace.Vertex2.Moved = true;
                    }
                    else
                    {
                        foldingLineOnFace.Vertex2.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                        foldingLineOnFace.Vertex2.Moved = true;
                    }
                }
            }
            // 设置选中点的新位置为projectinPoint;
            currentVertex.SetPoint3D(projectionPoint);
            currentVertex.Moved = true;

            // 找到选中点和折线之间的向量,得到关键向量2
            vectorFromCurrentFoldingLineToProjVertex = edgeBeforeMoving.Vertex2.GetPoint3D() - edgeBeforeMoving.Vertex1.GetPoint3D();

            // 计算所有上次移动面的折线，对于折线完全相等的直接更新到新的位置，对于不相等的要重新找打对应的边，然后计算折线位置，再更新折线
            foreach (Face face in lastTimeMovedFaces)
            {
                if (face == pickedFaceAfterCutting)
                    continue;

                Edge foldingLineForThisFace = null;
                foreach (Edge e in face.Edges)
                {
                    // 找到与原来折线共线的那条边
                    Point3D crossedPoint = new Point3D();
                    if (2 == CloverMath.GetIntersectionOfTwoSegments(lastFoldingLine, e, ref crossedPoint))
                    {
                        foldingLineForThisFace = e;
                        break;
                    }
                }

                // 假如木有折线，或者折线中的其中一个点和上面的折线拥有相同点返回
                if (foldingLineForThisFace == null)
                {
                    // 采用点更新法 
                    foreach (Edge e in face.Edges)
                    {
                        // 假如其中有一条边的一个顶点等于新折线，而另一个顶点等于旧折线中的点，折更新旧点到新点
                    }
                    continue;
                }

                // 计算现在的折线位置
                Edge currentFoldingLineForThisFace = CloverTreeHelper.GetEdgeCrossedFace(face, foldingLine);
                if (currentFoldingLineForThisFace == null)
                    continue;

                // 通过该面的原始面查询折线并更新折线位置和纹理坐标
                foreach (Edge e in face.Parent.Edges)
                {
                    // 判断折线的两个顶点，更新坐标并计算纹理
                    // 顶点1
                    if (CloverMath.IsPointInTwoPoints(foldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()) &&
                        !foldingLineForThisFace.Vertex1.Moved)
                    {
                        cloverController.FoldingSystem.CalculateTexcoord(foldingLineForThisFace.Vertex1, e);
                        if (CloverMath.IsPointInTwoPoints(currentFoldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                        {
                            foldingLineOnFace.Vertex1.SetPoint3D(currentFoldingLineForThisFace.Vertex1.GetPoint3D());
                            foldingLineOnFace.Vertex1.Moved = true;
                        }
                        else
                        {
                            foldingLineOnFace.Vertex1.SetPoint3D(currentFoldingLineForThisFace.Vertex2.GetPoint3D());
                            foldingLineOnFace.Vertex1.Moved = true;
                        }
                    }

                    // 顶点2
                    if (CloverMath.IsPointInTwoPoints(foldingLineForThisFace.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()) &&
                        !foldingLineForThisFace.Vertex2.Moved)
                    {
                        cloverController.FoldingSystem.CalculateTexcoord(foldingLineForThisFace.Vertex2, e);
                        if (CloverMath.IsPointInTwoPoints(currentFoldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                        {
                            foldingLineOnFace.Vertex2.SetPoint3D(currentFoldingLineForThisFace.Vertex1.GetPoint3D());
                            foldingLineOnFace.Vertex2.Moved = true;
                        }
                        else
                        {
                            foldingLineOnFace.Vertex2.SetPoint3D(currentFoldingLineForThisFace.Vertex2.GetPoint3D());
                            foldingLineOnFace.Vertex2.Moved = true;
                        }
                    }
                }
            }

            // 求得旋转量,并创建旋转矩阵
            Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOriginVertex);
            axis.Normalize();
            axis.Negate();
            double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOriginVertex);
            AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
            RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
            rotateTransform.CenterX = projectionPoint.X;
            rotateTransform.CenterY = projectionPoint.Y;
            rotateTransform.CenterZ = projectionPoint.Z;

            // 创建平移矩阵
            Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
            TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

            // 移动所有没有移动过的顶点
            foreach (Face face in lastTimeMovedFaces)
            {
                foreach (Vertex v in face.Vertices)
                {
                    if (!v.Moved)
                    {
                        v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                        v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                        v.Moved = true;
                    }
                }
            }

            // 最终更改所有顶点移动属性复位
            foreach (Face face in lastTimeMovedFaces)
            {
                foreach (Vertex v in face.Vertices)
                {
                    v.Moved = false;
                }
            }

            // 更新渲染层
            // 也许UpdateAll曾今给了你们许多欢乐的时光，但现在它应该退场了 ---kid
            //cloverController.RenderController.UpdateAll();
            //foreach (Face face in lastTimeMovedFaces)
            //{
            //    cloverController.RenderController.Update(face);
            //}
            // 好吧。。你们还是继续使用UpdateAll好了。。 ---kid
            cloverController.RenderController.UpdateAll();

            return true;
        }



    }
}
