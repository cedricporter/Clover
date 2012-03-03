using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Clover.Visual;
using System.Windows.Media;

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

        Double pointThreadhold = 10;         /// 拾取点误差
        //Double lineThreadhold = 0.15;      /// 拾取线误差
        Boolean isVisualEnable = true;       /// 开启视觉元素
        MainWindow mainWindow;
        VisualController visualController;

        VisualElementFactory lastOveredElementVi = null;
        VisualElementFactory currOveredElementVi = null;
        VisualElementFactory lastSelectedElementVi = null;
        VisualElementFactory currSelectedElementVi = null;

        public ToolFactory(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            visualController = VisualController.GetSingleton(mainWindow);
        }

        /// <summary>
        /// 执行ray scene query，调用其他函数完成拾取判断。
        /// </summary>
        /// <returns>拾取到的CloverElement。如果没拾取到，返回null</returns>
        public Object ExcuteHitTest(out Object shadow2DElement)
        {
            foreach (Edge edge in mainWindow.cloverController.Edges)
            {
                Point3D p1 = edge.Vertex1.GetPoint3D();
                Point3D p2 = edge.Vertex2.GetPoint3D();
                p1 *= Utility.GetInstance().To2DMat;
                p2 *= Utility.GetInstance().To2DMat;
                Point p12d = new Point(p1.X, p1.Y);
                Point p22d = new Point(p2.X, p2.Y);
                Point p = currMousePos;
                // 判断点
                if ((p - p12d).Length < pointThreadhold)
                {
                    //Debug.WriteLine(p1);
                    shadow2DElement = p12d;
                    return edge.Vertex1;
                }
                if ((p - p22d).Length < pointThreadhold)
                {
                    //Debug.WriteLine(p2);
                    shadow2DElement = p22d;
                    return edge.Vertex2;
                }
                // 判断线
                Vector V1 = p22d - p12d;
                Vector V2 = p - p12d;
                Double t = (V1 * V2) / (V1 * V1);
                Point p3 = p12d + t * V1;
                if ((p - p3).Length < pointThreadhold)
                {
                    shadow2DElement = new Clover.Edge2D(p12d, p22d);
                    return edge;
                }
                //Vector V1 = p - p12d;
                //Vector V2 = p22d - p;
                //Vector V0 = p22d - p12d;
                //if (V1.Length + V2.Length - V0.Length < lineThreadhold)
                //{
                //    //Debug.WriteLine(edge);
                //    return edge;
                //}                
            }
            shadow2DElement = null;
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
                Object shadowElement;
                currOveredElement = ExcuteHitTest(out shadowElement);

                // 上一帧拾取到的元素和这一帧拾取到的不同
                if (currOveredElement != lastOveredElement)
                {
                    if (isVisualEnable)
                    {
                        lastOveredElementVi = currOveredElementVi;
                        currOveredElementVi = null;
                    }
                    if (currOveredElement != null)
                    {
                        if (isVisualEnable)
                        {
                            // 当前拾取到的是点
                            if (currOveredElement.GetType().ToString() == "Clover.Vertex")
                            {
                                Point pos = (Point)shadowElement;
                                currOveredElementVi = new VertexHeightLightVisual((SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush"),
                                    pos.X, pos.Y);
                                currOveredElementVi.Start();
                                visualController.AddVisual(currOveredElementVi);
                            }
                            // 当前拾取到的是边
                            //else if (currOveredElement.GetType().ToString() == "Clover.Edge")
                            //{
                            //    Clover.Edge2D edge2d = (Clover.Edge2D)shadowElement;
                            //    currOveredElementVi = new EdgeHeightLightVisual((SolidColorBrush)App.Current.FindResource("VisualElementBlueBrush"),
                            //        edge2d.p1, edge2d.p2);
                            //    currOveredElementVi.Start();
                            //    visualController.AddVisual(currOveredElementVi);
                            //}
                        }
                        onEnterElement(currOveredElement);
                    }
                    if (lastOveredElement != null)
                    {
                        if (isVisualEnable)
                        {
                            // 使上一个视觉元素消失
                            if (lastOveredElementVi != null)
                            {
                                lastOveredElementVi.End();
                                lastOveredElementVi = null;
                            }
                        }
                        onLeaveElement(lastOveredElement);
                    }
                }
                

                lastOveredElement = currOveredElement;
                lastMousePos = currMousePos;
            }

            //Debug.WriteLine("=====================");
            //Debug.WriteLine(Mouse.GetPosition(mainWindow.foldingPaperViewport));
            
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
