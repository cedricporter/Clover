using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	显示当前折叠点占边的百分比
**/

namespace Clover.Visual
{
    class FoldLinePercentageVisual : VisualElementFactory
    {
        TextBlock textBolck = new TextBlock();
        Line line1 = new Line();
        Line line2 = new Line();
        Point p1, p2, p3, p4;

        #region get/set

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

        #endregion

        void UpdateLine()
        {
            Vector v = point2 - point1;
            Vector dir = v;
            if (dir.X != 0 || dir.Y != 0)
                dir.Normalize();
            Point pmid = point1 + v * offset;
            p1 = point1;
            p2 = pmid - dir * 20;
            if ((p2 - p1) * dir < 0)
                p2 = p1;
            p3 = pmid + dir * 20;
            p4 = point2;
            if ((p4 - p3) * dir < 0)
                p3 = p4;

            line1.X1 = p1.X;
            line1.Y1 = p1.Y;
            line1.X2 = p2.X;
            line1.Y2 = p2.Y;
            line2.X1 = p3.X;
            line2.Y1 = p3.Y;
            line2.X2 = p4.X;
            line2.Y2 = p4.Y;

            textBolck.Text = "0";
            textBolck.Text += offset.ToString("#.00");
            textBolck.RenderTransform = new TranslateTransform(pmid.X - 20, pmid.Y - 10);
        }

        public FoldLinePercentageVisual(Point pos1, Point pos2, Double offset)
        {
            point1 = pos1;
            point2 = pos2;
            this.offset = offset;
            line1.Stroke = line2.Stroke = (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush");
            line1.StrokeThickness = line2.StrokeThickness = 3;
            textBolck.HorizontalAlignment = HorizontalAlignment.Left;
            textBolck.VerticalAlignment = VerticalAlignment.Top;
            textBolck.Width = 40;
            textBolck.Height = 20;
            textBolck.FontSize = 13;
            textBolck.Foreground = new SolidColorBrush(Colors.White);
            textBolck.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            textBolck.TextAlignment = TextAlignment.Center;
            UpdateLine();
            box.Children.Add(textBolck);
            box.Children.Add(line1);
            box.Children.Add(line2);
            box.Opacity = 0;
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
