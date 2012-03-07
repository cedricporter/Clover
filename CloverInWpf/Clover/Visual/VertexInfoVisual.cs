using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	调试用顶点信息
**/

namespace Clover.Visual
{
    class VertexInfoVisual : VisualElementFactory
    {
        TextBlock infotext = new TextBlock();
        TranslateTransform ts;

        Vertex vertex;

        public VertexInfoVisual(Vertex v)
        {
            vertex = v;
            infotext.Foreground = new SolidColorBrush(Colors.Black);
            infotext.Background = new SolidColorBrush(Colors.White);
            infotext.Opacity = 0.5;
            box.Children.Add(infotext);
            ts = new TranslateTransform();
            TransformGroup = ts;
            UpdateInfoCallBack(v, null);
        }

        public void UpdateInfoCallBack(Object sender, EventArgs e)
        {
            Vertex v = (Vertex)sender;
            if (v == null)
                return;
            // 更新内容
            infotext.Text = "索引：" + v.Index.ToString();
            Point3D p = v.GetPoint3D();
            infotext.Text += "\n位置( " + p.X.ToString("#.0") + ", " + p.Y.ToString("#.0") + ", " + p.Z.ToString("#.0") + " )\n";
            infotext.Text += CloverController.GetInstance().VertexLayer.VertexCellTable[v.Index].Count.ToString();
            //infotext.Text += "\n纹理( " + v.u.ToString("#.0") + ", " + v.v.ToString("#.0") + " )";;
            // 更新位置
            Point3D pos = v.GetPoint3D();
            pos *= Utility.GetInstance().To2DMat;
            ts.X = pos.X + 10;
            ts.Y = pos.Y + 10;
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
            vertex.Update -= UpdateInfoCallBack;
            state = State.Destroy;
        }
    }
}
