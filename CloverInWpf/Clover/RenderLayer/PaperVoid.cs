using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	在执行Folding或Tucking时提供纸张的虚像，使用2D的Image实现
**/

namespace Clover
{
    class PaperVoid
    {

        public static void CreateShadow(Viewport3D vp, List<Face>topFaces, List<Face>bgFaces, Image topImgFront, Image bgImg)
        {
            RenderController render = RenderController.GetInstance();

            if (topFaces != null && topFaces.Count != 0)
            {
                //// 上层纸张
                Model3DGroup topModelGroup = new Model3DGroup();
                foreach (Face face in topFaces)
                {
                    topModelGroup.Children.Add(render.FaceMeshMap[face]);
                }
                render.Entity.Content = topModelGroup;
                // 正面拍一张照
                RenderTargetBitmap bmpf = new RenderTargetBitmap((int)vp.ActualWidth, (int)vp.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bmpf.Render(vp);
                topImgFront.Source = bmpf;
                //// 尝试提高性能
                //Rect rect = new Rect(new Size(vp.ActualWidth, vp.ActualHeight));
                //DrawingVisual dv = new DrawingVisual();
                //DrawingContext dc = dv.RenderOpen();
                //dc.DrawRectangle((Brush)null, (Pen)null, rect);
                //foreach (Face face in topFaces)
                //{
                //    dc.dr
                //}
                //dc.Close();

                //// 背面再拍一张照
                //RenderTargetBitmap bmpb = new RenderTargetBitmap((int)vp.ActualWidth, (int)vp.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                //Quaternion quat = new Quaternion(0, 1, 0, 0);
                //quat = quat * render.SrcQuaternion;
                //render.TransformGroup.Children[0] = new RotateTransform3D(new QuaternionRotation3D(quat));
                //bmpb.Render(vp);
                //render.TransformGroup.Children[0] = new RotateTransform3D(new QuaternionRotation3D(render.SrcQuaternion));
            }

            if (bgFaces != null && bgFaces.Count != 0)
            // 背景纸张
            {
                Model3DGroup bgModelGroup = new Model3DGroup();
                foreach (Face face in bgFaces)
                {
                    bgModelGroup.Children.Add(render.FaceMeshMap[face]);
                }
                render.Entity.Content = bgModelGroup;
                // 正面拍一张照
                RenderTargetBitmap bmpbg = new RenderTargetBitmap((int)vp.ActualWidth, (int)vp.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bmpbg.Render(vp);
                bgImg.Source = bmpbg;
            }

            render.Entity.Content = null;

            //List<PathFigure> pathFigures = new List<PathFigure>();
            //PathFigure pathFigure = new PathFigure();
            //pathFigure.StartPoint = new Point(vp.ActualWidth / 2, 0);
            //pathFigure.Segments.Add(new LineSegment(new Point(100, vp.ActualHeight), false));
            //pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, vp.ActualHeight), false));
            //pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, 0), false));
            //pathFigures.Add(pathFigure);
            //Geometry topImgFrontClipGeometry = new PathGeometry(pathFigures);
            //topImgFront.Clip = topImgFrontClipGeometry;
        }

        public static void UpdateShadow(Viewport3D vp, Point p1, Point p2, Image topImgFront, Image bgImg)
        {
            // 消除误差
            p1 = new Point(Math.Round(p1.X), Math.Round(p1.Y));
            p2 = new Point(Math.Round(p2.X), Math.Round(p2.Y));
            // 按逆时针寻找并剔除被折线“切掉”的点
            Point[] c = new Point[4];
            c[0] = new Point(0, 0);
            c[1] = new Point(0, vp.ActualHeight);
            c[2] = new Point(vp.ActualWidth, vp.ActualHeight);
            c[3] = new Point(vp.ActualWidth, 0);
            //c[4] = new Point(0, 0);
            List<Point> ignorePointList = new List<Point>();
            Vector v = p2 - p1;
            for (int i = 0; i < 4; i++)
            {
                if (Vector.CrossProduct((c[i] - p1), v) < 0)
                    ignorePointList.Add(c[i]);
            }
            // 划定topImgFront的显示区域
            List<PathFigure> pathFigures = new List<PathFigure>();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = p1;
            pathFigure.Segments.Add(new LineSegment(p2, false));
            //for (int i = 0; i < 4; i++) // 这堆写得比较蛋疼，主要功能是让它按顺序地绘制点
            //{
            //    if (!ignorePointList.Contains(c[i]))
            //        pathFigure.Segments.Add(new LineSegment(c[i], false));
            //}
            Point lastPoint = p2;
            while (ignorePointList.Count < 4)
            {
                foreach (Point p in c)
                {
                    if (ignorePointList.Contains(p))
                        continue;
                    //if (Math.Abs(p.X - lastPoint.X) < 0.00001 || Math.Abs(p.Y - lastPoint.Y) < 0.00001)
                    if (p.X == lastPoint.X || p.Y == lastPoint.Y)
                    {
                        pathFigure.Segments.Add(new LineSegment(p, false));
                        ignorePointList.Add(p);
                        lastPoint = p;
                    }
                }
            }
            pathFigures.Add(pathFigure);
            Geometry topImgFrontClipGeometry = new PathGeometry(pathFigures);
            topImgFront.Clip = topImgFrontClipGeometry;
        }

        public static void DestoryShadow(Viewport3D vp, Image topImgFront, Image bgImg)
        {
            topImgFront.Source = null;
            bgImg.Source = null;

            // 显示实像
            RenderController render = RenderController.GetInstance();
            render.Entity.Content = render.ModelGroup;
        }

    }
}
