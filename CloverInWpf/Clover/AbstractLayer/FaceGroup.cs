using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.AbstractLayer
{
    class FaceGroup
    {
        List<List<Face>> GroupList = new List<List<Face>>();
        

        /// <summary>
        /// 初始化面组
        /// </summary>
        /// <param name="facetree"></param>
        public void Initialize( FacecellTree facetree )
        {
            foreach ( Face f in facetree.Leaves )
            {
                AddFace(f);
            }
            
        }


        /// <summary>
        /// 向面组添加一个面
        /// </summary>
        /// <param name="f"></param>
        void AddFace(Face f)
        {
            if (GroupList.Count == 0)
            {
                List<Face> l = new List<Face>();
                l.Add( f );
                GroupList.Add( l );
            }
            else
            {
                bool isfind = false;
                foreach (List<Face> l in GroupList)
                {
                    foreach (Face face in l)
                    {
                        if (IsInSameGroup(f, face))
                        {
                            l.Add( f );
                            isfind = true;
                            break;
                        }

                    }
                    if (isfind)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 从面组中删除面
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        bool DeleteFace(Face f)
        {
            foreach (List<Face> l in GroupList)
            {
                if ( l.Remove( f ) )
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前面所在的整个组
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        List<Face> GetGroup(Face f)
        {
            foreach (List<Face> l in GroupList)
            {
                foreach (Face face in l)
                {
                    if (f == face)
                    {
                        return l;
                    }
                }
            }
            return null;
        }

        bool IsInSameGroup(Face f1, Face f2)
        {
            double A1, B1, C1, D1;
            double A2, B2, C2, D2;
            A1 = f1.Normal.X;
            A2 = f2.Normal.X;

            B1 = f1.Normal.Y;
            B2 = f2.Normal.Y;

            C1 = f1.Normal.Z;
            C2 = f2.Normal.Z;

            D1 = -( f1.Vertices[ 0 ].X * A1 + f1.Vertices[ 0 ].Y * B1 + f1.Vertices[ 0 ].Z * C1 );
            D2 = -( f1.Vertices[ 0 ].X * A2 + f1.Vertices[ 0 ].Y * B2 + f1.Vertices[ 0 ].Z * C2 );

            if ( A1 * B2 == A2 * B1 &&
                B1 * C2 == B2 * C1 &&
                C1 * D2 == C2 * D1
                )
            {
                return true;
            }
            else
                return false;
            
            
        }

        
    }
}
