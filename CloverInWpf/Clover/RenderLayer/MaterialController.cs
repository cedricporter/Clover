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

        /// <summary>
        /// 当分辨率改变时改变纸张的边的分辨率
        /// </summary>
        void UpdateEdgeLayer()
        {
            Double thickness = (height > width ? height : width) / 100;
            Image img = new Image();
            Rect rect = new Rect(new Size(width, height));
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawRectangle((Brush)null, new Pen(new SolidColorBrush(Colors.Black), thickness), rect);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            img.Source = bmp;
            ImageBrush imgb = new ImageBrush(bmp);

            if (edgeLayer != null)
                frontMaterial.Children.Remove(edgeLayer);
            edgeLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(edgeLayer);
        }



    }
}
