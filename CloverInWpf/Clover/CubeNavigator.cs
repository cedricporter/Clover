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

        Viewport2DVisual3D cubeFront, cubeBack, cubeUp, cubeDown, cubeLeft, cubeRight;
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeRight
        {
            get { return cubeRight; }
            set { cubeRight = value; }
        }
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeLeft
        {
            get { return cubeLeft; }
            set { cubeLeft = value; }
        }
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeDown
        {
            get { return cubeDown; }
            set { cubeDown = value; }
        }
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeUp
        {
            get { return cubeUp; }
            set { cubeUp = value; }
        }
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeBack
        {
            get { return cubeBack; }
            set { cubeBack = value; }
        }
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeFront
        {
            get { return cubeFront; }
            set { cubeFront = value; }
        }

        static MainWindow mainWindow;
        Viewport3D cubeNavViewport;
        public System.Windows.Controls.Viewport3D CubeNavViewport
        {
            get { return cubeNavViewport; }
            set { cubeNavViewport = value; }
        }
        Viewport2DVisual3D cubeNavModel;
        public System.Windows.Media.Media3D.Viewport2DVisual3D CubeNavModel
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
            //cubeNavModel = mainWindow.CubeNavModel;
            cubeFront = mainWindow.CubeFront;
            cubeBack = mainWindow.CubeBack;
            cubeLeft = mainWindow.CubeLeft;
            cubeRight = mainWindow.CubeRight;
            cubeUp = mainWindow.CubeUp;
            cubeDown = mainWindow.CubeDown;
            cubeNavViewport.PreviewMouseDown += cubeNavViewport_ButtonDown;
            mainWindow.PreviewMouseMove += cubeNavViewport_MouseMove;
            mainWindow.PreviewMouseUp += cubeNavViewport_ButtonUp;
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
                //cubeNavModel.Transform = rotts;
                cubeFront.Transform = cubeBack.Transform = cubeUp.Transform =
                     cubeDown.Transform = cubeLeft.Transform = cubeRight.Transform = rotts;

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

        public void RotateTo(String face)
        {
            Double val = Math.Sqrt(2) / 2;
            switch (face)
            {
                case "ft":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion());
                    break;
                case "bk":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion(0, 1, 0, 0));
                    break;
                case "up":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion(val, 0, 0, val));
                    break;
                case "dn":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion(val, 0, 0, -val));
                    break;
                case "lt":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion(0, val, 0, val));
                    break;
                case "rt":
                    RenderController.GetInstance().BeginRotationSlerp(new Quaternion(0, -val, 0, val));
                    break;
            }
        }

    }
}
