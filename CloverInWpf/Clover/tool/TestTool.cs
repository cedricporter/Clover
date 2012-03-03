using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;

namespace Clover.Tool
{
    class TestTool : ToolFactory
    {   
        public TestTool(MainWindow mainWindow) : base(mainWindow)
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
                
                Vertex vertex = (Vertex)element;
                List<Face> faces = CloverController.GetInstance().GetReferencedFaces(vertex);

                // 首先寻找离我们最近的那个面……
                Double minVal = Double.MaxValue;
                Face nearestFace = null;
                Matrix3D mat = RenderLayer.RenderController.GetInstance().Entity.Transform.Value;
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
                // 按照道理nearestFace是不可能为空的
                // 判断该面是正面朝向用户还是背面朝向用户
                Vector3D vector1 = nearestFace.Normal * mat;
                Vector3D vector2 = new Vector3D(0,0,1);
                if (Vector3D.DotProduct(vector1, vector2) < 0)
                    vector1 = nearestFace.Normal * -1;
                else
                    vector1 = nearestFace.Normal;
                // 计算旋转
                //RotateTransform3D rot;
                Quaternion quat;
                if (vector1 == nearestFace.Normal)
                    //rot = new RotateTransform3D();
                    quat = new Quaternion();
                else if (vector1 == nearestFace.Normal * -1)
                    //rot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 180));
                    quat = new Quaternion(new Vector3D(0, 1, 0), 180);
                else
                {
                    Vector3D axis = Vector3D.CrossProduct(vector1, vector2);
                    axis.Normalize();
                    Double deg = Vector3D.AngleBetween(vector1, vector2);
                    //rot = new RotateTransform3D(new AxisAngleRotation3D(axis, deg));
                    quat = new Quaternion(axis, deg);
                }

                // 应用旋转
                RenderLayer.RenderController.GetInstance().BeginRotationSlerp(quat);
                //RenderLayer.RenderController.GetInstance().RotateTransform = rot;
                //RenderLayer.RenderController.GetInstance().RotateTransform.BeginAnimation(RotateTransform3D.RotationProperty)
                //RenderLayer.RenderController.GetInstance().Entity

            }
            #endregion
        }

        protected override void onUnselectElement(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void onDrag(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }
    }
}
