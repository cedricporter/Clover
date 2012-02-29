using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

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
        public String text;
        public Point pos;
        public Color textColor;

        public TextVisualElement(String text, Point pos, Color textColor)
        {
            this.text = text;
            this.pos = pos;
            this.textColor = textColor;
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
