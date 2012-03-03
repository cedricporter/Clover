using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class EdgeLayer
    {

        #region get/set
        public int Count
        {
            get { return edgeTreeList.Count; }
        }
        public List<EdgeTree> EdgeTreeList
        {
            get { return edgeTreeList; }
        }
        #endregion

        List<EdgeTree> edgeTreeList = new List<EdgeTree>();
        CloverController controller;

        public EdgeLayer(CloverController controller)
        {
            this.controller = controller;
        }

        public void AddTree(EdgeTree tree)
        {
            edgeTreeList.Add(tree);
        }

        /// <summary>
        /// 一条边分割成两条边
        /// </summary>
        /// <param name="parentEdge"></param>
        /// <param name="cutVertex"></param>
        void UpdateTree(Edge parentEdge, Vertex cutVertex)
        {
            foreach (EdgeTree tree in edgeTreeList)
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
