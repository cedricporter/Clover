using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows.Controls;
using Clover.Visual;
using System.Windows;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	折叠工具
**/



namespace Clover.Tool
{
    class FoldTool : ToolFactory
    {
        Face nearestFace = null;
        Vertex pickedVertex = null;
        Vertex pickedVertexOrigin = null;
        Point pickedVertex2DPos = new Point();
        Point Origin2Dpos = new Point();
        CurrentModeVisual vi = null;

        public FoldTool(MainWindow mainWindow)
            : base(mainWindow)
        {
            //System.Windows.MessageBox.Show();
        }

        protected override void onEnterElement(Object element)
        {
            //Type t = element.GetType();
            Debug.WriteLine(element.GetType());
        }

        protected override void onLeaveElement(Object element)
        {
            //Debug.WriteLine(element.GetType());
        }

        protected override void onSelectElement(Object element)
        {
            #region 选取到的是点
            // 将视角变换到与当前纸张平行
            if (element.GetType().ToString() == "Clover.Vertex")
            {
                
                pickedVertex = (Vertex)element;
                // 寻找所有拥有该点的面的集合
                List<Face> faces = CloverTreeHelper.GetReferenceFaces(pickedVertex);

                // 首先寻找离我们最近的那个面……
                // 按照道理nearestFace是不可能为空的
                FindNearestFace(faces);

                // 计算旋转
                Quaternion quat = CalculateRotation();

                // 应用旋转
                RenderController.GetInstance().BeginRotationSlerp(quat);

                // 进入折叠模式
                // 锁定视角
                LockViewport(true);
                // 锁定鼠标OnPress和OnMove
                IsOnMoveLocked = true;
                IsOnPressLocked = true;
                // 显示模式
                vi = new CurrentModeVisual("Folding Mode");
                VisualController.GetSingleton().AddVisual(vi);
                vi.Start();
                // 计算并记录选择点的2D位置
                Point3D p3d = pickedVertex.GetPoint3D();
                p3d *= Utility.GetInstance().To2DMat;
                Origin2Dpos = new Point(p3d.X, p3d.Y); 
            }
            #endregion
        }

        protected override void onUnselectElement(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void onOverElement(Object element)
        {

        }

        protected override void onDrag(Object element)
        {
            #region 如果选中的是点

            if (element.GetType().ToString() == "Clover.Vertex")
            {
                // 视觉效果
                currSelectedElementVi.TransformGroup = new TranslateTransform(currMousePos.X, currMousePos.Y);
                // 求2D到3D的投影点
                Point3D projectionPoint = Get3DProjectionPoint();
                // 传给下一层处理
                // todo
                CloverController.GetInstance().ProjectionPoint = projectionPoint;
                CloverController.GetInstance().PickedPoint = pickedVertex.GetPoint3D();
            }
            

            #endregion

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
        /// 计算旋转量
        /// </summary>
        /// <returns></returns>
        Quaternion CalculateRotation()
        {
            Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;

            // 判断该面是正面朝向用户还是背面朝向用户
            Vector3D vector1 = nearestFace.Normal * mat;
            Vector3D vector2 = new Vector3D(0, 0, 1);
            if (Vector3D.DotProduct(vector1, vector2) < 0)
                vector1 = nearestFace.Normal * -1;
            else
                vector1 = nearestFace.Normal;
            // 计算旋转
            Quaternion quat;
            if (vector1 == new Vector3D(0, 0, 1))
                quat = new Quaternion();
            else if (vector1 == new Vector3D(0, 0, -1))
                quat = new Quaternion(new Vector3D(0, 1, 0), 180);
            else
            {
                Vector3D axis = Vector3D.CrossProduct(vector1, vector2);
                axis.Normalize();
                Double deg = Vector3D.AngleBetween(vector1, vector2);
                quat = new Quaternion(axis, deg);
            }
            return quat;
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
                cubeNavViewport.MouseLeftButtonDown -= cubeNav.cubeNavViewport_MouseLeftButtonDown;
                cubeNavViewport.MouseMove -= cubeNav.cubeNavViewport_MouseMove;
            }
            else
            {
                cubeNavViewport.MouseLeftButtonDown += cubeNav.cubeNavViewport_MouseLeftButtonDown;
                cubeNavViewport.MouseMove += cubeNav.cubeNavViewport_MouseMove;
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


    }
}
