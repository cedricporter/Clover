using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover.AbstractLayer;

namespace Clover
{
    public class FoldingUp
    {
        CloverController cloverController;
        List<Face> foldingFaces = new List<Face>();
        List<Face> movingFaces = new List<Face>();
        Edge lastFoldingLine = null;
        List<Edge> lastCuttedEdges = new List<Edge>();
        List<Face> originFaces = new List<Face>();

        bool isFirstCut = true;
        FaceGroup group;
        Face nearestFace;
        Face pickedFace;
        Face originFace;
        Vertex pickedVertex;
        Point3D originPoint;
        Point3D lastProjectionPoint;
        Point3D projectionPoint;
        Edge foldingLine;

        ///// <summary>
        ///// 构造函数
        ///// </summary>
        //public FoldingUp()
        //{
        //    this.cloverController = CloverController.GetInstance();
        //}

        #region 进入折叠模式

        public void EnterFoldingMode(Face nearestFace, Vertex pickedVertex)
        {
            this.cloverController = CloverController.GetInstance();

            // 修订选中的面为拥有选定点的同层面中最下面的那个面
            group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace);
            List<Face> faceList = group.GetFaceList();

            // 记录选中的面和选中的点
            this.nearestFace = nearestFace;
            this.pickedFace = nearestFace;
            this.pickedVertex = pickedVertex;
            // 找到最下层的面为pickedFace，最上层的face为nearestFace
            foreach (Face face in faceList)
            {
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer < pickedFace.Layer)
                    pickedFace = face;
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer > this.nearestFace.Layer)
                    this.nearestFace = face;
            }
            // 拷贝一份pickedFace作为originFace
            this.originFace = pickedFace.Clone() as Face;

            // 记录初始点位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
            lastProjectionPoint = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            isFirstCut = true;
        }

        #endregion

        #region 执行折叠

        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;

            // 判定FoldingUp的成立条件，若成立则进行FoldingUp，若不成立返回null
            if (!DetermineFoldingUpConditionEstablished() || foldingLine == null)
                return null;

            // 是否是第一次折叠
            if (isFirstCut)
            {
                // 不用进行任何Undo操作，直接进行
                isFirstCut = false;
                if (!DoFolding())
                    return null;
            }
            else
            {
                // 不是第一次折叠
                // 判断本次是否切割了一个新的面或新的边
                if (JudgeCutAnotherFace() || JudgeCutAnotherEdge())
                {
                    UndoLastCut();
                    if (!DoFolding())
                        return null;
                }
                else
                {
                    cloverController.FoldingSystem.MoveToANewPosition(pickedVertex,
                        lastTimeMovedFaces, lastFoldingLine, foldingLine, projectionPoint, lastProjectionPoint);
                    lastFoldingLine = foldingLine;
                    lastProjectionPoint = projectionPoint;
                    cloverController.RenderController.UpdateAll();
                }
            }

            // 更新重绘
            // todo 这个将被移到下一层
            //cloverController.RenderController.UpdateAll();

            return foldingLine;
        }

        /// <summary>
        /// 测试FoldingUp成立条件
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="originVertex"></param>
        /// <param name="projectionPoint"></param>
        /// <param name="foldingLine"></param>
        /// <returns></returns>
        private bool DetermineFoldingUpConditionEstablished()
        {
            // 不成立条件一：投影点和原始点是同一个点
            if (originPoint == projectionPoint)
                return false;

            // 不成立条件二：投隐点和原始点的连线以及折线有穿过与该面不同组的面
            foldingLine = CloverMath.GetPerpendicularBisector3D(originFace, originPoint, projectionPoint);
            //foldingLine = cloverController.FoldingSystem.GetFoldingLine(originFace, originPoint, projectionPoint);
            if (foldingLine == null)
                return false;

            //找到所有与该面不同组的面
            List<Face> facesInDifferentGroup = cloverController.FaceGroupLookupTable.GetFaceExcludeGroupFoundByFace(pickedFace);
            foreach (Face face in facesInDifferentGroup)
            {
                // 求线段和面的交点 
                Point3D crossPoint = new Point3D();
                if (CloverMath.IntersectionOfLineAndFace(originPoint, projectionPoint, face, ref crossPoint))
                {
                    if (CloverMath.IsPointInTwoPoints(crossPoint, originPoint, projectionPoint, 0.0001))
                        return false;
                }

                if (CloverMath.IntersectionOfLineAndFace(foldingLine.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D(),
                                                        face, ref crossPoint))
                {
                    if (CloverMath.IsPointInTwoPoints(crossPoint, foldingLine.Vertex1.GetPoint3D(),
                                                        foldingLine.Vertex2.GetPoint3D(), 0.0001))
                        return false;
                }
            }

            return true;
        }

        #endregion

        #region 退出折叠模式

        public void ExitFoldingMode()
        {
            // 更新Group
            cloverController.FaceGroupLookupTable.UpdateTableAfterFoldUp();
            // 反重叠
            cloverController.RenderController.AntiOverlap();
            // 添加折线
            // 释放资源
        }

        #endregion

        #region 与FoldingUp相关

        /// <summary>
        /// 判断本次拖拽是否有切割新的面
        /// </summary>
        /// <param name="foldingFaces"></param>
        /// <param name="originVertex"></param>
        /// <param name="projetionPoint"></param>
        /// <returns></returns>
        private bool JudgeCutAnotherFace()
        {
            // 取出比选顶点离屏幕最近面层数还要高的面
            List<Face> group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace).GetFaceList();
            List<Face> upperFaces = new List<Face>();
            foreach (Face face in group)
            {
                if (face.Layer > nearestFace.Layer)
                    upperFaces.Add(face);
            }
            if (upperFaces.Count == 0)
                return false;

            if (foldingLine == null)
                return false;

            // 判断是否有切割新的面
            foreach (Face face in upperFaces)
            {
                // 求线段和面的交点 
                Point3D crossPoint = new Point3D();
                foreach (Edge edge in face.Edges)
                {
                    if (1 == CloverMath.GetIntersectionOfTwoSegments(edge, foldingLine, ref crossPoint))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断是否有切割其他的边
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        /// <returns></returns>
        private bool JudgeCutAnotherEdge()
        {
            bool findVertex1 = false;
            bool findVertex2 = false;
            foreach (Edge e in lastCuttedEdges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    findVertex1 = true;
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    findVertex2 = true;
            }

            if (findVertex1 && findVertex2)
                return false;
            return true;
        }

        /// <summary>
        /// 记录本次切割所切的两条边
        /// </summary>
        private void RecordCuttedEdges()
        {
            lastCuttedEdges.Clear();
            foreach (Edge e in pickedFace.Edges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
            }
        }

        /// <summary>
        /// 撤消之前的折叠
        /// </summary>
        void UndoLastCut()
        {
            cloverController.ShadowSystem.Undo();
            cloverController.ShadowSystem.Undo();
            cloverController.ShadowSystem.CheckUndoTree();
        }

        /// <summary>
        /// 执行折叠
        /// </summary>
        /// <returns></returns>
        bool DoFolding()
        {
            if (!PresentFoldingUp())
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割所切的两条边
            RecordCuttedEdges();
            lastFoldingLine = foldingLine;
            lastProjectionPoint = projectionPoint;
            return true;
        }

        #endregion

        #region 有关折叠的函数

        List<Face> lastTimeMovedFaces;
        List<Face> facesWithFoldingLine = new List<Face>();
        List<Face> facesWithoutFoldingLine = new List<Face>();
        List<Edge> newEdges;

        /// <summary>
        /// 以拖动点的方式执行FoldingUp
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="pickedVertex"></param>
        /// <param name="projectionPoint"></param>
        public bool PresentFoldingUp()
        {
            ShadowSystem shadowSystem = cloverController.ShadowSystem;

            // 查找所有需要分割的面
            FindFacesWithFoldLine();

            // 向下层传递，割面并拍照
            SnapshotNode node = cloverController.SnapshotBeforeCut();
            newEdges = cloverController.FoldingSystem.CutFaces(facesWithFoldingLine, foldingLine);
            cloverController.SnapshotAfterCut(node, newEdges);

            // 查找所有需要移动的面
            FindFaceWithoutFoldLine();
            lastTimeMovedFaces = facesWithoutFoldingLine;

            // 向下层传递，旋转面
            // 这里应该根据纸张是正面还是背面自动判断是旋转180度还是-180度
            // todo
            cloverController.FoldingSystem.RotateFaces(lastTimeMovedFaces, foldingLine, 180);

            return true;
        }

        /// <summary>
        /// 测试要移动的面
        /// </summary>
        /// <param name="face">待测试的面</param>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="pickedVertex">选中的点</param>
        /// <returns></returns>
        public void FindFacesWithFoldLine()
        {
            facesWithFoldingLine.Clear();
            facesWithoutFoldingLine.Clear();

            // 选中的面一定是移动的面
            facesWithFoldingLine.Add(pickedFace);

            // 查找同层面中有折线跨过的那些面
            foreach (Face face in group.GetFaceList())
            {
                if (face.Layer > pickedFace.Layer)
                {
                    if (CloverTreeHelper.IsEdgeCrossedFace(face, foldingLine))
                    {
                        facesWithFoldingLine.Add(face);
                    }
                }
            }

            #region 在不同组中找到所有和移动部分有联系的面

            //// 在不同组中除了折线以外所有和移动部分有联系的面
            //List<Face> faceNotInSameGroup = cloverController.FaceGroupLookupTable.GetFaceExcludeGroupFoundByFace(pickedFace);
            //if (faceNotInSameGroup.Count == 0)
            //    return;

            //// 首先找到折线所在的两条边,并记录下这两条边的4个顶点
            //List<Vertex> foldingLineVertexList = new List<Vertex>();
            //foreach (Edge e in pickedFace.Edges)
            //{
            //    if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
            //    {
            //        foldingLineVertexList.Add(e.Vertex1);
            //        foldingLineVertexList.Add(e.Vertex2);
            //    }
            //}

            //// 从其中的一个顶点出发，找到所有的边，直到遇到另外一个顶点
            //List<Vertex> PartOfVertices = new List<Vertex>();
            //int index = pickedFace.Vertices.IndexOf(foldingLineVertexList[0]);
            //for (int i = 0; i < pickedFace.Vertices.Count(); i++)
            //{
            //if (i + index > pickedFace.Vertices.Count())
            //{
            //    index = -i;
            //}

            //    if (foldingLineVertexList.Contains(pickedFace.Vertices[i]))
            //        break;

            //    PartOfVertices.Add(pickedFace.Vertices[i]);
            //}

            //List<Vertex> verticesIncludedPickedVertex = null;
            //// 如果该部分顶点包含选中点，则选取该部分顶点作为迭代条件
            //if (PartOfVertices.Contains(pickedVertex))
            //{
            //    verticesIncludedPickedVertex = PartOfVertices;
            //}
            //else
            //{
            //    verticesIncludedPickedVertex = (pickedFace.Vertices.Except(foldingLineVertexList).ToList()).Except(PartOfVertices).ToList();
            //}

            //// 对于包含选中点的点集，迭代的查找所有包含该点集的面的集合
            //foreach (Vertex v in verticesIncludedPickedVertex)
            //{
            //    foreach (Face face in faceNotInSameGroup)
            //    {
            //        if (face.Vertices.Contains(v))
            //        {
            //            facesWithoutFoldingLine.Add(face);
            //        }
            //    }
            //}
            #endregion

        }

        /// <summary>
        /// 判定移动的面
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public void FindFaceWithoutFoldLine()
        {


            // 查找cut完成后所有要移动的面
            List<Face> tempFaces = new List<Face>();
            foreach (Face face in facesWithFoldingLine)
            {
                tempFaces.Add(face.LeftChild);
                tempFaces.Add(face.RightChild);
            }

            // 从tempFaces中剔除拥有PickedVertex的那些Face

            List<Face> facesWithPickedVertex = CloverTreeHelper.FindFacesFromVertex(tempFaces, pickedVertex);
            tempFaces = tempFaces.Except(facesWithPickedVertex).ToList();

            foreach (Face face in tempFaces)
            {
                foreach (Face faceWPV in facesWithPickedVertex)
                {
                    if (CloverMath.IsIntersectionOfTwoFace(face, faceWPV))
                    {
                        bool isClosed = false;
                        // 要除去折线的所有边
                        foreach (Edge e in faceWPV.Edges)
                        {
                            //if (e.Face1 == face || e.Face2 == face)
                            if ((e.Face1 == face || e.Face2 == face) && !newEdges.Contains(e))
                            {
                                isClosed = true;
                                break;
                            }
                        }
                        if (isClosed)
                        {
                            facesWithoutFoldingLine.Add(face);
                            break;
                        }
                    }
                }
            }

            foreach (Face face in facesWithPickedVertex)
            {
                if (!facesWithoutFoldingLine.Contains(face))
                    facesWithoutFoldingLine.Add(face);
            }
        }


        #endregion

    }
}
