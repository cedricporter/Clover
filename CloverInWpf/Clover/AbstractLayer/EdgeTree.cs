using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    public class EdgeTree
    {
        Edge root;
        public Clover.Edge Root
        {
            get { return root; }
            set { root = value; }
        }

        public EdgeTree(Edge r)
        {
            root = r;
        }

        //List<Edge> leaves;

        //public List<Edge> Leaves()
        //{
        //    leaves.Clear();
        //    travel(root);
        //    return leaves;
        //}

        public bool IsEmpty
        { get { return root == null; } }

        public bool IsContain(Edge edge)
        {
            bool isthere = false;
            travel(edge, root, ref isthere);

            return isthere;
        }

        public bool IsLeaves(Edge edge)
        {
            return edge.LeftChild == null && edge.RightChild == null;
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


        //void travel(Edge e)
        //{
        //    if (e == null)
        //        return;
        //    if (IsLeaves(e))
        //        leaves.Add(e);
        //    travel(e.LeftChild);
        //    travel(e.RightChild);
        //}
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
