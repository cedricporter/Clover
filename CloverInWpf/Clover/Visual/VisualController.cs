using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

/**
@date		:	2012/02/29
@filename	: 	VisualController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	负责管理各种交互视觉效果
**/

namespace Clover.Visual
{
    
    class VisualController
    {

        #region 单例模式

        static VisualController instance = null;

        public static VisualController GetSingleton(MainWindow mainWindow)
        {
            if (instance != null)
                return instance;
            return new VisualController(mainWindow);
        }

        #endregion

        MainWindow mainWindow;
        Grid grid = new Grid();
        List<VisualElementFactory> visualList = new List<VisualElementFactory>();

        private VisualController(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            // 初始化视觉容器
            grid.Height = grid.Width = 0;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;
            mainWindow.WindowRoot.Children.Add(grid);
        }

        public void Update()
        {
            foreach (VisualElementFactory vi in visualList)
            {
                switch (vi.GetState())
                {
                    case VisualElementFactory.State.FadeIn:
                        vi.FadeIn();
                        break;
                    case VisualElementFactory.State.Display:
                        vi.Display();
                        break;
                    case VisualElementFactory.State.FadeOut:
                        vi.FadeOut();
                        break;
                    case VisualElementFactory.State.Destroy:
                        RemoveVisual(vi);
                        break;
                }
            }
        }

        #region VisualList外部管理接口
        
        public void AddVisual(VisualElementFactory vi)
        {
            grid.Children.Add(vi.grid);
            visualList.Add(vi);
        }

        public void RemoveVisual(VisualElementFactory vi)
        {
            grid.Children.Remove(vi.grid);
            visualList.Remove(vi);
        }

        public void RemoveAllVisual()
        {
            foreach (VisualElementFactory vi in visualList)
                grid.Children.Remove(vi.grid);
            visualList.RemoveAll(null);
        }

        #endregion

    }
}
