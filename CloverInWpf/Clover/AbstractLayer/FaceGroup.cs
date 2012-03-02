using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.AbstractLayer
{
    class FaceGroup
    {
        List<Face> GroupList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public void AddFaceBehind(Face f)
        {
            GroupList.Add( f );
        }

        public bool DeleteFace( Face f )
        {
            GroupList.Remove( f );
            return false;
        }


        public List<Face> GetGroup()
        {
            return GroupList;
        }

       

        //bool IsInSameGroup(Face f1, Face f2)
        //{
        //    double A1, B1, C1, D1;
        //    double A2, B2, C2, D2;
        //    A1 = f1.Normal.X;
        //    A2 = f2.Normal.X;

        //    B1 = f1.Normal.Y;
        //    B2 = f2.Normal.Y;

        //    C1 = f1.Normal.Z;
        //    C2 = f2.Normal.Z;

        //    D1 = -( f1.Vertices[ 0 ].X * A1 + f1.Vertices[ 0 ].Y * B1 + f1.Vertices[ 0 ].Z * C1 );
        //    D2 = -( f1.Vertices[ 0 ].X * A2 + f1.Vertices[ 0 ].Y * B2 + f1.Vertices[ 0 ].Z * C2 );

        //    if ( A1 * B2 == A2 * B1 &&
        //        B1 * C2 == B2 * C1 &&
        //        C1 * D2 == C2 * D1
        //        )
        //    {
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        
    }
}
