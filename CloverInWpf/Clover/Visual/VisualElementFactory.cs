using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

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
            FadeIn, Display, FadeOut, Destroy
        }

        State state = State.FadeIn;
        
        // 容器 
        public Grid grid = new Grid();

        public VisualElementFactory()
        {
            grid.Name = "grid";
        }

        public State GetState()
        {
            return state;
        }

        public abstract void FadeIn();

        public abstract void Display();

        public abstract void FadeOut();
        
    }
}
