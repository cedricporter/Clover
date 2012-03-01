using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

/**
@date		:	2012/03/01
@filename	: 	TextVisualElement.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	普通文字的显示
**/

namespace Clover.Visual
{
    class TextVisualElement : VisualElementFactory
    {
        TextBlock textBlock = new TextBlock();

        public TextVisualElement(String text, Point pos, Brush textColor)
        {
            textBlock.Text = text;
            textBlock.FontSize = 12;
            textBlock.Margin = new System.Windows.Thickness(5,2,5,2);
            textBlock.Foreground = textColor;
            grid.Children.Add(textBlock);
            grid.RenderTransform = new TranslateTransform(pos.X, pos.Y);
        }

        public override void FadeIn()
        {
            if (!grid.HasAnimatedProperties)
            {
                grid.BeginAnimation(Grid.OpacityProperty, (DoubleAnimation)App.Current.FindResource("AlpahFadeInAnimation"));
            }
            if (grid.Opacity == 1.0)
            {
                state = VisualElementFactory.State.Display;
            }
        }

        public override void Display()
        {
            grid.BeginAnimation(Grid.OpacityProperty, null);
            state = VisualElementFactory.State.FadeOut;
        }

        public override void FadeOut()
        {
            if (!grid.HasAnimatedProperties)
            {
                grid.BeginAnimation(Grid.OpacityProperty, (DoubleAnimation)App.Current.FindResource("AlpahFadeOutAnimation"));
            }
            if (grid.Opacity == 0.0)
            {
                //MessageBox.Show("!");
                state = VisualElementFactory.State.Destroy;
            }
        }
    }
}
