using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Clover.Tool
{
    class TestTool : ToolFactory
    {   
        public TestTool(MainWindow mainWindow) : base(mainWindow)
        {
            //System.Windows.MessageBox.Show();
        }

        protected override void onEnterElement(Object element)
        {
            //Type t = element.GetType();
            Debug.WriteLine(element.GetType());
        }

        protected override void onLeaveElement(Object element)
        {
            //Debug.WriteLine(element.GetType());
        }

        protected override void onSelectElement(Object element)
        {
            // 将视角变换到与当前纸张平行
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                //Clover.Vertex v = (Clover.Vertex)element;
                //Clover.Edge ad = new Clover.Edge();
                
            }
        }

        protected override void onUnselectElement(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void onDrag(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
