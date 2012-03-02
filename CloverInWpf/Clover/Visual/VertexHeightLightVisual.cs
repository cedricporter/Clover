using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Clover.Visual
{
    class VertexHeightLightVisual : VisualElementFactory
    {
        Ellipse mark;

        public VertexHeightLightVisual(Brush brush, Double posX, Double posY)
        {
            mark = (Ellipse)App.Current.FindResource("VertexMark");
            mark.Fill = brush;
            grid.Children.Add(mark);
            grid.Opacity = 0;
            TranslateTransform = new TranslateTransform(posX, posY);
            //mark.Opacity = 0;
        }

        public override void FadeIn()
        {
            if (grid.Opacity < 1)
            {
                grid.Opacity += 0.1;
            }
            else
                state = VisualElementFactory.State.Display;
        }

        public override void Display()
        {
            
        }

        public override void FadeOut()
        {
            if (grid.Opacity > 0)
            {
                grid.Opacity -= 0.1;
            }
            else
                state = VisualElementFactory.State.Destroy;
        }
    }
}
