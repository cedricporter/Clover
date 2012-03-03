using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using Clover.RenderLayer;
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
        
        static MainWindow mainWindow;
        static Viewport3D cubeNavViewport;
        static Model3DGroup cubeNavModel;
        public System.Windows.Media.Media3D.Model3DGroup CubeNavModel
        {
            get { return cubeNavModel; }
            set 
            { 
                cubeNavModel = value;
            }
        }
        static Point lastMousePos;
        static Quaternion lastQuat = new Quaternion();
        public System.Windows.Media.Media3D.Quaternion LastQuat
        {
            get { return lastQuat; }
            set 
            { 
                lastQuat = value; 
            }
        }
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
            cubeNavViewport = mainWindow.CubeNavViewport;
            cubeNavModel = mainWindow.CubeNavModel;
            cubeNavViewport.MouseLeftButtonDown += new MouseButtonEventHandler(cubeNavViewport_MouseLeftButtonDown);
            cubeNavViewport.MouseMove += new MouseEventHandler(cubeNavViewport_MouseMove);
        }

        static private void cubeNavViewport_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(mainWindow);
        }

        static private void cubeNavViewport_MouseMove(Object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 计算Cube的旋转
                Point currMousePos = e.GetPosition(mainWindow);
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
                //RenderController.GetInstance().UpdatePosition();

                lastQuat = quar;
                lastMousePos = currMousePos;
            }

        }

    }
}
