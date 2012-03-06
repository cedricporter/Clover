using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Clover.Visual
{
    class FoldLinePercentageVisual : VisualElementFactory
    {
        TextBlock textBolck = new TextBlock();
        Line line1 = new Line();
        Line line2 = new Line();
        Point p1, p2, p3, p4;

        Point point1, point2;
        public System.Windows.Point Point2
        {
            get { return point2; }
            set { point2 = value; UpdateLine(); }
        }
        public System.Windows.Point Point1
        {
            get { return point1; }
            set { point1 = value; UpdateLine(); }
        }
        Double offset;
        public System.Double Offset
        {
            get { return offset; }
            set { offset = value; UpdateLine(); }
        }
        void UpdateLine()
        {
            Vector v = point2 - point1;
            Vector dir = v;
            dir.Normalize();
            p1 = point1;
            p2 = point1 + v * offset - (dir * 13);
            p3 = point1 + v * offset + (dir * 13);
            p4 = point2;
            
            line1.X1 = p1.X;
            line1.Y1 = p1.Y;
            line1.X2 = p2.X;
            line1.Y2 = p2.Y;
            line2.X1 = p3.X;
            line2.Y1 = p3.Y;
            line2.X2 = p4.X;
            line2.Y2 = p4.Y;

            textBolck.Text = offset.ToString("#.00");
            textBolck.RenderTransform = new TranslateTransform(p2.X, p2.Y);
        }

        public FoldLinePercentageVisual(Point pos1, Point pos2, Double offset)
        {
            point1 = pos1;
            point2 = pos2;
            this.offset = offset;
            line1.Stroke = line2.Stroke = new SolidColorBrush(Color.FromRgb(255, 153, 0));
            line1.StrokeThickness = line2.StrokeThickness = 2;
            line1.StrokeDashArray.Add(1);
            line1.StrokeDashArray.Add(3);
            line2.StrokeDashArray.Add(1);
            line2.StrokeDashArray.Add(3);
            textBolck.FontSize = 13;
            UpdateLine();
            box.Children.Add(textBolck);
            box.Children.Add(line1);
            box.Children.Add(line2);
            
        }

        public override void FadeIn()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public override void Display()
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public override void FadeOut()
        {
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
