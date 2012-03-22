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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="IsFacingUser"></param>
        /// <returns></returns>
        public bool UpdateGroupAfterFoldUp( FaceGroup participatedGroup ,FaceGroup movedFaceGroup, FaceGroup fixedFaceGroup, bool IsFacingUser = true )
        {

            // 发现不是foldup操作，直接返回
            if ( !CloverMath.IsTwoVectorTheSameDir( movedFaceGroup.Normal, fixedFaceGroup.Normal ) )
            {
                return false;
            }

            fixedFaceGroup.SortFace();
            movedFaceGroup.SortFace();
            fixedFaceGroup.Normal = participatedGroup.Normal;

            // 判断组是不是面向用户

            if ( IsFacingUser )
            {
                int layer = 0;
                int hardlayer = 0;
                int lastlayer = fixedFaceGroup.GetFaceList()[ 0 ].Layer;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    if ( fixedFaceGroup.GetFaceList()[ i ].Layer == lastlayer )
                    {
                        lastlayer = fixedFaceGroup.GetFaceList()[ i ].Layer;
                        fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    else
                    {
                        layer++;
                        lastlayer = fixedFaceGroup.GetFaceList()[ i ].Layer;
                        fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    hardlayer++;

                }
                layer = hardlayer;
                // 根据是否覆盖来调整layer的值
                for ( int i = fixedFaceGroup.GetFaceList().Count - 1; i >= 0; i-- )
                {

                    if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( movedFaceGroup.GetFaceList()[ movedFaceGroup.GetFaceList().Count - 1 ], fixedFaceGroup.GetFaceList()[ i ] ) )
                    {
                        layer--;
                    }
                    else if ( i == fixedFaceGroup.GetFaceList().Count - 1 )
                    {
                        break;
                    }
                }

                lastlayer = movedFaceGroup.GetFaceList()[ 0 ].Layer;
                for ( int i = movedFaceGroup.GetFaceList().Count - 1; i >= 0; i-- )
                {

                    if ( movedFaceGroup.GetFaceList()[ i ].Layer == lastlayer )
                    {
                        lastlayer = movedFaceGroup.GetFaceList()[ i ].Layer;
                        movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    else
                    {
                        layer++;
                        lastlayer = movedFaceGroup.GetFaceList()[ i ].Layer;
                        movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    fixedFaceGroup.AddFace( movedFaceGroup.GetFaceList()[ i ] );
                }
            }
            else
            {

                int layer = 0;
                int lastlayer = fixedFaceGroup.GetFaceList()[ 0 ].Layer;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    if ( fixedFaceGroup.GetFaceList()[ i ].Layer == lastlayer )
                    {
                        lastlayer = fixedFaceGroup.GetFaceList()[ i ].Layer;
                        fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    else
                    {
                        layer++;
                        lastlayer = fixedFaceGroup.GetFaceList()[ i ].Layer;
                        fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }

                }
                layer = fixedFaceGroup.GetBottomLayer();
                layer--;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( movedFaceGroup.GetFaceList()[ 0 ], fixedFaceGroup.GetFaceList()[ i ] ) )
                    {
                        layer++;
                    }
                    else if ( i == 0 )
                    {
                        break;
                    }
                }

                lastlayer = movedFaceGroup.GetFaceList()[ 0 ].Layer;
                for ( int i = 0; i < movedFaceGroup.GetFaceList().Count; i++ )
                {
                    if ( movedFaceGroup.GetFaceList()[ i ].Layer == lastlayer )
                    {
                        lastlayer = movedFaceGroup.GetFaceList()[ i ].Layer;
                        movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }
                    else
                    {
                        layer--;
                        lastlayer = movedFaceGroup.GetFaceList()[ i ].Layer;
                        movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    }

                    fixedFaceGroup.AddFace( movedFaceGroup.GetFaceList()[ i ] );
                    
                }
            }

            faceList.Clear();
            foreach(Face f in fixedFaceGroup.GetFaceList())
            {
                faceList.Add( f );
            }
            return true;
        }


    }


        
}

