using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Clover.Visual
{
    class TuckTriangleVisual : VisualElementFactory
    {
        Polygon triangle = new Polygon();
        Point p1, p2, p3;
        public System.Windows.Point P3
        {
            get { return p3; }
            set { p3 = value; triangle.Points[2] = p3; }
        }
        public System.Windows.Point P2
        {
            get { return p2; }
            set { p2 = value; triangle.Points[1] = p2; }
        }
        public System.Windows.Point P1
        {
            get { return p1; }
            set { p1 = value; triangle.Points[0] = p1; }
        }

        public TuckTriangleVisual(Point p1, Point p2, Point p3, Brush color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            triangle.Fill = color;
            box.Children.Add(triangle);
            triangle.Points.Clear();
            triangle.Points.Add(p1);
            triangle.Points.Add(p2);
            triangle.Points.Add(p3);
        }

        void UpdateData()
        {    
            
            
            
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
            //throw new Exception("The method or operation is not implemented.");
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
