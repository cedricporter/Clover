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
        Face nearestFace = null;

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
                Matrix3D mat = RenderController.GetInstance().Entity.Transform.Value;
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
                if (vector1 == new Vector3D(0,0,1))
                    //rot = new RotateTransform3D();
                    quat = new Quaternion();
                else if (vector1 == new Vector3D(0,0,-1))
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
                RenderController.GetInstance().BeginRotationSlerp(quat);
            }
            #endregion
        }

        protected override void onUnselectElement(Object element)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        protected override void onDrag(Object element)
        {
            #region 如果选中的是点

            if (element.GetType().ToString() == "Clover.Vertex")
            {
                Vertex pickedVertex = (Vertex)element;
                //Point3D pickedVertex = ((Vertex)element).GetPoint3D();
                // 鼠标拖动顶点，动态生成折线
                // 记录该点的原始位置
                Vertex prevVertex = CloverController.GetInstance().GetPrevVersion(pickedVertex);
                if (prevVertex == null)
                    return;
                Point3D pOrigin = prevVertex.GetPoint3D() ;

                Matrix3D to3DMat = Utility.GetInstance().To2DMat;
                if (!to3DMat.HasInverse)
                    return;
                to3DMat.Invert();
                Point3D start = new Point3D(currMousePos.X, currMousePos.Y, 0.0000001);
                Point3D end = new Point3D(currMousePos.X, currMousePos.Y, 0.9999999);
                start *= to3DMat;
                end *= to3DMat;
                Point3D P1 = new Point3D();
                CloverMath.IntersectionOfLineAndFace(start, end, nearestFace, ref P1);
                
                

                //// 获取当前鼠标位置，并转换为3D空间中的射线
                //Point3D p1 = new Point3D(currMousePos.X, currMousePos.Y, 0);
                //Matrix3D to3DMat = Utility.GetInstance().To2DMat;
                //if (!to3DMat.HasInverse)
                //    return;
                //to3DMat.Invert();
                //p1 *= to3DMat;
                //p1.Z = 0;
                //Point3D p2 = p1 + new Vector3D(0, 0, 3000);
                //Point3D Pon = nearestFace.Vertices[0].GetPoint3D();
                //Vector3D N = nearestFace.Normal;
                //// 已知线上两点p1p2,面法线N，面上一点Pon，求射线与面的交点Pt
                //Double t = -Vector3D.DotProduct((p1 - Pon), N) / Vector3D.DotProduct((p2 - p1), N);
                //Point3D Pt = p1 + t * (p2 - p1);
                //// 求Pt与原来的点P0的垂直平分线
                //Point3D P0 = ((Vertex)element).GetPoint3D();
                //Vector3D Vcon = Pt - P0;
                //Point3D Pmid = P0 + Vcon / 2;
                //Vector3D Vver = new Vector3D(Vcon.Y, -Vcon.X, 0);
                //Vver.Normalize();
            }
            

            #endregion

        }
    }
}
