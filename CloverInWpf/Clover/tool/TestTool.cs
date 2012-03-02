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
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onUnselectElement(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
