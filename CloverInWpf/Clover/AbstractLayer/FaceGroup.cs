using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover;
using System.Windows.Media;

namespace Clover.AbstractLayer
{
    class FaceGroup
    {
        List<Face> GroupList;
        Vector3D normal;
        double A, B, C, D;

        public FaceGroup(Face f)
        {
            if (f != null)
            {
                GroupList.Add( f );
                A = f.Normal.X;
                B = f.Normal.Y;
                C = f.Normal.Z;
                D = -( f.Vertices[ 0 ].X * A + f.Vertices[ 0 ].Y * B + f.Vertices[ 0 ].Z * C );
            }
            
            
        }
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



        bool IsInSameGroup( Face f1, Face f2 )
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
            if (
                (Math.Abs(A1 * B2 - A2 * B1) < 0.0001)  &&
                (Math.Abs(B1 * C2 - B2 * C1) < 0.0001)  &&
                (Math.Abs(C1 * D2 - C2 * D1) < 0.0001)  
               )
            {
                return true;
            }
            else
                return false;
        }


        bool IsMatch( Face f )
        {
            double A1, B1, C1, D1;
            A1 = f.Normal.X;
            B1 = f.Normal.Y;
            C1 = f.Normal.Z;
            D1 = -( f.Vertices[ 0 ].X * A1 + f.Vertices[ 0 ].Y * B1 + f.Vertices[ 0 ].Z * C1 );

            if (
                ( Math.Abs( A1 * B - A * B1 ) < 0.0001 )  &&
                ( Math.Abs( B1 * C - B * C1 ) < 0.0001 )  &&
                ( Math.Abs( C1 * D - C * D1 ) < 0.0001 )
               )
            {
                return true;
            }
            else
                return false;
        }


        
    }
}
