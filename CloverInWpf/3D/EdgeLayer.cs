using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class EdgeLayer
    {
        List<EdgeTree> EdgeTreeList = new List<EdgeTree>();

        CloverController controller;

        public EdgeLayer(CloverController controller)
        {
            this.controller = controller;
        }

        public void AddTree(EdgeTree tree)
        {
            EdgeTreeList.Add(tree);
        }

        /// <summary>
        /// 一条边分割成两条边
        /// </summary>
        /// <param name="parentEdge"></param>
        /// <param name="cutVertex"></param>
        void UpdateTree(Edge parentEdge, Vertex cutVertex)
        {
            foreach (EdgeTree tree in EdgeTreeList)
            {
                if (tree.IsContain(parentEdge))
                {
                    Edge e1 = new Edge(parentEdge.Vertex1, cutVertex);
                    Edge e2 = new Edge(cutVertex, parentEdge.Vertex2);

                    // 设置左右孩子会自动设置父亲
                    //e1.Parent = e2.Parent = parentEdge;

                    parentEdge.LeftChild = e1;
                    parentEdge.RightChild = e2;

                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            return true;
        }

    }
}
