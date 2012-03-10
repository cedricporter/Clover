using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;


/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	显示目前选择了要叠多少层
**/

namespace Clover.Visual
{
    class FaceLayerStackVisual : VisualElementFactory
    {
        Dictionary<Border, Face> itemFaceMap = new Dictionary<Border, Face>();
        StackPanel panel = new StackPanel();
        TranslateTransform translate = new TranslateTransform();
        public System.Windows.Media.TranslateTransform Translate
        {
            get { return translate; }
            set { translate = value; }
        }

        public FaceLayerStackVisual(List<Face> faces, Point pos)
        {
            box.Children.Add(panel);
            panel.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            panel.Orientation = Orientation.Vertical;
            panel.Width = 80;
            TransformGroup = translate;
            translate.X = pos.X;
            translate.Y = pos.Y;
            InitialStack(faces);
            box.Opacity = 0;
        }

        void InitialStack(List<Face> faces)
        {
            Border stackItem;
            TextBlock text;
            int layer = 1;
            foreach (Face f in faces)
            {
                stackItem = new Border();
                panel.Children.Add(stackItem);
                stackItem.Height = 20;
                stackItem.Background = new SolidColorBrush(Colors.Transparent);
                stackItem.MouseEnter += OnStackItemMouseEnter;
                stackItem.MouseLeave += OnStackItemMouseLeave;
                itemFaceMap[stackItem] = f;
                text = new TextBlock();
                stackItem.Child = text;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.Foreground = new SolidColorBrush(Colors.White);
                text.Text = layer.ToString();
                
                layer++;
            }
        }

        void OnStackItemMouseEnter(Object sender, EventArgs e)
        {
            (sender as Border).Background = (SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush");
        }

        void OnStackItemMouseLeave(Object sender, EventArgs e)
        {
            (sender as Border).Background = new SolidColorBrush(Colors.Transparent);
        }

        void OnStackItemClick(Object sender, EventArgs e)
        {
            // todo
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
