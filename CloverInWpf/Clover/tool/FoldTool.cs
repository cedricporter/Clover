﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows.Controls;
using Clover.Visual;
using System.Windows;
using System.Windows.Input;
using Clover.AbstractLayer;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	Folding工具
**/



namespace Clover.Tool
{
    class FoldTool : ToolFactory
    {
        Face nearestFace = null;
        Vertex pickedVertex = null;
        //FaceGroup currGroup = null;
        List<Vertex> canSelectedVertices = new List<Vertex>();
        List<Face> canSelectedFaces = new List<Face>();
        Point Origin2Dpos = new Point();
        Point3D projectionPoint;
        CurrentModeVisual currentModeVi = null;
        DashLineVisual lineVi = null;
        DashLineVisual foldLineVi = null;
        FoldLinePercentageVisual foldLineInfoVi1 = null;
        FoldLinePercentageVisual foldLineInfoVi2 = null;
        Edge foldLine = null;
        Point projectionPoint2D, foldLinePointA2D, foldLinePointB2D;
        PaperVoid paperVoid;

        enum FoldingMode
        {
            DoingNothing, FoldingUp//, Blending
        }
        FoldingMode mode = FoldingMode.DoingNothing;

        public FoldTool(MainWindow mainWindow)
            : base(mainWindow)
        {
            paperVoid = PaperVoid.GetInstance();
        }

        protected override void onEnterElement(Object element)
        {

        }

        protected override void onLeaveElement(Object element)
        {

        }

        /// <summary>
        /// 选取元素
        /// </summary>
        /// <param name="element"></param>
        protected override void onSelectElement(Object element)
        {

            // 将视角变换到与当前纸张平行
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                //CloverController.GetInstance().ShadowSystem.CheckUndoTree();
                pickedVertex = (Vertex)element;
                // 寻找所有拥有该点的面的集合
                List<Face> faces = CloverTreeHelper.GetReferenceFaces(pickedVertex);
                // 首先寻找离我们最近的那个面……
                // 按照道理nearestFace是不可能为空的
                FindNearestFace(faces);

                // 进入折叠模式，传递给下层
                List<Face> topFaces = CloverController.GetInstance().FoldingUp.EnterFoldingMode(pickedVertex, nearestFace);

                // 锁定视角
                LockViewport(true);
                // 锁定鼠标OnPress和OnMove
                IsOnMoveLocked = true;
                IsOnPressLocked = true;

                RenderController.GetInstance().OnRotationEndOnce += new OnRotationEndHandle(
                    () =>
                    {
                        // 创建虚像……
                        //PaperVoid.CreateShadow(mainWindow.foldingPaperViewport, mainWindow.cloverController.FaceLeaves, null,
                        //        mainWindow.VoidPaperTopImgFront, mainWindow.VoidPaperBgImg, mainWindow.VoidPaperTopImgBack);
                        List<Face> bgFaces = new List<Face>();
                        foreach (Face leave in mainWindow.cloverController.FaceLeaves)
                        {
                            if (topFaces.Contains(leave))
                                continue;
                            bgFaces.Add(leave);
                        }
                        paperVoid.CreateShadow(mainWindow.foldingPaperViewport, topFaces, bgFaces,
                                mainWindow.VoidPaperTopImgFront, mainWindow.VoidPaperBgImg, mainWindow.VoidPaperTopImgBack);
                        RenderController.GetInstance().OnRotationEndOnce = null;
                    }
                    );

                EnterFoldingUp();

            }

        }

        /// <summary>
        /// 鼠标单击，切换模式
        /// </summary>
        protected override void onClick(Boolean isCancled)
        {
            if (mode == FoldingMode.DoingNothing)
                return;

            if (Mouse.RightButton == MouseButtonState.Pressed)
                exit(isCancled);
            else if (Mouse.MiddleButton == MouseButtonState.Pressed)
                CloverController.GetInstance().FoldingUp.CutFoldLine(Get3DProjectionPoint());

        }

        protected override void onUnselectElement(Object element)
        {
        }

        protected override void onOverElement(Object element)
        {

        }

        /// <summary>
        /// 拖动元素
        /// </summary>
        /// <param name="element"></param>
        protected override void onDrag(Object element)
        {

            if (element.GetType().ToString() == "Clover.Vertex")
            {
                if (mode == FoldingMode.FoldingUp)
                {
                    // 求2D到3D的投影点
                    projectionPoint = Get3DProjectionPoint();
                    // 磁性处理
                    Point3D nearestPoint;
                    Boolean isAttached = AbstractLayer.Magnet.PerformVertexAttach(projectionPoint, nearestFace, out nearestPoint);
                    if (isAttached)
                        projectionPoint = nearestPoint;
                    // 传给下一层处理
                    this.foldLine = CloverController.GetInstance().FoldingUp.OnDrag(projectionPoint);
                    // 更新虚像。。。
                    Point outP1 = new Point();
                    Point outP2 = new Point();
                    if (!FineIntersectionOfFoldlineAndViewport(ref outP1, ref outP2))
                        return;
                    paperVoid.UpdateShadow(mainWindow.foldingPaperViewport, outP1, outP2,
                        mainWindow.VoidPaperTopImgFront, mainWindow.VoidPaperBgImg, mainWindow.VoidPaperTopImgBack);


                }

            }

        }

        public override void onIdle()
        {
            if (mode != FoldingMode.DoingNothing)
            {
                // 更新各点的2D坐标 
                Matrix3D to2DMat = Utility.GetInstance().To2DMat;
                Point3D p3d;
                p3d = pickedVertex.GetPoint3D() * to2DMat;
                Origin2Dpos = new Point(p3d.X, p3d.Y);
                p3d = projectionPoint * to2DMat;
                projectionPoint2D = new Point(p3d.X, p3d.Y);
                if (foldLine != null)
                {
                    p3d = foldLine.Vertex1.GetPoint3D() * to2DMat;
                    foldLinePointA2D = new Point(p3d.X, p3d.Y);
                    p3d = foldLine.Vertex2.GetPoint3D() * to2DMat;
                    foldLinePointB2D = new Point(p3d.X, p3d.Y);
                }
                // 蓝点视觉效果
                if (lineVi != null)
                    lineVi.StartPoint = Origin2Dpos;
                if (currOveredElementVi != null)
                {
                    (currOveredElementVi as VertexHeightLightVisual).TranslateTransform.X = Origin2Dpos.X - 5;
                    (currOveredElementVi as VertexHeightLightVisual).TranslateTransform.Y = Origin2Dpos.Y;
                }
                // 红点视觉效果
                (currSelectedElementVi as VertexHeightLightVisual).TranslateTransform.X = projectionPoint2D.X - 5;
                (currSelectedElementVi as VertexHeightLightVisual).TranslateTransform.Y = projectionPoint2D.Y;
                lineVi.EndPoint = projectionPoint2D;
                // 更新折线显示
                UpdateFoldLine();
                // 更新提示信息
                UpdateFoldLineInfo();
            }
        }

        /// <summary>
        /// 寻找离我们最近的那个面
        /// </summary>
        /// <param name="faces"></param>
        void FindNearestFace(List<Face> faces)
        {
            Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;
            Double minVal = Double.MaxValue;

            foreach (Face f in faces)
            {
                Double val = 0;
                int count = 0;
                foreach (Vertex v in f.Vertices)
                {
                    Vector3D vec = new Vector3D(v.GetPoint3D().X, v.GetPoint3D().Y, v.GetPoint3D().Z);
                    vec *= mat;
                    val += vec.Length;
                    count++;
                }
                val /= count;
                if (minVal > val)
                {
                    minVal = val;
                    nearestFace = f;
                }
            }
        }

        /// <summary>
        /// 计算旋转量
        /// </summary>
        /// <returns></returns>
        Quaternion CalculateFoldingUpRotation()
        {
            Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;

            // 判断该面是正面朝向用户还是背面朝向用户
            Vector3D vector1 = nearestFace.Normal * mat;
            Vector3D vector2 = new Vector3D(0, 0, 1);
            if (Vector3D.DotProduct(vector1, vector2) < 0)
                vector1 = nearestFace.Normal * -1;
            else
                vector1 = nearestFace.Normal;
            // 计算旋转
            Quaternion quat;
            if (vector1 == new Vector3D(0, 0, 1))
                quat = new Quaternion();
            else if (vector1 == new Vector3D(0, 0, -1))
                quat = new Quaternion(new Vector3D(0, 1, 0), 180);
            else
            {
                Vector3D axis = Vector3D.CrossProduct(vector1, vector2);
                axis.Normalize();
                Double deg = Vector3D.AngleBetween(vector1, vector2);
                quat = new Quaternion(axis, deg);
            }
            return quat;
        }

        /// <summary>
        /// 锁定或解锁视角
        /// </summary>
        /// <param name="islock"></param>
        void LockViewport(Boolean islock)
        {
            CubeNavigator cubeNav = CubeNavigator.GetInstance();
            Viewport3D cubeNavViewport = cubeNav.CubeNavViewport;
            if (islock)
            {
                cubeNavViewport.PreviewMouseDown -= cubeNav.cubeNavViewport_ButtonDown;
                // mainWindow.MouseMove -= mainWindow.Window_TranslatePaper;
            }
            else
            {
                cubeNavViewport.PreviewMouseDown += cubeNav.cubeNavViewport_ButtonDown;
                // mainWindow.MouseMove += mainWindow.Window_TranslatePaper;
            }
        }

        /// <summary>
        /// 求2D到3D的投影点
        /// 杨旭瑜提供
        /// </summary>
        /// <returns></returns>
        Point3D Get3DProjectionPoint()
        {
            Matrix3D to3DMat = Utility.GetInstance().To2DMat;
            if (!to3DMat.HasInverse)
                return new Point3D(0, 0, 0);
            to3DMat.Invert();
            Point3D start = new Point3D(currMousePos.X, currMousePos.Y, 0.0000001);
            Point3D end = new Point3D(currMousePos.X, currMousePos.Y, 0.9999999);
            start *= to3DMat;
            end *= to3DMat;

            return CloverMath.IntersectionOfLineAndPlane(start, end, nearestFace.Normal, nearestFace.Vertices[0].GetPoint3D());
        }

        /// <summary>
        /// 更新折线
        /// </summary>
        /// <param name="edge"></param>
        void UpdateFoldLine()
        {
            if (foldLine == null)
                return;
            foldLineVi.StartPoint = foldLinePointA2D;
            foldLineVi.EndPoint = foldLinePointB2D;
        }

        /// <summary>
        /// 退出xx模式
        /// </summary>
        public override void exit(Boolean isCancled)
        {
            if (mode == FoldingMode.DoingNothing)
                return;

            mode = FoldingMode.DoingNothing;

            ExitFoldingUp();

            currSelectedElementVi.End();
            currSelectedElementVi = null;
            currOveredElementVi.End();
            currOveredElementVi = null;
            currSelectedElement = currOveredElement = null;
            canSelectedVertices.Clear();
            canSelectedFaces.Clear();
            IsOnMoveLocked = false;
            IsOnPressLocked = false;
            LockViewport(false);

            // 传给下层。。
            if (!isCancled)
                CloverController.GetInstance().FoldingUp.ExitFoldingMode();
            // 显示实像
            //RenderController.GetInstance().Entity.Content = RenderController.GetInstance().ModelGroup;
            // 销毁虚像……
            paperVoid.DestoryShadow(mainWindow.foldingPaperViewport,
                mainWindow.VoidPaperTopImgFront, mainWindow.VoidPaperBgImg, mainWindow.VoidPaperTopImgBack);

            
        }

        /// <summary>
        /// 进入Folding模式
        /// </summary>
        void EnterFoldingUp()
        {
            mode = FoldingMode.FoldingUp;
            // 计算旋转
            Quaternion quat = CalculateFoldingUpRotation();
            // 应用旋转
            RenderController.GetInstance().BeginRotationSlerp(quat);
            // 显示模式
            currentModeVi = new CurrentModeVisual("Folding Up Mode");
            VisualController.GetSingleton().AddVisual(currentModeVi);
            currentModeVi.Start();
            // 显示连线提示
            lineVi = new DashLineVisual(Origin2Dpos, currMousePos, (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush"));
            VisualController.GetSingleton().AddVisual(lineVi);
            lineVi.Start();
            foldLineVi = new DashLineVisual(new Point(0, 0), new Point(0, 0), (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush"));
            VisualController.GetSingleton().AddVisual(foldLineVi);
            foldLineVi.Start();
            // 显示折线提示
            foldLineInfoVi1 = new FoldLinePercentageVisual(new Point(-100, -100), new Point(-100, -100), 0);
            foldLineInfoVi2 = new FoldLinePercentageVisual(new Point(-100, -100), new Point(-100, -100), 0);
            VisualController.GetSingleton().AddVisual(foldLineInfoVi1);
            VisualController.GetSingleton().AddVisual(foldLineInfoVi2);
            foldLineInfoVi1.Start();
            foldLineInfoVi2.Start();
        }

        /// <summary>
        /// 退出Folding模式
        /// </summary>
        void ExitFoldingUp()
        {
            foldLineVi.End();
            foldLineVi = null;
            lineVi.End();
            lineVi = null;
            currentModeVi.End();
            currentModeVi = null;
            foldLineInfoVi1.End();
            foldLineInfoVi1 = null;
            foldLineInfoVi2.End();
            foldLineInfoVi2 = null;
        }

        /// <summary>
        /// 更新折线提示
        /// </summary>
        /// <param name="edge"></param>
        void UpdateFoldLineInfo()
        {
            if (foldLine == null)
                return;
            KeyValuePair<Vertex, Edge> pair1 = new KeyValuePair<Vertex, Edge>();
            KeyValuePair<Vertex, Edge> pair2 = new KeyValuePair<Vertex, Edge>();
            foreach (Edge e in nearestFace.Edges)
            {
                if (pair1.Key == null && CloverMath.IsPointInTwoPoints(foldLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.01))
                    pair1 = new KeyValuePair<Vertex, Edge>(foldLine.Vertex1, e);
                if (pair2.Key == null && CloverMath.IsPointInTwoPoints(foldLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.01))
                    pair2 = new KeyValuePair<Vertex, Edge>(foldLine.Vertex2, e);
            }
            if (pair1.Key == null || pair2.Key == null)
                return;
            Point3D p5, p6, p1, p2, p3, p4;
            p5 = pair1.Key.GetPoint3D();
            p6 = pair2.Key.GetPoint3D();
            p1 = pair1.Value.Vertex1.GetPoint3D();
            p2 = pair1.Value.Vertex2.GetPoint3D();
            p3 = pair2.Value.Vertex1.GetPoint3D();
            p4 = pair2.Value.Vertex2.GetPoint3D();
            Double offset1 = (p1 - p5).Length / (p1 - p2).Length;
            Double offset2 = (p3 - p6).Length / (p3 - p4).Length;

            Matrix3D to2DMat = Utility.GetInstance().To2DMat;
            p1 *= to2DMat;
            p2 *= to2DMat;
            p3 *= to2DMat;
            p4 *= to2DMat;

            foldLineInfoVi1.Point1 = new Point(p1.X, p1.Y);
            foldLineInfoVi1.Point2 = new Point(p2.X, p2.Y);
            foldLineInfoVi2.Point1 = new Point(p3.X, p3.Y);
            foldLineInfoVi2.Point2 = new Point(p4.X, p4.Y);
            foldLineInfoVi1.Offset = offset1;
            foldLineInfoVi2.Offset = offset2;
        }

        ///// <summary>
        ///// 如果有多层存在，将纸张散开以供用户选择
        ///// </summary>
        //void SpreadOut()
        //{
        //    if (canSelectedFaces.Count < 2)
        //        return;
        //    //计算散开的方向
        //    Vector3D dir, v1, v2;
        //    Point3D p0, p1, p2;
        //    p0 = p1 = p2 = new Point3D();
        //    int count = nearestFace.Vertices.Count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        if (pickedVertex.GetPoint3D() == nearestFace.Vertices[i].GetPoint3D())
        //        {
        //            p0 = pickedVertex.GetPoint3D();
        //            p1 = nearestFace.Vertices[(i + count - 1) % count].GetPoint3D();
        //            p2 = nearestFace.Vertices[(i + 1) % count].GetPoint3D();
        //            break;
        //        }
        //    }
        //    v1 = p1 - p0;
        //    v1.Normalize();
        //    v2 = p2 - p0;
        //    v2.Normalize();
        //    dir = v1 + v2;
        //    dir.Normalize();
        //    // 计算散开的位移

        //}

        /// <summary>
        /// 寻找折线与视口的交点
        /// </summary>
        /// <param name="outP1"></param>
        /// <param name="outP2"></param>
        /// <returns>没找全两个，返回false</returns>
        private Boolean FineIntersectionOfFoldlineAndViewport(ref Point outP1, ref Point outP2)
        {
            Vector v = foldLineVi.StartPoint - foldLineVi.EndPoint;
            Point p1 = foldLineVi.StartPoint + v * 10000;
            Point p2 = foldLineVi.StartPoint - v * 10000;
            Point c1, c2, c3, c4;
            c1 = new Point(0, 0);
            c2 = new Point(0, mainWindow.foldingPaperViewport.ActualHeight);
            c3 = new Point(mainWindow.foldingPaperViewport.ActualWidth, mainWindow.foldingPaperViewport.ActualHeight);
            c4 = new Point(mainWindow.foldingPaperViewport.ActualWidth, 0);
            List<Point> pList = new List<Point>();
            pList.Add(c1);
            pList.Add(c2);
            pList.Add(c3);
            pList.Add(c4);
            pList.Add(c1);

            // 寻找折线与视口大小的矩形的交点
            int finded = 0;
            for (int i = 0; i < 4; i++)
            {

                if (finded == 0 && CloverMath.GetIntersectionOfTwoSegments(p1, p2, pList[i], pList[i + 1], out outP1))
                    finded++;
                else if (finded == 1 && CloverMath.GetIntersectionOfTwoSegments(p1, p2, pList[i], pList[i + 1], out outP2))
                {
                    finded++;
                    break;
                }
            }
            if (finded != 2)
                return false;

            Vector v1 = lineVi.EndPoint - Origin2Dpos;
            Vector v2 = outP2 - outP1;
            if (Vector.CrossProduct(v2, v1) > 0)
            {
                Point temp = outP1;
                outP1 = outP2;
                outP2 = temp;
            }

            return true;
        }

    }

}