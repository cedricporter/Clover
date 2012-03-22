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
        LineArrow lineArrow = new LineArrow();
        TextBlock textBlock = new TextBlock();
        TranslateTransform translateText;

        public BlendAngleVisual(int degree, Point pos1, Point pos2)
        {
            box.Children.Add(lineArrow);
            box.Children.Add(textBlock);
            lineArrow.StrokeThickness = 3;
            lineArrow.ArrowSize = 8;
            lineArrow.Stroke = (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush");
            lineArrow.BendAmount = 0;
            lineArrow.StartArrow = Microsoft.Expression.Media.ArrowType.Arrow;
            textBlock.Width = 40;
            textBlock.Height = 20;
            textBlock.FontSize = 13;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            textBlock.TextAlignment = TextAlignment.Center;
            translateText = new TranslateTransform();
            textBlock.RenderTransform = translateText;
            TransformGroup = new TranslateTransform();
            box.Opacity = 0;
            //Update(degree)
        }

        public void Update(int degree, Point pos1, Point pos2)
        {
            double w = pos2.X - pos1.X;
            double h = pos2.Y - pos1.Y;
            double halfw = w / 2;
            double halfh = h / 2;
            if (w >= 0)
            {
                lineArrow.Width = w;
                (TransformGroup as TranslateTransform).X = pos1.X + 5;
                lineArrow.StartCorner = Microsoft.Expression.Media.CornerType.TopLeft;
                translateText.X = halfw - 20;
            }
            else
            {
                lineArrow.Width = -w;
                (TransformGroup as TranslateTransform).X = pos2.X + 5;
                lineArrow.StartCorner = Microsoft.Expression.Media.CornerType.TopRight;
                translateText.X = - 20 - halfw;
            }
            if (h >= 0)
            {
                lineArrow.Height = h;
                (TransformGroup as TranslateTransform).Y = pos1.Y;
                translateText.Y = halfh - 10;
            }
            else
            {
                lineArrow.Height = -h;
                (TransformGroup as TranslateTransform).Y = pos1.Y - h;
                translateText.Y = - 10 - halfh;
            }
            textBlock.Text = (180 - degree).ToString();
            textBlock.Text += "°";
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
