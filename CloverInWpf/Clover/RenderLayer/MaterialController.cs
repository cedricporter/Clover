using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	控制纸张双面的材质，包括纹理和折线
**/

namespace Clover.RenderLayer
{
    class MaterialController
    {
        int height = 2; ///材质垂直分辨率
        int width = 2;  ///材质水平分辨率
        MaterialGroup frontMaterial = null;
        MaterialGroup backMaterial = null;
        DiffuseMaterial edgeLayer = null;
        DiffuseMaterial frontFoldLineLayer = null;

        public MaterialController()
        {

        }

        /// <summary>
        /// 更新纸张正面的纹理
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public MaterialGroup UpdateFrontMaterial(MaterialGroup mat)
        {
            frontMaterial = mat;
            ImageSource img = ((mat.Children[0] as DiffuseMaterial).Brush as ImageBrush).ImageSource;
            height = (int)img.Height;
            width = (int)img.Width;
            UpdateEdgeLayer();
            return mat;
        }

        /// <summary>
        /// 更新纸张背面的纹理
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public MaterialGroup UpdateBackMaterial(MaterialGroup mat)
        {
            backMaterial = mat;
            return mat;
        }

        public void AddFoldingLine(Double u0, Double v0, Double u1, Double v1, Boolean isUpdate = false)
        {
            Point p0 = new Point(u0 * width, v0 * height);
            Point p1 = new Point(u1 * width, v1 * height);
            AddFrontFoldingLine(p0, p1, isUpdate);
        }

        void AddFrontFoldingLine(Point p0, Point p1, Boolean isUpdate)
        {
            //Image newImg = new Image();

            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            Pen pen = new Pen(new SolidColorBrush(Colors.Black), 20);
            pen.DashStyle = DashStyles.Dash;
            dc.DrawLine(pen, p0, p1);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            //newImg.Source = bmp;
            ImageBrush imgb = new ImageBrush(bmp);

            if (frontFoldLineLayer != null)
                frontMaterial.Children.Remove(frontFoldLineLayer);
            frontFoldLineLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(frontFoldLineLayer);
        }

        /// <summary>
        /// 当分辨率改变时改变纸张的边的分辨率
        /// </summary>
        void UpdateEdgeLayer()
        {
            Double thickness = (height > width ? height : width) / 100;
            //Image img = new Image();
            Rect rect = new Rect(new Size(width, height));
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawRectangle((Brush)null, new Pen(new SolidColorBrush(Colors.Black), thickness), rect);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            //img.Source = bmp;
            ImageBrush imgb = new ImageBrush(bmp);

            if (edgeLayer != null)
                frontMaterial.Children.Remove(edgeLayer);
            edgeLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(edgeLayer);
        }



    }
}
