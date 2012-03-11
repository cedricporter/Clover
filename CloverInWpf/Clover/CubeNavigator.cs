using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Diagnostics;

/**
@date		:	2012/02/27
@filename	: 	CubeNavigator.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	一个立方体导航块
**/

/*   cube by kid..
      4 ______7
      /|      /
   0 /_|___3 /|
     | |____|_|
     | /5   | /6
    1|/____2|/    
*/


namespace Clover
{

    public class CubeNavigator
    {

        #region get/set
        
        static MainWindow mainWindow;
        Viewport3D cubeNavViewport;
        public System.Windows.Controls.Viewport3D CubeNavViewport
        {
            get { return cubeNavViewport; }
            set { cubeNavViewport = value; }
        }
        Model3DGroup cubeNavModel;
        public System.Windows.Media.Media3D.Model3DGroup CubeNavModel
        {
            get { return cubeNavModel; }
            set 
            { 
                cubeNavModel = value;
            }
        }
        Point lastMousePos;
        Quaternion lastQuat = new Quaternion();
        public System.Windows.Media.Media3D.Quaternion LastQuat
        {
            get { return lastQuat; }
            set 
            { 
                lastQuat = value; 
            }
        }

        #endregion

        #region 私有成员

        Boolean isDraging = false;

        #endregion

        #region 单例

        static CubeNavigator instance = null;
        public static CubeNavigator GetInstance()
        {
            if (instance == null)
            {
                Debug.Assert(mainWindow != null);
                instance = new CubeNavigator();
            }
            return instance;
        }
        public static void InitializeInstance(MainWindow window)
        {
            mainWindow = window;

        }
        private CubeNavigator()
        {
            cubeNavViewport = mainWindow.CubeNavViewport;
            cubeNavModel = mainWindow.CubeNavModel;
            cubeNavViewport.MouseDown += cubeNavViewport_ButtonDown;
            mainWindow.MouseMove += cubeNavViewport_MouseMove;
            mainWindow.MouseUp += cubeNavViewport_ButtonUp;
        }

        #endregion

        public void cubeNavViewport_ButtonDown(Object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(mainWindow);
            isDraging = true;
        }

        public void cubeNavViewport_ButtonUp(Object sender, MouseButtonEventArgs e)
        {
            isDraging = false;
        }

        public void cubeNavViewport_MouseMove(Object sender, MouseEventArgs e)
        {
            if (!isDraging)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 计算Cube的旋转
                Point currMousePos = e.GetPosition(mainWindow);
                if (currMousePos.Equals(lastMousePos))
                    return;
                Vector offsetVec = currMousePos - lastMousePos;
                Double rotDeg = offsetVec.Length;
                System.Windows.Media.Media3D.Vector3D mouseDir = new System.Windows.Media.Media3D.Vector3D(offsetVec.X, -offsetVec.Y, 0);
                System.Windows.Media.Media3D.Vector3D foreDir = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);
                System.Windows.Media.Media3D.Vector3D axisOfRatate = System.Windows.Media.Media3D.Vector3D.CrossProduct(mouseDir, foreDir);
                axisOfRatate.Normalize();
                System.Windows.Media.Media3D.Quaternion quar = new System.Windows.Media.Media3D.Quaternion(axisOfRatate, rotDeg);
                quar = quar * lastQuat;
                System.Windows.Media.Media3D.QuaternionRotation3D rot3d = new System.Windows.Media.Media3D.QuaternionRotation3D(quar);
                System.Windows.Media.Media3D.RotateTransform3D rotts = new System.Windows.Media.Media3D.RotateTransform3D(rot3d);
                cubeNavModel.Transform = rotts;
                
                // 让CloverRoot模仿cube的动作
                RenderController.GetInstance().SrcQuaternion = quar;
                RenderController.GetInstance().RotateTransform = rotts;

                lastQuat = quar;
                lastMousePos = currMousePos;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                 // 计算纸张的平移量
                Point currMousePos = e.GetPosition(mainWindow);
                if (currMousePos.Equals(lastMousePos))
                    return;
                Vector offsetVec = currMousePos - lastMousePos;

                RenderController.GetInstance().TranslateTransform.OffsetX += offsetVec.X;
                RenderController.GetInstance().TranslateTransform.OffsetY -= offsetVec.Y;

                lastMousePos = currMousePos;
            }

        }

    }
}
