using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.AbstractLayer
{
    class FaceGroup
    {
        List<List<Face>> GroupList = new List<List<Face>>();

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

        bool IsInSameGroup(Face f1, Face f2)
        {
            float A1, B1, C1, D1;
            float A2, B2, C2, D2;
            A1 = f1.Normal.x;
            A2 = f2.Normal.x;

            B1 = f1.Normal.y;
            B2 = f2.Normal.y;

            C1 = f1.Normal.z;
            C2 = f2.Normal.z;

            D1 = -( f1.Vertices[ 0 ].point.x * A1 + f1.Vertices[ 0 ].point.y * B1 + f1.Vertices[ 0 ].point.z * C1 );
            D2 = -( f1.Vertices[ 0 ].point.x * A2 + f1.Vertices[ 0 ].point.y * B2 + f1.Vertices[ 0 ].point.z * C2 );

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
