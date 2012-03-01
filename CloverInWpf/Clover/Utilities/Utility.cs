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
        Matrix3D WorlCameMat = Matrix3D.Identity; ///从世界坐标系转换到摄像机坐标系的矩阵
        Matrix3D ProjViewMat = Matrix3D.Identity; ///投影矩阵
        Matrix3D to2DMat;
        public System.Windows.Media.Media3D.Matrix3D To2DMat
        {
            get { return to2DMat; }
            //set { to2DMat = value; }
        }
                          ///

        MainWindow mainWindow;
        public Utility(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

        }

        /// <summary>
        /// 当镜头缩放时，更新由世界到镜头的矩阵
        /// </summary>
        public void UpdateWorlCameMat(Double distance)
        {
            WorlCameMat = Matrix3D.Identity;
            WorlCameMat.OffsetZ = -distance;

            UpdateTo2DMat();
        }

        /// <summary>
        /// 当窗口大小发生改变时，更新视口投影矩阵
        /// </summary>
        public void UpdateProjViewMat(Double height, double width)
        {
            double aspectRatio = width / height;
            double FoV = MathUtils.DegreesToRadians(45);
            double zn = 0.125;
            double xScale = 1 / Math.Tan(FoV / 2);
            double yScale = aspectRatio * xScale;
            double m33 = -1;
            double m43 = zn * m33;
            ProjViewMat = new Matrix3D(
                xScale, 0, 0, 0,
                0, yScale, 0, 0,
                0, 0, m33, -1,
                0, 0, m43, 0);

            double scaleX = width / 2;
            double scaleY = height / 2;

            ProjViewMat.Append(new Matrix3D(
                scaleX, 0, 0, 0,
                0, -scaleY, 0, 0,
                0, 0, 1, 0,
                scaleX, scaleY, 0, 1));

            UpdateTo2DMat();
        }

        /// <summary>
        /// 当窗口大小发生改变时，更新视口矩阵
        /// </summary>
        public void UpdateTo2DMat()
        {
            to2DMat = WorlCameMat * ProjViewMat;
        }
    }
}
