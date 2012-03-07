using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	磁铁类，负责判断各种贴合的情况
**/

namespace Clover.AbstractLayer
{
    public class Magnet
    {
        static Boolean isMagnetismEnable = true;

        static Double vertexMagnetismVal = 5;
        static Double edgeMagnetismVal = 5;

        /// <summary>
        /// 在给定的Face中寻找可吸附的边或点
        /// </summary>
        /// <param name="vertex"></param>
        public static Boolean PerformVertexAttach(Point3D point, Face face, out Point3D outPoint)
        {
            if (!isMagnetismEnable)
            {
                outPoint = new Point3D();
                return false;
            }
            // 先判断吸附点
            foreach (Vertex v in face.Vertices)
            {
                if (CloverMath.IsTwoPointsEqual(point, v.GetPoint3D(), vertexMagnetismVal))
                {
                    outPoint = v.GetPoint3D();
                    return true;
                }
            }
            // 再判断吸附边缘
            foreach (Edge e in face.Edges)
            {
                if (CloverMath.IsPointInTwoPoints(point, e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), edgeMagnetismVal))
                {
                    outPoint = CloverMath.GetNearestPointOnSegment(point, e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D());
                    return true;
                }
            }

            outPoint = new Point3D();
            return false;
        }


    }
}
