using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

/**
@date		:	2012/02/29
@filename	: 	VisualElementFactory.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	Visual元素工厂类
**/

namespace Clover.Visual
{
    abstract class VisualElementFactory
    {
        public enum State
        {
            Created, FadeIn, Display, FadeOut, Destroy
        }

        protected State state = State.Created;
        
        /// <summary>
        /// 容器 
        /// </summary>
        public Grid grid = new Grid();

        public VisualElementFactory()
        {
            grid.Name = "grid";
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;
            Border bg = new Border();
            bg.Background = new SolidColorBrush(Color.FromArgb(120,0,0,0));
            bg.BorderBrush = new SolidColorBrush(Color.FromRgb(51,51,51));
            bg.BorderThickness = new Thickness(1);
            grid.Children.Add(bg);
            grid.Opacity = 0;
        }

        /// <summary>
        /// 获取当前视觉元素的状态
        /// </summary>
        /// <returns></returns>
        public State GetState()
        {
            return state;
        }

        /// <summary>
        /// 使视觉元素处于FadeIn状态
        /// </summary>
        public void Start()
        {
            state = VisualElementFactory.State.FadeIn;
        }

        /// <summary>
        /// 使视觉元素处于FadeOut状态
        /// </summary>
        public void End()
        {
            state = VisualElementFactory.State.FadeOut;
        }

        public abstract void FadeIn();

        public abstract void Display();

        public abstract void FadeOut();
        
    }
}
