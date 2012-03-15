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
                if (!FirstCut())
                    return null;
            }
            else
            {
                // 不是第一次折叠
                // 判断本次是否切割了一个新的面
                if (JudgeCutAnotherFace())
                {
                    if (!UndoLastCutAndDoFolding())
                        return null;
                }
                else
                {
                    if (JudgeCutAnotherEdge())
                    {
                        if (!UndoLastCutAndDoFolding())
                            return null;
                    }
                    else
                    {
                        //MoveToNewPosition();
                        MoveToANewPosition();
                        lastFoldingLine = foldingLine;
                        lastProjectionPoint = projectionPoint;
                    }
                }
            }

            // 更新重绘
            cloverController.RenderController.UpdateAll();

            return foldingLine;

            //return FoldingUpToAPoint();
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

        public void ExitFoldingMode()
        {
            // 更新Group
            cloverController.FaceGroupLookupTable.UpdateTableAfterFoldUp();
            // 反重叠
            cloverController.RenderController.AntiOverlap();
            // 添加折线
            // 释放资源
        }

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

        private void RecordCuttedEdges()
        {
            // 记录本次切割所切的两条边
            lastCuttedEdges.Clear();
            foreach (Edge e in pickedFace.Edges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
            }
            return;
        }

        /// <summary>
        /// 移动所有点到新的位置
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        private void MoveToNewPosition()
        {
            // 和折线共边的那个点
            Vertex currentVertex = cloverController.VertexLayer.GetVertex(pickedVertex.Index);
            Vector3D vectorFromLastFoldingLineToOrginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();
            Edge edgeForCalculate = null;
            bool seqForCalculate = true; // 如果是true，则后面为vertex2 减去 vertex 1, 为false则相反

            Face pickedFaceAfterCutting = null;
            List<Face> lastTimeMovedFace = cloverController.FoldingSystem.GetLastTimeMovedFace();
            // 从上次移动的面中找到带有选中点的那个面
            foreach (Face face in lastTimeMovedFace)
            {
                foreach (Vertex v in face.Vertices)
                {
                    if (v == currentVertex)
                    {
                        pickedFaceAfterCutting = face;
                        break;
                    }
                }
                if (pickedFaceAfterCutting != null)
                    break;
            }

            // 根据选中面计算平移和旋转矩阵
            // 此处需要记录计算向量所用的两个顶点，不然在三角形的时候是没有办法判断的，之前就悲剧的出问题了，囧！！！
            foreach (Vertex v in pickedFaceAfterCutting.Vertices)
            {
                if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
                {
                    foreach (Edge e in pickedFaceAfterCutting.Edges)
                    {
                        // 计算原来选中点和折线连线的那条边的向量
                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                             CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = true;
                            break;
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = false;
                            break;
                        }
                    }
                    v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                    v.Moved = true;
                    // 重新计算纹理坐标
                    foreach (Edge e in pickedFace.Edges)
                    {
                        if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
                        {
                            cloverController.FoldingSystem.CalculateTexcoord(v, e);
                            break;
                        }
                    }
                }
                if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                {
                    foreach (Edge e in pickedFaceAfterCutting.Edges)
                    {
                        // 计算原来选中点和折线连线的那条边的向量
                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                             CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = true;
                            break;
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = false;
                            break;
                        }
                    }
                    v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                    v.Moved = true;
                    // 重新计算纹理坐标
                    foreach (Edge e in pickedFace.Edges)
                    {
                        if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
                        {
                            cloverController.FoldingSystem.CalculateTexcoord(v, e);
                            break;
                        }
                    }
                }

            }

            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (e == edgeForCalculate)
                {
                    if (seqForCalculate)
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
                    }
                    else
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex2.GetPoint3D();
                    }
                }
            }

            //cloverController.RenderController.Update(face);
            // 求旋转和平移
            // 取得移动点和之前折线点的向量,要求折线点一定是和移动点共边的那个点

            // 求得旋转量,并创建旋转矩阵
            Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            axis.Normalize();
            axis.Negate();
            double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
            RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
            rotateTransform.CenterX = projectionPoint.X;
            rotateTransform.CenterY = projectionPoint.Y;
            rotateTransform.CenterZ = projectionPoint.Z;

            // 创建平移矩阵
            Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
            TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

            // 选定面的移动
            foreach (Vertex v in pickedFaceAfterCutting.Vertices)
            {
                if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) &&
                    !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex2.GetPoint3D()))
                {
                    if (v.Moved == false)
                    {
                        v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                        v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                        v.Moved = true;
                    }
                }
            }

            // 对于上次移动的面除了折线点，其他点做旋转和平移操作
            foreach (Face face in lastTimeMovedFace)
            {
                if (face != pickedFaceAfterCutting)
                {
                    foreach (Vertex v in face.Vertices)
                    {
                        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
                        {
                            if (!v.Moved)
                            {
                                // 对于折线点要计算新的纹理坐标
                                v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                                foreach (Edge edge in face.Edges)
                                {
                                    if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.0001))
                                        cloverController.FoldingSystem.CalculateTexcoord(v, edge);
                                }
                                v.Moved = true;
                            }
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                        {
                            if (!v.Moved)
                            {
                                v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                                foreach (Edge edge in face.Edges)
                                {
                                    if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.0001))
                                        cloverController.FoldingSystem.CalculateTexcoord(v, edge);
                                }
                                v.Moved = true;
                            }
                        }

                        if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()) &&
                            !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                        {
                            if (v.Moved == false)
                            {
                                v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                                v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                                v.Moved = true;
                            }
                        }
                    }
                }
            }

            foreach (Vertex v in cloverController.VertexLayer.Vertices)
            {
                v.Moved = false;
            }
            //foreach (Face face in cloverController.FoldingSystem.GetLastTimeMovedFace())
            //{
            //    // 移动折线到新的位置
            //    foreach (Vertex v in face.Vertices)
            //    {
            //        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
            //        {
            //            foreach (Edge e in face.Edges)
            //            {
            //                // 计算原来选中点和折线连线的那条边的向量
            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
            //                     CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
            //                    break;
            //                }

            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
            //                    CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
            //                    break;
            //                }
            //            }
            //            v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
            //            // 重新计算纹理坐标
            //            foreach (Edge e in pickedFace.Edges)
            //            {
            //                if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
            //                {
            //                    cloverController.FoldingSystem.CalculateTexcoord(v, e);
            //                    break;
            //                }
            //            }
            //        }
            //        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
            //        {
            //            foreach (Edge e in face.Edges)
            //            {
            //                // 计算原来选中点和折线连线的那条边的向量
            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
            //                     CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
            //                    break;
            //                }

            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
            //                    CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
            //                    break;
            //                }
            //            }
            //            v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
            //            // 重新计算纹理坐标
            //            foreach (Edge e in pickedFace.Edges)
            //            {
            //                if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
            //                {
            //                    cloverController.FoldingSystem.CalculateTexcoord(v, e);
            //                    break;
            //                }
            //            }
            //        }

            //    }
            //    foreach (Edge e in face.Edges)
            //    {
            //        if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //        {
            //            vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
            //            break;
            //        }

            //        if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //        {
            //            vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex2.GetPoint3D();
            //            break;
            //        }
            //    }

            //    //cloverController.RenderController.Update(face);
            //    // 求旋转和平移
            //    // 取得移动点和之前折线点的向量,要求折线点一定是和移动点共边的那个点

            //    // 求得旋转量,并创建旋转矩阵
            //    Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            //    axis.Normalize();
            //    axis.Negate();
            //    double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            //    AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
            //    RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
            //    rotateTransform.CenterX = projectionPoint.X;
            //    rotateTransform.CenterY = projectionPoint.Y;
            //    rotateTransform.CenterZ = projectionPoint.Z;

            //    // 创建平移矩阵
            //    Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
            //    TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

            //    // 对于选定面除了折线点，其他点做旋转和平移操作
            //    foreach (Vertex v in face.Vertices)
            //    {
            //        if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) &&
            //            !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex2.GetPoint3D()))
            //        {
            //            v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
            //            v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
            //        }
            //    }
            //}
        }

        private bool MoveToANewPosition()
        {
            // 用于计算矩阵的两个关键向量
            Vector3D vectorFromLastFoldingLineToOriginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();

            // 更新当前选中点到最新的状态
            Vertex currentVertex = cloverController.VertexLayer.GetVertex(pickedVertex.Index);

            // 从上次移动的面中找到带有选中点的那个面
            Face pickedFaceAfterCutting = null;
            List<Face> lastTimeMovedFace = cloverController.FoldingSystem.GetLastTimeMovedFace();

            // 从上次移动的面中找到带有选中点的那个面
            foreach (Face face in lastTimeMovedFace)
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
            foreach (Face face in lastTimeMovedFace)
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
                Edge currentFoldingLineForThisFace = cloverController.FoldingSystem.GetFoldingLineOnAFace(face, foldingLine);
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
            foreach (Face face in lastTimeMovedFace)
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
            foreach (Face face in lastTimeMovedFace)
            {
                foreach (Vertex v in face.Vertices)
                {
                    v.Moved = false;
                }
            }

            return true;
        }


        private bool UndoLastCutAndDoFolding()
        {
            //撤消之前的折叠
            cloverController.ShadowSystem.Undo();
            cloverController.ShadowSystem.Undo();
            cloverController.ShadowSystem.CheckUndoTree();

            if (!cloverController.FoldingSystem.ProcessFoldingUp(pickedFace, pickedVertex, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割所切的两条边
            RecordCuttedEdges();
            lastFoldingLine = foldingLine;
            lastProjectionPoint = projectionPoint;
            return true;
        }
        private bool FirstCut()
        {
            if (!cloverController.FoldingSystem.ProcessFoldingUp(pickedFace, pickedVertex, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割的边
            RecordCuttedEdges();
            lastFoldingLine = foldingLine;
            isFirstCut = false;
            lastProjectionPoint = projectionPoint;
            return true;
        }
        private void SaveOriginFaces(List<Face> foldingFaces)
        {
            // 如果是第一次折叠，记录当前的面表
            foreach (Face face in foldingFaces)
            {
                originFaces.Add(face.Clone() as Face);
            }
        }
        #endregion

    }
}
