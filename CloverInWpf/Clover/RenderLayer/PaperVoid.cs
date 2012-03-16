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
                // 上层纸张
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
                // 背面再拍一张照
                RenderTargetBitmap bmpb = new RenderTargetBitmap((int)vp.ActualWidth, (int)vp.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                Quaternion quat = new Quaternion(0, 1, 0, 0);
                quat = quat * render.SrcQuaternion;
                render.TransformGroup.Children[0] = new RotateTransform3D(new QuaternionRotation3D(quat));
                bmpb.Render(vp);
                render.TransformGroup.Children[0] = new RotateTransform3D(new QuaternionRotation3D(render.SrcQuaternion));
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

            render.Entity.Content = render.ModelGroup;

            List<PathFigure> pathFigures = new List<PathFigure>();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(vp.ActualWidth / 2, 0);
            pathFigure.Segments.Add(new LineSegment(new Point(100, vp.ActualHeight), false));
            pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, vp.ActualHeight), false));
            pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, 0), false));
            pathFigures.Add(pathFigure);
            Geometry topImgFrontClipGeometry = new PathGeometry(pathFigures);
            topImgFront.Clip = topImgFrontClipGeometry;
        }

        public static void UpdateShadow(Viewport3D vp, Point p1, Point p2, Image topImgFront, Image bgImg)
        {
            List<PathFigure> pathFigures = new List<PathFigure>();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(vp.ActualWidth/2,0);
            pathFigure.Segments.Add(new LineSegment(new Point(100, vp.ActualHeight), false));
            pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, vp.ActualHeight), false));
            pathFigure.Segments.Add(new LineSegment(new Point(vp.ActualWidth, 0), false));
            pathFigures.Add(pathFigure);
            Geometry topImgFrontClipGeometry = new PathGeometry(pathFigures);
            topImgFront.Clip = topImgFrontClipGeometry;
        }

        public static void DestoryShadow(Viewport3D vp, Image topImgFront, Image bgImg)
        {

        }

    }
}
