using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover.Visual;
using System.Windows.Controls;
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
@note		:	Blending工具
**/

namespace Clover.Tool
{
    
    class BlendTool : ToolFactory
    {
        Vertex pickedVertex;
        Face nearestFace;
        Point3D projectionPoint;
        CurrentModeVisual currentModeVi = null;
        enum FoldingMode
        {
            DoingNothing, Blending
        }
        FoldingMode mode = FoldingMode.DoingNothing;

        // 仅供测试用
        Clover.AbstractLayer.Blending blendingTest = new Clover.AbstractLayer.Blending();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        public BlendTool(MainWindow mainWindow)
            : base(mainWindow)
        { }

        public override void onIdle()
        {

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

        protected override void onSelectElement(Object element)
        {
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                pickedVertex = (Vertex)element;
                // 寻找所有拥有该点的面的集合
                List<Face> faces = CloverTreeHelper.GetReferenceFaces(pickedVertex);
                // 首先寻找离我们最近的那个面……
                // 按照道理nearestFace是不可能为空的
                FindNearestFace(faces);
                // 求2D到3D的投影点
                projectionPoint = Get3DProjectionPoint();
                // 锁定视角
                LockViewport(true);
                // 锁定鼠标OnPress和OnMove
                IsOnMoveLocked = true;
                IsOnPressLocked = true;

                // 进入Blending模式
                EnterBlending();

                // 向下层传递数据
                blendingTest.EnterBlendingMode(pickedVertex, nearestFace);

            }
        }

        protected override void onUnselectElement(Object element)
        {
            
        }

        protected override void onDrag(Object element)
        {
            Double offsetX = currMousePos.X - lastMousePos.X;
            blendingTest.OnDrag(offsetX);
        }

        protected override void onClick()
        {
            if (mode == FoldingMode.DoingNothing)
                return;

            if (Mouse.RightButton == MouseButtonState.Pressed)
                exit();
        }

        protected override void exit()
        {
            ExitBlending();
            currSelectedElementVi.End();
            currSelectedElementVi = null;
            currOveredElementVi.End();
            currOveredElementVi = null;
            currSelectedElement = currOveredElement = null;
            IsOnMoveLocked = false;
            IsOnPressLocked = false;
            LockViewport(false);
        }

        void EnterBlending()
        {
            mode = FoldingMode.Blending;
            // 计算旋转
            Quaternion quat = CalculateBlendingRotation();
            // 应用旋转
            RenderController.GetInstance().BeginRotationSlerp(quat);
            // 显示模式
            currentModeVi = new CurrentModeVisual("Blending Mode");
            VisualController.GetSingleton().AddVisual(currentModeVi);
            currentModeVi.Start();
        }

        void ExitBlending()
        {
            mode = FoldingMode.DoingNothing;
            currentModeVi.End();
            currentModeVi = null;

            // 向下层传递退出Blending模式
            blendingTest.ExitBlendingMode();
        }

        /// <summary>
        /// 计算旋转量
        /// </summary>
        /// <returns></returns>
        Quaternion CalculateBlendingRotation()
        {
            Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;

            // 判断该面是正面朝向用户还是背面朝向用户
            Vector3D vector1 = nearestFace.Normal * mat;
            Vector3D vector2 = new Vector3D(0, 0, 1);
            Vector3D vector3 = projectionPoint - pickedVertex.GetPoint3D();
            if (Vector3D.DotProduct(vector1, vector2) < 0)
                vector1 = nearestFace.Normal * -1;
            else
                vector1 = nearestFace.Normal;
            // 计算旋转
            Quaternion quat;
            if (vector1 == new Vector3D(0, 0, 1))
            {
                quat = new Quaternion(new Vector3D(1, 0, 0), -70);
            }
            else if (vector1 == new Vector3D(0, 0, -1))
                quat = new Quaternion(new Vector3D(1, 0, 0), 110);
            else
            {
                Vector3D axis = Vector3D.CrossProduct(vector1, vector2);
                axis.Normalize();
                Double deg = Vector3D.AngleBetween(vector1, vector2);
                quat = new Quaternion(axis, deg);
                quat = new Quaternion(new Vector3D(1, 0, 0), -70) * quat;
            }
            return quat;
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
            }
            else
            {
                cubeNavViewport.PreviewMouseDown += cubeNav.cubeNavViewport_ButtonDown;
            }
        }


    }
}
