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
    public abstract class VisualElementFactory
    {
        public enum State
        {
            Created, FadeIn, Display, FadeOut, Destroy
        }

        protected State state = State.Created;
        
        /// <summary>
        /// 容器 
        /// </summary>
        public Canvas box = new Canvas();

        /// <summary>
        /// 变换
        /// </summary>
        Transform transformGroup;
        public System.Windows.Media.Transform TransformGroup
        {
            get { return transformGroup; }
            set { transformGroup = value; }
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
            box.RenderTransform = transformGroup;
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
