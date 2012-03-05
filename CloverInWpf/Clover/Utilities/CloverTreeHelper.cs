using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;

namespace Clover
{
    /// <summary>
    /// 提供一些使用的查询折纸结构的工具函数
    /// </summary>
    class CloverTreeHelper
    {

        /// <summary>
        /// 判断一个面是否在一个组中
        /// </summary>
        /// <param name="face"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static Boolean IsFaceInGroup(Face face, FaceGroup group)
        {
            // 等待杨旭瑜写完group，。。。
            // todo
            return false;
        }

        /// <summary>
        /// 判断一个点是否在一个面中
        /// </summary>
        /// <param name="vervex"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public static Boolean IsVertexInFace(Vertex vervex, Face face)
        {
            foreach (Vertex v in face.Vertices)
            {
                if (v == vervex)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 返回所有拥有该点的叶子面
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static List<Face> GetReferenceFaces(Vertex v)
        {
            List<Face> facelist = new List<Face>();
            foreach (Face face in CloverController.GetInstance().FaceLeaves)
            {
                if (IsVertexInFace(v, face))
                    facelist.Add(face);
            }
            return facelist;
        }

        /// <summary>
        /// 返回两面的共边，如果没有则返回null
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns></returns>
        public static Edge GetSharedEdge(Face face1, Face face2)
        {
            foreach (Edge edge1 in face1.Edges)
            {
                foreach (Edge edge2 in face2.Edges)
                {
                    if (edge1 == edge2)
                        return edge1;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 返回一个组的基面，用以计算其他面的渲染偏差量
        /// </summary>
        /// <param name="group"></param>
        public static Face FindBaseFace(FaceGroup group)
        {
            //todo 

            return null;
        }


    }
}
