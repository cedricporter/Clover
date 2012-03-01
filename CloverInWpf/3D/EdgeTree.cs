using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class EdgeTree
    {
        Edge root;

        public EdgeTree(Edge r)
        {
            root = r;
        }

        // 没必要，因为分割操作在EdgeLayer，直接对树中的节点分割，所以这里应该只是一些基本功能
        //public bool AddEdge( Edge node )
        //{
        //    return true;
        //}
        //public bool DeleteEdge( Edge node )
        //{
        //    return true;
        //}

        public bool IsEmpty
        { get { return root == null; } }

        public bool IsContain(Edge edge)
        {
            bool isthere = false;
            travel(edge, root, ref isthere);

            return isthere;
        }

        void travel(Edge target, Edge r, ref bool isExist)
        {
            if (r == null || isExist)
                return;

            if (target == r)
                isExist = true;

            travel(target, r.LeftChild, ref isExist);
            travel(target, r.RightChild, ref isExist);
        }

        /// <summary>
        /// 通过一些条件索引所有符合条件的边界点
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetNode()
        {
            return null;
        }


    }
}
