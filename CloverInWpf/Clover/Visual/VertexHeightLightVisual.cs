﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace Clover.Visual
{
    class VertexHeightLightVisual : VisualElementFactory
    {
        Ellipse mark;

        public VertexHeightLightVisual(Brush brush, Double posX, Double posY)
        {
            mark = new Ellipse();
            mark.Height = mark.Width = 10;
            mark.Margin = new System.Windows.Thickness(0, -5, 0, 0);
            mark.Fill = brush;
            box.Children.Add(mark);
            box.Opacity = 0;
            TranslateTransform = new TranslateTransform(posX, posY);
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