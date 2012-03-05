﻿using System;
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
        public static Boolean IsPointInTwoPoints(Point3D p, Point3D p1, Point3D p2, Double threadhold = 0.0001)
        {
            Vector3D V1 = p2 - p1;
            Vector3D V2 = p - p1;
            Double t = Vector3D.DotProduct(V1, V2) / Vector3D.DotProduct(V1, V1);
            Point3D p3 = p1 + t * V1;
            if ((p - p3).Length < threadhold)
            {
                if (Vector3D.DotProduct((p - p1), (p - p2)) <= 0)
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
        public static Boolean IsPointInTwoPoints(Point p, Point p1, Point p2, Double threadhold = 0.0001)
        {
            Vector V1 = p2 - p1;
            Vector V2 = p - p1;
            Double t = (V1 * V2) / (V1 * V1);
            Point p3 = p1 + t * V1;
            if ((p - p3).Length < threadhold)
            {
                if (((p - p1) * (p - p2)) <= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断直线p1,p2
        /// </summary>
        /// <param name="p1">直线上的一点p1</param>
        /// <param name="p2">直线上的一点p2</param>
        /// <param name="N">平面的法向量</param>
        /// <param name="Pon">平面上的一点</param>
        /// <returns>交点</returns>
        /// <remarks>注意！该函数并不提供平行检测！</remarks>
        /// <author>Kid</author>
        public static Point3D IntersectionOfLineAndPlane(Point3D p1, Point3D p2, Vector3D N, Point3D Pon)
        {
            Double D = Vector3D.DotProduct((p2 - p1), N);
            //if (D < 0.0001)
            //    return (Point3D)null;
            Double t = -Vector3D.DotProduct((p1 - Pon), N) / D;
            Point3D Pt = p1 + t * (p2 - p1);
            return Pt;
        }

        /// <summary>
        /// 判断直线是否与一个face有交点，如果有，则在最后一个参数传出来。
        /// </summary>
        /// <param name="p1">直线上的点1</param>
        /// <param name="p2">直线上的点2</param>
        /// <param name="f"></param>
        /// <param name="v">结果传出</param>
        /// <returns></returns>
        public static bool IntersectionOfLineAndFace(Point3D p1, Point3D p2, Face f, ref Point3D p)
        {
            Vector3D vline = p1 - p2;
            // 排除平行的情况
            if ( Math.Abs( Vector3D.DotProduct(vline, f.Normal)) < 0.000001 )
            {
                return false;
            }
            Point3D IntersectiongPoint = IntersectionOfLineAndPlane(p1, p2, f.Normal, f.Vertices[0].GetPoint3D());

            // 判断点在不在face内
            Point3D[] points = new Point3D[f.Vertices.Count];
            for (int i = 0; i < points.Length; i++)
                points[i] = f.Vertices[i].GetPoint3D();
            if (IsPointInArea(IntersectiongPoint, points))
            {
                p = IntersectiongPoint;
                return true;
            }
            return false;
                
            //Vector3D[] vector = new Vector3D[f.Vertices.Count];
            //int i = 0;
            //foreach (Vertex ve in f.Vertices)
            //{
            //    vector[i] = IntersectiongPoint - ve.GetPoint3D();
            //    i++;
            //}

            //foreach ( Vector3D v1 in vector)
            //{
            //    foreach ( Vector3D v2 in vector)
            //    {
            //        if ( Vector3D.DotProduct( v1, v2 ) <= 0 )
            //        {
            //            p = IntersectiongPoint;
            //            return true;
            //        }

            //    }
            //}
            //return false;
             
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

        /// <summary>
        /// 求两线段之间最短距离
        /// </summary>
        /// <param name="e1">线段1</param>
        /// <param name="e2">线段2</param>
        /// <returns></returns>
        public static double GetDistanceBetweenTwoSegments(Edge e1, Edge e2)
        {
            Vector3D u = new Vector3D();
            u = e1.Vertex2.GetPoint3D() - e1.Vertex1.GetPoint3D();
            Vector3D v = new Vector3D();
            v = e2.Vertex2.GetPoint3D() - e2.Vertex1.GetPoint3D();
            Vector3D w = new Vector3D();
            w = e1.Vertex1.GetPoint3D() - e2.Vertex1.GetPoint3D();

            double a = Vector3D.DotProduct(u, u);
            double b = Vector3D.DotProduct(u, v);
            double c = Vector3D.DotProduct(v, v);
            double d = Vector3D.DotProduct(u, w);
            double e = Vector3D.DotProduct(v, w);
            double D = a * c - b * b;
            double sc, sN, sD = D;
            double tc, tN, tD = D;

            // 两条线平行
            if (D < Double.MinValue)
            {
                sN = 0.0f;
                sD = 1.0f;
                tN = e;
                tD = c;
            }
            else
            {
                sN = (b * e - c * d);
                tN = (a * e - b * d);

                if (sN < 0.0)
                {
                    sN = 0.0;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0)
            {
                tN = 0.0;
                if (-d < 0.0)
                {
                    sN = 0.0;
                }
                else if (-d < a)
                {
                    sN = sD;
                }
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {
                // tc > 1 => the t=1 edge is visible        
                tN = tD;
                // recompute sc for this edge      
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = (Math.Abs(sN) < Double.MinValue ? 0.0 : sN / sD);
            tc = (Math.Abs(tN) < Double.MinValue ? 0.0 : tN / tD);
            // get the difference of the two closest points     
            Vector3D dP = w + (sc * u) - (tc * v);
            // = S1(sc) - S2(tc)    
            return dP.Length;   // return the closest distance
        }

        /// <summary>
        /// 计算一个面中两点的中垂线，结果在参数从输出
        /// </summary>
        /// <param name="f"></param>
        /// <param name="po"></param>
        /// <param name="pd"></param>
        /// <param name="t">直线的方向向量</param>
        /// <param name="p">直线上的一点</param>
        public static bool GetMidperpendicularInFace( Face f, Vertex po, Vertex pd, ref Vector3D t, ref Point3D p)
        {
            if ( CloverMath.IsTwoPointsEqual( po.GetPoint3D(), pd.GetPoint3D(), 0.00001 ) ) 
            {
                return false;
            }

            Point3D pOriginal = po.GetPoint3D();
            Point3D pDestination = pd.GetPoint3D();
            t = Vector3D.CrossProduct( f.Normal, ( pOriginal - pDestination ) );
            
            p.X = ( po.X + pd.X ) / 2;
            p.Y = ( po.Y + pd.Y ) / 2;
            p.Z = ( po.Z + pd.Z ) / 2;
            return true;

        }
        
        /// <summary>
        /// 求补集
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static double PerpProduct(Vector3D u, Vector3D v)
        {
            return (u.X * v.Y - v.X * u.Y);
        }

        /// <summary>
        /// 计算两线段之间的交点
        /// </summary>
        /// <param name="e1">线段1</param>
        /// <param name="e2">线段2</param>
        /// <param name="intersection"></param>
        /// <returns>0:不相交 1:相交且有唯一交点 2:线段覆盖在另一线段上</returns>
        public static int GetIntersectionOfTwoSegments(Edge e1, Edge e2, ref Point3D intersection)
        {
            Vector3D u = e1.Vertex2.GetPoint3D() - e1.Vertex1.GetPoint3D();
            Vector3D v = e2.Vertex2.GetPoint3D() - e2.Vertex1.GetPoint3D();
            Vector3D w = e1.Vertex1.GetPoint3D() - e2.Vertex1.GetPoint3D();
            double D = PerpProduct(u, v);

            //判断两线段是否平行
            if (Math.Abs(D) < Double.MinValue)
            {
                if (PerpProduct(u, w) != 0 || PerpProduct(v, w) != 0)
                {
                    return 0;   // 两线段不相交 
                }

                double du = Vector3D.DotProduct(u, u);
                double dv = Vector3D.DotProduct(v, v);

                if (du == 0 && dv == 0)
                { 
                    // 两条线段是两个不同的点
                    if (e1.Vertex1.GetPoint3D() == e2.Vertex2.GetPoint3D())
                        return 0;
                    else
                    {
                        intersection.X = e1.Vertex1.X;
                        intersection.Y = e1.Vertex1.Y;
                        intersection.Z = e1.Vertex1.Z;
                        return 1;
                    }
                }
                if (du == 0)
                { 
                    // 假如其中一条线段是一个点
                    return 0;
                }
                if (dv == 0)
                { 
                    // 同上 
                    return 0;
                }

                // 两线段是非平行线段
                double t0, t1;
                Vector3D w2 = e1.Vertex2.GetPoint3D() - e2.Vertex1.GetPoint3D();
                if (v.X != 0)
                {
                    t0 = w.X / v.X;
                    t1 = w2.X / v.X;
                }
                else
                {
                    t0 = w.Y / v.Y;
                    t1 = w2.Y / v.Y;
                }
                if (t0 > t1)
                {
                    double t = t0; t0 = t1; t1 = t; 
                }
                if (t0 > 1 || t1 < 0)
                {
                    return 0;   // 没有重叠 
                }

                t0 = t0 < 0 ? 0 : t0;
                t1 = t1 > 1 ? 1 : t1;

                if (t0 == t1)
                {
                    intersection.X = e2.Vertex1.X + t0 * v.X;
                    intersection.Y = e2.Vertex1.Y + t0 * v.Y;
                    intersection.Z = e2.Vertex1.Z + t0 * v.Z;
                    return 1;
                }

                return 2;
            }
            double sI = PerpProduct(v, w) / D;
            if (sI < 0 || sI > 1)
                return 0;
            double tI = PerpProduct(u, w) / D;
            if (tI < 0 || tI > 1)
                return 0;

            intersection.X = e1.Vertex1.X + sI * u.X;
            intersection.Y = e1.Vertex1.Y + sI * u.Y;
            intersection.Z = e1.Vertex1.Z + sI * u.Z;
            return 1;
        }


        /// <summary>
        /// 判断一个3D点是否处在由points围成的平面内
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <param name="points">平面的边界</param>
        /// <returns>返回true如果点在平面内或平面边界上</returns>
        /// <author>kid</author>
        public static Boolean IsPointInArea(Point3D pe, Point3D[] points)
        {
            Vector3D lastN = new Vector3D(0, 0, 0);
            for (int i = 0; i < points.Length; i++)
            {
                Vector3D v1 = pe - points[i];
                Vector3D v2 = points[(i + 1) % points.Length] - pe;
                Vector3D currN = Vector3D.CrossProduct(v1, v2);
                if (Vector3D.DotProduct(currN, lastN) < 0)
                    return false;
                lastN = currN;
            }
                return true;
        }




        /// <summary>
        /// 判断一个点是否位于一个face中
        /// </summary>
        /// <param name="p"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsPointInFace(Point3D p, Face f)
        {
            Vector3D[] vector = new Vector3D[ f.Vertices.Count ];
            int i = 0;
            foreach ( Vertex ve in f.Vertices )
            {
                vector[ i ] = p - ve.GetPoint3D();
                i++;
            }

            foreach ( Vector3D v1 in vector )
            {
                foreach ( Vector3D v2 in vector )
                {
                    if ( Vector3D.DotProduct( v1, v2 ) < 0 )
                    {
                        return true;
                    }

                }
            }
            return false;
        }


        /// <summary>
        /// 判断两个face是否相交
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool IsIntersectionOfTwoFace(Face f1, Face f2)
        {
            foreach (Vertex v in f1.Vertices)
            {
                if ( IsPointInFace( v.GetPoint3D(), f2 ) )
                    return true;
            }
            return false;
        }
    }
}
