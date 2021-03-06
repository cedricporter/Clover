﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;
using System.Windows.Media.Media3D;


namespace Clover
{
    class FoldingUpKid
    {

        #region 成员变量

        CloverController cloverController;
        FaceGroup group;
        Vertex pickedVertex;
        Face baseFace;
        List<Face> facesAboveBase = new List<Face>();
        List<Face> facesBelowBase = new List<Face>();
        List<Face> facesWithFoldLine = new List<Face>();
        List<Face> facesWithoutFoldLine = new List<Face>();
        List<Edge> newEdges = new List<Edge>();
        Point3D originPoint;
        Point3D projectionPoint;
        Edge currFoldLine = null;
        Edge lastFoldLine = null;

        #endregion

        #region 进入折叠模式

        public void EnterFoldingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.cloverController = CloverController.GetInstance();

            // 寻找同group面中拥有pickedVertex的面中最下面的那个面作为baseFace
            this.group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace);
            if (this.group == null)
            {
                System.Windows.MessageBox.Show("找不到Groups");
                return;
            }
            this.pickedVertex = pickedVertex;
            this.baseFace = nearestFace;
            foreach (Face face in group.GetFaceList())
            {
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer < baseFace.Layer)
                    baseFace = face;
            }

            // 将同Group的面分为在base面之上（含baseFace）和base面之下的两组
            foreach (Face face in group.GetFaceList())
            {
                if (face.Layer >= baseFace.Layer)
                    this.facesAboveBase.Add(face);
                else
                    this.facesBelowBase.Add(face);
            }

            // 保存pickedVertex的原始位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
        }

        #endregion

        #region 在折叠模式中
        
        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;
            facesWithFoldLine.Clear();

            // todo 
            // 这里还应该有个判断条件，当facesAboveBase中的面有不同group的相邻面时，不可进行FoldingUp

            // 第一步，作折线
            if (CloverMath.IsTwoPointsEqual(originPoint, projectionPoint))
                return null;
            Point3D p1 = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            Point3D p2 = new Point3D(projectionPoint.X, projectionPoint.Y, projectionPoint.Z);
            CloverMath.GetPerpendicularBisector(ref p1, ref p2, group.Normal);
            currFoldLine = new Edge(new Vertex(p1), new Vertex(p2));
            if (lastFoldLine == null)
                lastFoldLine = currFoldLine;

            // 第二步，在facesAboveBase中找出所有被折线切过的面。如果没有任何面被切过，则判定失败。
            foreach (Face face in facesAboveBase)
            {
                if (CloverTreeHelper.IsEdgeCrossedFace(face, currFoldLine))
                    facesWithFoldLine.Add(face);
            }
            if (facesWithFoldLine.Count == 0)
                return null;

            // 第三步，对facesWithFoldLine中的面进行逐个判定，只要有一个面判定失败，则判定失败。
            // 不成立条件二：投隐点和原始点的连线以及折线有穿过与该面不同组的面
            //foldingLine = CloverMath.GetPerpendicularBisector3D(originFace, originPoint, projectionPoint);
            ////foldingLine = cloverController.FoldingSystem.GetFoldingLine(originFace, originPoint, projectionPoint);
            //if (foldingLine == null)
            //    return false;

            ////找到所有与该面不同组的面
            //List<Face> facesInDifferentGroup = cloverController.FaceGroupLookupTable.GetFaceExcludeGroupFoundByFace(pickedFace);
            //foreach (Face face in facesInDifferentGroup)
            //{
            //    // 求线段和面的交点 
            //    Point3D crossPoint = new Point3D();
            //    if (CloverMath.IntersectionOfLineAndFace(originPoint, projectionPoint, face, ref crossPoint))
            //    {
            //        if (CloverMath.IsPointInTwoPoints(crossPoint, originPoint, projectionPoint, 0.0001))
            //            return false;
            //    }

            //    if (CloverMath.IntersectionOfLineAndFace(foldingLine.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D(),
            //                                            face, ref crossPoint))
            //    {
            //        if (CloverMath.IsPointInTwoPoints(crossPoint, foldingLine.Vertex1.GetPoint3D(),
            //                                            foldingLine.Vertex2.GetPoint3D(), 0.0001))
            //            return false;
            //    }
            //}

            // 第四步，求currFoldLine与baseFace的交点
            Edge returnEdge = CloverTreeHelper.GetEdgeCrossedFace(baseFace, currFoldLine);

            // 第五步，移动两个虚像，造成面已经被切割折叠的假象
            // 该部分已经移至UI层处理

            // 第六步，进入下一个轮回
            lastFoldLine = currFoldLine;
            

            // 第七步，返回值
            return returnEdge;
        }

        /// <summary>
        /// 判定移动的面
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public bool FindFaceWithoutFoldLine()
        {
            // 查找cut完成后所有要移动的面
            List<Face> tempFaces = new List<Face>();
            foreach (Face face in facesWithFoldLine)
            {
                if (face.LeftChild != null)
                    tempFaces.Add(face.LeftChild);
                if (face.RightChild != null)
                    tempFaces.Add(face.RightChild);
            }
            if (tempFaces.Count == 0)
                return false;

            // 从tempFaces中剔除拥有PickedVertex的那些Face

            List<Face> facesWithPickedVertex = CloverTreeHelper.FindFacesFromVertex(tempFaces, pickedVertex);
            tempFaces = tempFaces.Except(facesWithPickedVertex).ToList();

            foreach (Face face in tempFaces)
            {
                foreach (Face faceWPV in facesWithPickedVertex)
                {
                    if (CloverMath.IsIntersectionOfTwoFaceOnOnePlane(face, faceWPV))
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
                            facesWithoutFoldLine.Add(face);
                            break;
                        }
                    }
                }
            }

            foreach (Face face in facesWithPickedVertex)
            {
                if (!facesWithoutFoldLine.Contains(face))
                    facesWithoutFoldLine.Add(face);
            }

            return true;
        }

        #endregion

        #region 退出折叠模式

        public void ExitFoldingMode()
        {
            // 应用折叠
            //Edge foldLine = CloverTreeHelper.GetEdgeCrossedFace(baseFace, currFoldLine);
            if (currFoldLine == null)
                return;
            newEdges = cloverController.CutFaces(facesWithFoldLine, currFoldLine);
            FindFaceWithoutFoldLine();
            cloverController.RotateFaces(facesWithoutFoldLine, currFoldLine, 180);

            // 添加折线
            if (newEdges.Count != 0)
            {
                foreach (Edge edge in newEdges)
                {
                    cloverController.RenderController.AddFoldingLine(edge.Vertex1.u, edge.Vertex1.v, edge.Vertex2.u, edge.Vertex2.v);
                }
            }

            // 更新组
            //cloverController.FaceGroupLookupTable.UpdateTableAfterFoldUp();


            // 释放资源
            facesAboveBase.Clear();
            facesBelowBase.Clear();
            facesWithFoldLine.Clear();
            facesWithoutFoldLine.Clear();
            newEdges.Clear();
            lastFoldLine = null;
        }

        #endregion
    }
}
