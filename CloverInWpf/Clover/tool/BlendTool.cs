using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	Blending工具
**/

namespace Clover.Tool
{
    class BlendTool : ToolFactory
    {
        Vertex pickedVertex;
        Face nearestFace;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public BlendTool(MainWindow mainWindow)
            : base(mainWindow)
        { }

        public override void onIdle()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onEnterElement(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onLeaveElement(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onOverElement(Object element)
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

        protected override void onDrag(Object element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void onClick()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override void exit()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
