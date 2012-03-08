﻿using System;
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

namespace Clover
{
    class MaterialController
    {
        int height = 2;                 ///材质垂直分辨率
        int width = 2;                  ///材质水平分辨率
        Double thickness = 0;              ///线条粗细
        MaterialGroup frontMaterial = new MaterialGroup();
        MaterialGroup backMaterial = new MaterialGroup();
        MaterialGroup transparentFrontMaterial = null;
        MaterialGroup transparentBackMaterial = null;
        DiffuseMaterial frontEdgeLayer = null;
        DiffuseMaterial backEdgeLayer = null;
        DiffuseMaterial frontFoldLineLayer = null;
        DiffuseMaterial backFoldLineLayer = null;
        DiffuseMaterial frontAnimationLayer = null;

        public MaterialController()
        {
            
        }

        /// <summary>
        /// 获取纸张正面的半透明材质
        /// </summary>
        /// <returns></returns>
        public MaterialGroup GetFrontShadow()
        {
            transparentFrontMaterial = frontMaterial.Clone();
            for (int i=0; i<transparentFrontMaterial.Children.Count; i++)
            {
                // 第二层为边
                if (i == 1)
                    continue;
                DiffuseMaterial dm = (transparentFrontMaterial.Children[i] as DiffuseMaterial);
                dm.Color = dm.AmbientColor = Color.FromArgb(100, 255, 255, 255);
            }
            return transparentFrontMaterial;
        }

        /// <summary>
        /// 获取纸张背面的半透明材质
        /// </summary>
        /// <returns></returns>
        public MaterialGroup GetBackShadow()
        {
            transparentBackMaterial = backMaterial.Clone();
            foreach (Material m in transparentBackMaterial.Children)
            {
                DiffuseMaterial dm = (DiffuseMaterial)m;
                if (dm != null)
                {
                    dm.Color = dm.AmbientColor = Color.FromArgb(100, 255, 255, 255);
                }
            }
            return transparentBackMaterial;
        }

        /// <summary>
        /// 更新纸张正面的纹理
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public MaterialGroup UpdateFrontMaterial(DiffuseMaterial mat)
        {
            if (frontMaterial.Children.Count == 0)
                frontMaterial.Children.Add(mat);
            else
            {
                BeginPaperChange((DiffuseMaterial)frontMaterial.Children[0]);
                frontMaterial.Children[0] = mat;
            }
            ImageBrush imgb = (mat.Brush as ImageBrush);
            imgb.ViewportUnits = BrushMappingMode.Absolute;
            ImageSource img = imgb.ImageSource;
            height = (int)img.Height;
            width = (int)img.Width;
            thickness = (height > width ? height : width) / 100;

            UpdateFrontEdgeLayer();

            return frontMaterial;
        }

        /// <summary>
        /// 更新纸张背面的纹理
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public MaterialGroup UpdateBackMaterial(DiffuseMaterial mat)
        {
            if (backMaterial.Children.Count == 0)
                backMaterial.Children.Add(mat);
            else
                backMaterial.Children[0] = mat;

            UpdateBackEdgeLayer();

            return backMaterial;
        }

        #region 添加折线

        /// <summary>
        /// 添加折线
        /// </summary>
        /// <param name="u0"></param>
        /// <param name="v0"></param>
        /// <param name="u1"></param>
        /// <param name="v1"></param>
        public void AddFoldingLine(Double u0, Double v0, Double u1, Double v1)
        {
            Point p0 = new Point(u0 * width, v0 * height);
            Point p1 = new Point(u1 * width, v1 * height);
            AddFrontFoldingLine(p0, p1);
            AddBackFoldingLine(p0, p1);
        }

        /// <summary>
        /// 在折纸的正面添加一条折线
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        void AddFrontFoldingLine(Point p0, Point p1)
        {
            // 保存以前的折线
            ImageSource oldBmp = null;
            if (frontFoldLineLayer != null)
            {
                oldBmp = (frontFoldLineLayer.Brush as ImageBrush).ImageSource;
            }

            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            Pen pen = new Pen(new SolidColorBrush(Colors.Black), thickness*0.5);
            pen.DashStyle = DashStyles.Dash;
            if (frontFoldLineLayer != null)
            {
                dc.DrawImage(oldBmp, new Rect(new Size(width, height)));
            }
            dc.DrawLine(pen, p0, p1);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            ImageBrush imgb = new ImageBrush(bmp);
            imgb.ViewportUnits = BrushMappingMode.Absolute;

            if (frontFoldLineLayer != null)
                frontMaterial.Children.Remove(frontFoldLineLayer);
            frontFoldLineLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(frontFoldLineLayer);
        }

        /// <summary>
        /// 在折纸的背面添加一条折线
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        void AddBackFoldingLine(Point p0, Point p1)
        {
            // 保存以前的折线
            ImageSource oldBmp = null;
            if (backFoldLineLayer != null)
            {
                oldBmp = (backFoldLineLayer.Brush as ImageBrush).ImageSource;
            }

            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            Pen pen = new Pen(new SolidColorBrush(Colors.Black), thickness * 0.5);
            pen.DashStyle = DashStyles.DashDot;
            if (backFoldLineLayer != null)
            {
                dc.DrawImage(oldBmp, new Rect(new Size(width, height)));
            }
            dc.DrawLine(pen, p0, p1);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            ImageBrush imgb = new ImageBrush(bmp);
            imgb.ViewportUnits = BrushMappingMode.Absolute;

            if (backFoldLineLayer != null)
                backMaterial.Children.Remove(backFoldLineLayer);
            backFoldLineLayer = new DiffuseMaterial(imgb);
            backMaterial.Children.Add(backFoldLineLayer);
        }

        #endregion

        /// <summary>
        /// 当分辨率改变时改变纸张的边的分辨率
        /// </summary>
        void UpdateFrontEdgeLayer()
        {
            Rect rect = new Rect(new Size(width, height));
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawRectangle((Brush)null, new Pen(new SolidColorBrush(Colors.Black), thickness), rect);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            ImageBrush imgb = new ImageBrush(bmp);
            imgb.ViewportUnits = BrushMappingMode.Absolute;

            if (frontEdgeLayer != null)
                frontMaterial.Children.Remove(frontEdgeLayer);
            frontEdgeLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(frontEdgeLayer);
        }

        /// <summary>
        /// 当分辨率改变时改变纸张的边的分辨率
        /// </summary>
        void UpdateBackEdgeLayer()
        {
            Rect rect = new Rect(new Size(width, height));
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawRectangle((Brush)null, new Pen(new SolidColorBrush(Colors.Black), thickness), rect);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            ImageBrush imgb = new ImageBrush(bmp);
            imgb.ViewportUnits = BrushMappingMode.Absolute;

            if (backEdgeLayer != null)
                backMaterial.Children.Remove(backEdgeLayer);
            backEdgeLayer = new DiffuseMaterial(imgb);
            backMaterial.Children.Add(backEdgeLayer);
        }

        #region 切换纹理时候的动画

        int paperChangeCount = -1;
        void BeginPaperChange(DiffuseMaterial oldMat)
        {
            paperChangeCount = 0;
            frontAnimationLayer = oldMat;
            frontMaterial.Children.Add(frontAnimationLayer);
        }

        public void PaperChange()
        {
            if (paperChangeCount == -1)
                return;

            int alpha = 255 - 5 * paperChangeCount;
            frontAnimationLayer.AmbientColor = frontAnimationLayer.Color = Color.FromArgb((Byte)alpha, 255, 255, 255);

            if (paperChangeCount++ > 50)
            {
                paperChangeCount = -1;
                frontMaterial.Children.Remove(frontAnimationLayer);
                frontAnimationLayer = null;
            }
        }
        #endregion

    }
}
