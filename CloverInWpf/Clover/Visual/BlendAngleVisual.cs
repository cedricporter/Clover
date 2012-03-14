using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Expression.Controls;
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
@note		:	在Blending模式下提示两个面的夹角度数
**/

namespace Clover.Visual
{
    class BlendAngleVisual : VisualElementFactory
    {
        int degree;
        LineArrow lineArrow1 = new LineArrow();
        LineArrow lineArrow2 = new LineArrow();
        TextBlock textBlock = new TextBlock();
        TranslateTransform translate1, translate2, translateText;

        public BlendAngleVisual(int degree, Point pos1, Point pos2)
        {
            this.degree = degree;
            box.Children.Add(lineArrow1);
            box.Children.Add(lineArrow2);
            box.Children.Add(textBlock);
            //lineArrow1.Width = lineArrow2.Width = 30;
            //lineArrow1.Height = lineArrow2.Height = (pos2.Y - pos1.Y) / 2 - 10;
            lineArrow1.StrokeThickness = lineArrow2.StrokeThickness = 3;
            lineArrow1.ArrowSize = lineArrow2.ArrowSize = 8;
            lineArrow1.Stroke = lineArrow2.Stroke = (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush");
            lineArrow2.StartCorner = Microsoft.Expression.Media.CornerType.BottomLeft;
            lineArrow1.BendAmount = lineArrow2.BendAmount = 0;
            //translate1 = new TranslateTransform(pos1.X, pos1.Y);
            lineArrow1.RenderTransform = translate1;
            translate2 = new TranslateTransform(0, lineArrow1.Height + 20);
            lineArrow2.RenderTransform = translate2;
            textBlock.Text = degree.ToString();
            textBlock.Width = 40;
            textBlock.Height = 20;
            textBlock.FontSize = 13;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            textBlock.TextAlignment = TextAlignment.Center;
            translateText = new TranslateTransform(10, lineArrow1.Height);
            textBlock.RenderTransform = translateText;
            TransformGroup = new TranslateTransform(pos1.X, pos1.Y);
            box.Opacity = 0;
            //Update(degree)
        }

        public void Update(int degree, Point pos1, Point pos2)
        {
            double w = pos1.X + 30 - pos2.X;
            lineArrow1.Width = 30;
            if (w < 0)
            {
                lineArrow2.Width = -w;
                translate2.X = pos2.X - pos1.X + w;
                lineArrow2.StartCorner = Microsoft.Expression.Media.CornerType.BottomRight;
            }
            else
            {
                lineArrow2.Width = w;
                translate2.X = pos2.X - pos1.X;
                lineArrow2.StartCorner = Microsoft.Expression.Media.CornerType.BottomLeft;
            }
            //lineArrow2.Width = w > 0 ? w : -w;
            //translate2.X = pos2.X - pos1.X;
            double h = (pos2.Y - pos1.Y) / 2 - 10;
            lineArrow1.Height = lineArrow2.Height = h > 0 ? h : 0;
            translate2.Y = lineArrow1.Height + 20;
            textBlock.Text = degree.ToString();
            translateText.Y = lineArrow1.Height;
            (TransformGroup as TranslateTransform).X = pos1.X + 5;
            (TransformGroup as TranslateTransform).Y = pos1.Y;
        }

        public override void FadeIn()
        {
            if (box.Opacity < 1)
            {
                box.Opacity += 0.1;
            }
            else
                state = VisualElementFactory.State.Display;
        }

        public override void Display()
        {

        }

        public override void FadeOut()
        {
            if (box.Opacity > 0)
            {
                box.Opacity -= 0.1;
            }
            else
                state = VisualElementFactory.State.Destroy;
        }

    }
}
