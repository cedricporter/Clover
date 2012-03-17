using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Clover.Visual;
using System.Windows.Input;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	tuck工具
**/

namespace Clover.Tool
{
    class TuckTool : ToolFactory
    {

        Face nearestFace = null;
        Vertex pickedVertex = null;
        Point3D projectionPoint;
        CurrentModeVisual currentModeVi = null;

        enum FoldingMode
        {
            DoingNothing, TuckingIn
        }
        FoldingMode mode = FoldingMode.DoingNothing;

        public TuckTool(MainWindow mainWindow)
            : base(mainWindow)
        {

        }


        protected override void onSelectElement(Object element)
        {
            // 将视角变换到与当前纸张平行
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                pickedVertex = (Vertex)element;
                // 寻找所有拥有该点的面的集合
                List<Face> faces = CloverTreeHelper.GetReferenceFaces(pickedVertex);
                // 首先寻找离我们最近的那个面……
                // 按照道理nearestFace是不可能为空的
                FindNearestFace(faces);

                // 进入折叠模式，传递给下层


                // 锁定视角
                LockViewport(true);
                // 锁定鼠标OnPress和OnMove
                IsOnMoveLocked = true;
                IsOnPressLocked = true;

                EnterTuckingIn();

            }
        }

        protected override void onDrag(Object element)
        {
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                // 求2D到3D的投影点
                projectionPoint = Get3DProjectionPoint();

                // 传给下层
            }
        }

        protected override void exit()
        {
            if (mode == FoldingMode.TuckingIn)
                ExitTuckingIn();

            currSelectedElementVi.End();
            currSelectedElementVi = null;
            currOveredElementVi.End();
            currOveredElementVi = null;
            currSelectedElement = currOveredElement = null;
            IsOnMoveLocked = false;
            IsOnPressLocked = false;
            LockViewport(false);

            // 向下层传递

            mode = FoldingMode.DoingNothing;
        }

        public override void onIdle()
        {
            
        }

        void EnterTuckingIn()
        {
            mode = FoldingMode.TuckingIn;

            // 显示模式
            currentModeVi = new CurrentModeVisual("Tucking In Mode");
            VisualController.GetSingleton().AddVisual(currentModeVi);
            currentModeVi.Start();
        }

        void ExitTuckingIn()
        {
            mode = FoldingMode.DoingNothing;

            currentModeVi.End();
            currentModeVi = null;
        }

        /// <summary>
        /// 锁定或解锁视角
        /// </summary>
        /// <param name="islock"></param>
        void LockViewport(Boolean islock)
        {
            CubeNavigator cubeNav = CubeNavigator.GetInstance();
            Viewport3D cubeNavViewport = cubeNav.CubeNavViewport;
            if (islock)
            {
                cubeNavViewport.PreviewMouseDown -= cubeNav.cubeNavViewport_ButtonDown;
                // mainWindow.MouseMove -= mainWindow.Window_TranslatePaper;
            }
            else
            {
                cubeNavViewport.PreviewMouseDown += cubeNav.cubeNavViewport_ButtonDown;
                // mainWindow.MouseMove += mainWindow.Window_TranslatePaper;
            }
        }

        /// <summary>
        /// 求2D到3D的投影点
        /// 杨旭瑜提供
        /// </summary>
        /// <returns></returns>
        Point3D Get3DProjectionPoint()
        {
            Matrix3D to3DMat = Utility.GetInstance().To2DMat;
            if (!to3DMat.HasInverse)
                return new Point3D(0, 0, 0);
            to3DMat.Invert();
            Point3D start = new Point3D(currMousePos.X, currMousePos.Y, 0.0000001);
            Point3D end = new Point3D(currMousePos.X, currMousePos.Y, 0.9999999);
            start *= to3DMat;
            end *= to3DMat;

            return CloverMath.IntersectionOfLineAndPlane(start, end, nearestFace.Normal, nearestFace.Vertices[0].GetPoint3D());
        }

        /// <summary>
        /// 寻找离我们最近的那个面
        /// </summary>
        /// <param name="faces"></param>
        void FindNearestFace(List<Face> faces)
        {
            Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;
            Double minVal = Double.MaxValue;

            foreach (Face f in faces)
            {
                Double val = 0;
                int count = 0;
                foreach (Vertex v in f.Vertices)
                {
                    Vector3D vec = new Vector3D(v.GetPoint3D().X, v.GetPoint3D().Y, v.GetPoint3D().Z);
                    vec *= mat;
                    val += vec.Length;
                    count++;
                }
                val /= count;
                if (minVal > val)
                {
                    minVal = val;
                    nearestFace = f;
                }
            }
        }



        protected override void onEnterElement(Object element)
        {
            
        }

        protected override void onLeaveElement(Object element)
        {
            
        }

        protected override void onOverElement(Object element)
        {
            
        }

        protected override void onUnselectElement(Object element)
        {
            
        }

        protected override void onClick()
        {
            if (mode == FoldingMode.DoingNothing)
                return;

            if (Mouse.RightButton == MouseButtonState.Pressed)
                exit();
        }

    }
}
