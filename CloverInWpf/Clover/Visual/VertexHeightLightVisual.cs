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
            TranslateTransform = new TranslateTransform(posX, posY);
            //mark.Opacity = 0;
        }

        public override void FadeIn()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Display()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void FadeOut()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
