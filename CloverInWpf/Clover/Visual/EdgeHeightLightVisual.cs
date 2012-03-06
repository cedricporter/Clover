using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
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
@note		:	线段高亮
**/

namespace Clover.Visual
{
    class EdgeHeightLightVisual : VisualElementFactory
    {

        Rectangle mark;

        public EdgeHeightLightVisual(Brush brush, Point p1, Point p2)
        {
            Vector v = p2 - p1;
            double deg = Vector.AngleBetween(v, new Vector(0, -1));
            mark = new Rectangle();
            mark.Width = 10;
            mark.Height = v.Length;
            mark.RadiusX = mark.RadiusY = 2;
            //mark.Margin = new System.Windows.Thickness(0, -5, 0, 0);
            mark.Fill = brush;
            box.Children.Add(mark);
            box.Opacity = 0;
            TransformGroup tg = new TransformGroup();
            TranslateTransform ts = new TranslateTransform(p1.X, p1.Y);
            RotateTransform tr = new RotateTransform(Vector.AngleBetween(v, new Vector(0, -1)));
            tg.Children.Add(tr);
            tg.Children.Add(ts);
            
            TransformGroup = tg;
            //mark.Opacity = 0;
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
