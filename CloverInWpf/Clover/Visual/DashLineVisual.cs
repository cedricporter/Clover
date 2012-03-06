using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	在指定的两点间生成虚线
**/

namespace Clover.Visual
{
    class DashLineVisual : VisualElementFactory
    {
        Path linePath;

        #region get/set
        
        Point startPoint;
        public System.Windows.Point StartPoint
        {
            get { return startPoint; }
            set 
            { 
                startPoint = value;
                UpdateLineData();
            }
        }
        Point endPoint;

        public System.Windows.Point EndPoint
        {
            get { return endPoint; }
            set 
            { 
                endPoint = value;
                UpdateLineData();
            }
        }

        #endregion


        public DashLineVisual(Point start, Point end, Brush color)
        {
            //Path line = new Path();

            linePath = new Path();
            linePath.Stroke = color;
            linePath.StrokeDashArray.Add(4);
            linePath.StrokeDashArray.Add(4);
            linePath.StrokeThickness = 2;
            startPoint = start;
            endPoint = end;
            UpdateLineData();
            box.Children.Add(linePath);
            box.Opacity = 0;
        }

        void UpdateLineData()
        {
            LineGeometry line = new LineGeometry();
            line.StartPoint = startPoint;
            line.EndPoint = endPoint;
            linePath.Data = line;
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
