using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover;
using System.Windows.Media;

namespace Clover.AbstractLayer
{

    class FaceSort : IComparer<Face>
    {
        /// <summary>
        /// 对组中的面进行排序
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        int IComparer<Face>.Compare(Face f1, Face f2)
        {
            if (f1.Layer > f2.Layer)
                return 1;
            else if (f1.Layer == f2.Layer)
                return 0;
            else
                return -1;
        }

    }


    /// <summary>
    /// 面组，里面的face都位于同一个plane上
    /// </summary>
    public class FaceGroup
    {
        List<Face> faceList;
        Vector3D normal = new Vector3D(); // 组的法向量
        public System.Windows.Media.Media3D.Vector3D Normal
        {
            get { return normal; }
            set { normal = value; }
        }
        public int Count
        {
            get { return faceList.Count; }
        }
        double a;
        public double A
        {
            get { return a; }
            set { a = value; }
        }
        double b;
        public double B
        {
            get { return b; }
            set { b = value; }
        }
        double c;
        public double C
        {
            get { return c; }
            set { c = value; }
        }
        double d;
        public double D
        {
            get { return d; }
            set { d = value; }
        }
        Point4D plainFormula = new Point4D();

        /// <summary>
        /// group的构造函数，会计算一个group的法向量
        /// </summary>
        /// <param name="f"></param>
        public FaceGroup(Face f)
        {
            if (f != null)
            {
                faceList = new List<Face>();
                normal = f.Normal;
                // normal.Normalize();
                faceList.Add(f);
                a = normal.X;
                b = normal.Y;
                c = normal.Z;
                d = -(f.Vertices[0].X * a + f.Vertices[0].Y * b + f.Vertices[0].Z * c);
            }
        }


        /// <summary>
        /// 向一个组中增加面
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool AddFace(Face f)
        {
            //if (IsMatch(f))
            //{
            faceList.Add(f);
            SortFace();
            return true;
            //}
            //return false;

        }
        /// <summary>
        /// 逆序组中的面
        /// </summary>
        public void RevertFaces()
        {
            int layer = 0;
            SortFace();
            for ( int i = faceList.Count; i >= 0; i-- )
            {
                faceList[ i ].Layer = layer;
                layer++;
            }
            SortFace();
        }

        /// <summary>
        /// 删除某个面
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool RemoveFace(Face f)
        {
            return faceList.Remove(f);
        }

        /// <summary>
        /// 得到面组
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaceList()
        {
            return faceList;
        }

        /// <summary>
        /// 检测某个face是存在group中
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool HasFace(Face f)
        {
            return faceList.Contains(f);
        }

        /// <summary>
        /// 对面组中的面进行排序
        /// </summary>
        public void SortFace()
        {
            FaceSort fc = new FaceSort();
            faceList.Sort(fc);
        }


        /// <summary>
        /// 判断两个面是否属于一个组
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        bool IsInSameGroup(Face f1, Face f2, double ErrorMargin = 0.00001)
        {
            double A1, B1, C1, D1;
            double A2, B2, C2, D2;
            f1.UpdateVertices();
            f2.UpdateVertices();
            A1 = f1.Normal.X;
            A2 = f2.Normal.X;

            B1 = f1.Normal.Y;
            B2 = f2.Normal.Y;

            C1 = f1.Normal.Z;
            C2 = f2.Normal.Z;

            D1 = -(f1.Vertices[0].X * A1 + f1.Vertices[0].Y * B1 + f1.Vertices[0].Z * C1);
            D2 = -(f1.Vertices[0].X * A2 + f1.Vertices[0].Y * B2 + f1.Vertices[0].Z * C2);
            if (
                (Math.Abs(A1 * B2 - A2 * B1) < ErrorMargin)  &&
                (Math.Abs(B1 * C2 - B2 * C1) < ErrorMargin)  &&
                (Math.Abs(C1 * D2 - C2 * D1) < ErrorMargin)
               )
            {
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// 判断一个面是否属于这个组
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool IsMatch(Face f, double ErrorMargin = 0.00001)
        {
            f.UpdateVertices();
            double A1, B1, C1, D1;
            A1 = f.Normal.X;
            B1 = f.Normal.Y;
            C1 = f.Normal.Z;
            D1 = -(f.Vertices[0].X * A1 + f.Vertices[0].Y * B1 + f.Vertices[0].Z * C1);

            if (
                (Math.Abs(A1 * b - a * B1) < ErrorMargin)  &&
                (Math.Abs(B1 * c - b * C1) < ErrorMargin)  &&
                (Math.Abs(C1 * d - c * D1) < ErrorMargin)
               )
            {
                return true;
            }
            else
                return false;
        }

        public int GetTopLayer()
        {
            return faceList[0].Layer;
        }

        public int GetBottomLayer()
        {
            return faceList[faceList.Count - 1].Layer;
        }

    }
}

