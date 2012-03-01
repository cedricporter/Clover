using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using _3DTools;

namespace Clover
{
    public class Utility
    {
        Matrix3D CameMat = Matrix3D.Identity; ///从世界坐标系转换到摄像机坐标系的矩阵
        Matrix3D ProjMat = Matrix3D.Identity; ///投影矩阵
        Matrix3D ViewMat = Matrix3D.Identity; ///从视口到屏幕的矩阵
        Matrix3D to2DMat;
                          ///


        public Utility()
        {
            

        }

        /// <summary>
        /// 当窗口大小发生改变时，更新视口矩阵
        /// </summary>
        public void UpdateProjMat()
        {

        }

        /// <summary>
        /// 当窗口大小发生改变时，更新视口矩阵
        /// </summary>
        public void UpdateViewMat(Double height, double width)
        {
            double aspectRatio = width / height;
            double FoV = MathUtils.DegreesToRadians(45);
            double zn = 0.125;
            double xScale = 1 / Math.Tan(FoV / 2);
            double yScale = aspectRatio * xScale;
            double m33 = -1;
            double m43 = zn * m33;
            to2DMat = new Matrix3D(
                xScale, 0, 0, 0,
                0, yScale, 0, 0,
                0, 0, m33, -1,
                0, 0, m43, 0);

            double scaleX = width / 2;
            double scaleY = height / 2;

            to2DMat.Append(new Matrix3D(
                scaleX, 0, 0, 0,
                0, -scaleY, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1));
        }
        //public static Point Convert3DTo2D(Point3D point3D)
        //{

        //}
    }
}
