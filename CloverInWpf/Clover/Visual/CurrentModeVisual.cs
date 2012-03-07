using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
@note		:	在左上角显示当前处于何种模式
**/

namespace Clover.Visual
{
    class CurrentModeVisual : VisualElementFactory
    {

        int posY = 60;

        public CurrentModeVisual(String text)
        {
            Grid innerBox = new Grid();
            Rectangle bg = new Rectangle();
            bg.RadiusX = bg.RadiusY = 15;
            bg.Fill = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            innerBox.Children.Add(bg);
            TextBlock textblock = new TextBlock();
            textblock.Margin = new Thickness(24, 12, 24, 12);
            textblock.Text = text;
            textblock.FontSize = 14;
            textblock.Foreground = new SolidColorBrush(Colors.White);
            innerBox.Children.Add(textblock);
            box.Children.Add(innerBox);
            box.Opacity = 0;
            TransformGroup = new TranslateTransform(10, posY);
        }

        public override void FadeIn()
        {
            if (box.Opacity < 1)
            {
                box.Opacity += 0.1;
                TransformGroup = new TranslateTransform(10, posY);
                posY -= 1;
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
                TransformGroup = new TranslateTransform(10, posY);
                posY += 1;
            }
            else
                state = VisualElementFactory.State.Destroy;
        }
    }
}
