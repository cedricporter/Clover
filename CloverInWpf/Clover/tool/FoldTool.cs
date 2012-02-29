using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
@date		:	2012/02/29
@filename	: 	FoldTool.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	纸张折叠工具
**/

namespace Clover.Tool
{
    class FoldTool : ToolFactory
    {

        public FoldTool(MainWindow mainWindow)
            : base(mainWindow)
        {

        }

        protected override void onEnterElement(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onLeaveElement(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
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
