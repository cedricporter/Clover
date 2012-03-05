using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    public class CloverTreeHelper
    {
        /// <summary>
        /// 通过点来找面
        /// </summary>
        /// <param name="faceList">传入一个面表</param>
        /// <param name="vertex"></param>
        /// <returns>返回找到的面</returns>
        public static List<Face> FindFacesFromVertex(List<Face> faceList, Vertex vertex)
        {
            List<Face> list = new List<Face>();

            foreach (Face face in faceList)
            {
                foreach (Edge edge in face.Edges)
                {
                    //if ( CloverMath.IsTwoPointsEqual(edge.Vertex1.GetPoint3D(), vertex.GetPoint3D(), 0.001)
                    //    || CloverMath.IsTwoPointsEqual(edge.Vertex2.GetPoint3D(), vertex.GetPoint3D(), 0.001))
                    if (edge.IsVerticeIn(vertex))
                    {
                        list.Add(face);
                        break;
                    }
                }
            }

            return list;
        }

    }
}
