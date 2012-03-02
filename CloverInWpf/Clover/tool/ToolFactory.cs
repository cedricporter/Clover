using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Media3D;

/**
@date		:	2012/02/29
@filename	: 	ToolFactory.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	工具工厂类
**/

namespace Clover.Tool
{
    public abstract class ToolFactory
    {
        static public Point lastMousePos = new Point();
        static public Point currMousePos = new Point();
        static public Object lastOveredElement = null;
        static public Object currOveredElement = null;
        static public Object lastSelectedElement = null;
        static public Object currSelectedElement = null;

        MainWindow mainWindow;

        public ToolFactory(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        /// <summary>
        /// 执行ray scene query，调用其他函数完成拾取判断。
        /// </summary>
        /// <returns>拾取到的CloverElement。如果没拾取到，返回null</returns>
        public Object ExcuteHitTest()
        {
            foreach (Edge edge in mainWindow.cloverController.Edges)
            {
                Point3D p1 = edge.Vertex1.GetPoint3D();
                Point3D p2 = edge.Vertex2.GetPoint3D();
                // 首先要将点从模型坐标系变换到世界坐标系
                Matrix3D worldMat = mainWindow.cloverController.RenderController.Entity.Transform.Value;
                p1 *= worldMat;
                p2 *= worldMat;
                _3DTools.MathUtils.

            }

            return null;
        }

        /// <summary>
        /// 鼠标移动时的逻辑
        /// </summary>
        public void onMove()
        {
            currMousePos = Mouse.GetPosition(mainWindow);
            if (Mouse.LeftButton != MouseButtonState.Pressed && Mouse.RightButton != MouseButtonState.Pressed)
            {
                currOveredElement = ExcuteHitTest();

                if (currOveredElement != lastOveredElement)
                {
                    if (currOveredElement != null)
                        onEnterElement(currOveredElement);
                    if (lastOveredElement != null)
                        onLeaveElement(lastOveredElement);
                }

                lastOveredElement = currOveredElement;
                lastMousePos = currMousePos;
            }

            Debug.WriteLine("=====================");
            Debug.WriteLine(Mouse.GetPosition(mainWindow.foldingPaperViewport));
            
        }

        /// <summary>
        /// 鼠标左键点击时的逻辑
        /// </summary>
        public void onPress()
        {
            lastSelectedElement = currSelectedElement;
            currSelectedElement = currOveredElement;
            if (currSelectedElement != null)
                onSelectElement(currSelectedElement);
            if (lastSelectedElement != null)
                onUnselectElement(lastSelectedElement);
        }

        protected abstract void onEnterElement(Object element);

        protected abstract void onLeaveElement(Object element);

        protected abstract void onSelectElement(Object element);

        protected abstract void onUnselectElement(Object element);




    }
}
