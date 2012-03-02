using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Clover.AbstractLayer
{
    class FaceGroup
    {
        List<Face> GroupList = new List<Face>();
        

        public void Initialize( FacecellTree facetree )
        {
            foreach ( Face f in facetree.Leaves )
            {
                AddFace(f);
            }
            
        }


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

        bool DeleteFace(Face f)
        {
            foreach (List<Face> l in GroupList)
            {
                if ( l.Remove( f ) )
                    return true;
            }
            return false;
        }


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

            D1 = -( f1.Vertices[ 0 ].point.X * A1 + f1.Vertices[ 0 ].point.Y * B1 + f1.Vertices[ 0 ].point.Z * C1 );
            D2 = -( f1.Vertices[ 0 ].point.X * A2 + f1.Vertices[ 0 ].point.Y * B2 + f1.Vertices[ 0 ].point.Z * C2 );

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
