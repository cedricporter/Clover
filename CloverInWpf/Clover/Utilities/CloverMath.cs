using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Clover
{
    public class CloverMath
    {
        /// <summary>
        /// 判断平面上的两点是否相等
        /// </summary>
        /// <param name="p1">点P1</param>
        /// <param name="p2">点P2</param>
        /// <param name="threadhold">容许的误差值</param>
        /// <returns>返回true如果判断成立</returns>
        /// <author>Kid</author>
        public static Boolean IsTwoPointsEqual(Point p1, Point p2, Double threadhold)
        {
            if ((p2 - p1).Length < threadhold)
                return true;
            return false;
        }

        /// <summary>
        /// 判断空间中的两点是否相等
        /// </summary>
        /// <param name="p1">点P1</param>
        /// <param name="p2">点P2</param>
        /// <param name="threadhold">容许的误差值</param>
        /// <returns>返回true如果判断成立</returns>
        /// <author>Kid</author>
        public static Boolean IsTwoPointsEqual(Point3D p1, Point3D p2, Double threadhold)
        {
            if ((p2 - p1).Length < threadhold)
                return true;
            return false;
        }

        /// <summary>
        /// 判断点p是否处在空间线段p1,p2上
        /// </summary>
        /// <param name="p">要判断的点</param>
        /// <param name="p1">线段的端点p1</param>
        /// <param name="p2">线段的端点p2</param>
        /// <param name="threadhold">容许的误差值</param>
        /// <returns>返回true如果判断成立</returns>
        /// <author>Kid</author>
        public static Boolean IsPointInTwoPoints(Point3D p, Point3D p1, Point3D p2, Double threadhold)
        {
            Vector3D V1 = p2 - p1;
            Vector3D V2 = p - p1;
            Double t = Vector3D.DotProduct(V1, V2) / Vector3D.DotProduct(V1, V1);
            Point3D p3 = p1 + t * V1;
            if ((p - p3).Length < threadhold)
            {
                if (Vector3D.DotProduct((p - p1), (p - p2)) < 0)
                    return true;
            } 
            return false;
        }

        /// <summary>
        /// 判断点p是否处在平面线段p1,p2上
        /// </summary>
        /// <param name="p">要判断的点</param>
        /// <param name="p1">线段的端点p1</param>
        /// <param name="p2">线段的端点p2</param>
        /// <param name="threadhold">容许的误差值</param>
        /// <returns>返回true如果判断成立</returns>
        /// <author>Kid</author>
        public static Boolean IsPointInTwoPoints(Point p, Point p1, Point p2, Double threadhold)
        {
            Vector V1 = p2 - p1;
            Vector V2 = p - p1;
            Double t = (V1 * V2) / (V1 * V1);
            Point p3 = p1 + t * V1;
            if ((p - p3).Length < threadhold)
            {
                if (((p - p1) * (p - p2)) < 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断直线p1,p2
        /// </summary>
        /// <param name="p1">直线上的一点p1</param>
        /// <param name="p2">直线上的一点p2</param>
        /// <param name="face">平面</param>
        /// <returns>交点</returns>
        /// <remarks>注意！该函数并不提供平行检测！</remarks>
        /// <author>Kid</author>
        public static Point3D IntersectionOfLineAndPlane(Point3D p1, Point3D p2, Face face)
        {
            Vector3D N = face.Normal;
            Point3D Pon = face.Vertices[0].GetPoint3D();
            Double D = Vector3D.DotProduct((p2 - p1), N);
            //if (D < 0.0001)
            //    return (Point3D)null;
            Double t = -Vector3D.DotProduct((p1 - Pon), N) / D;
            Point3D Pt = p1 + t * (p2 - p1);
            return Pt;
        }

        /// <summary>
        /// 求一线段的中垂线
        /// </summary>
        /// <param name="p1">线段的一个端点，计算完后会由该变量返回中垂线的一个端点</param>
        /// <param name="p2">线段的另一个端点，计算完后会由该变量返回中垂线的另一个端点</param>
        /// <author>Kid</author>
        public static void GetPerpendicularBisector(ref Point p1, ref Point p2)
        {
            Vector V1 = p2 - p1;
            Point Pmid = p1 + V1 / 2;
            Vector V2 = new Vector(V1.Y, -V1.X);
            V2.Normalize();
            p1 = Pmid + 1000 * V2;
            p2 = Pmid - 1000 * V2;
        }

        
    }
}
