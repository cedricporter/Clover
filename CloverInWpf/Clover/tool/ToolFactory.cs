using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Mogre;
using System.Windows.Input;

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

        protected static RaySceneQuery raySceneQuery = null;


        MainWindow mainWindow;

        public ToolFactory(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            //raySceneQuery = mainWindow.sceneManager.CreateRayQuery(new Ray(), SceneManager.WORLD_GEOMETRY_TYPE_MASK);
            //raySceneQuery.SetSortByDistance(true);
            //System.Windows.MessageBox.Show("ToolFactory!");
        }

        /// <summary>
        /// 执行ray scene query，调用其他函数完成拾取判断。
        /// </summary>
        /// <returns>拾取到的CloverElement。如果没拾取到，返回null</returns>
        public Object ExcuteHitTest()
        {
            Camera camera = mainWindow.cameras[0];

            float x = (float)(currMousePos.X / mainWindow.MogreImage.ActualWidth);
            float y = (float)(currMousePos.Y / mainWindow.MogreImage.ActualHeight);

            if (null == raySceneQuery)
            {
                raySceneQuery = mainWindow.sceneManager.CreateRayQuery(new Ray(), SceneManager.WORLD_GEOMETRY_TYPE_MASK);
                raySceneQuery.SetSortByDistance(true);
            }
            raySceneQuery.Ray = camera.GetCameraToViewportRay(x, y);

            Ray ray = raySceneQuery.Ray;
            RaySceneQueryResult rayresult = raySceneQuery.Execute();
            if (rayresult.Count <= 0)
                return null;

            RaySceneQueryResult queryResult = raySceneQuery.GetLastResults();

            foreach (RaySceneQueryResultEntry resultEntry in queryResult)
            {
                if (resultEntry.movable == null)
                    continue;

                // 折纸的mesh也许不叫这个名字，不过先这样写着先
                if (resultEntry.movable.Name != "CloverEntity")
                    continue;

                // todo
                // PerformPick

            }
            return null;
        }

        /// <summary>
        /// 鼠标移动时的逻辑
        /// </summary>
        public void onMove()
        {
            currMousePos = Mouse.GetPosition(mainWindow.MogreImage);
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
