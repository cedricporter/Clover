using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class EdgeTree
    {
        Edge root;

        public bool AddNode( Edge node )
        {

            return true;
        }


        public bool DeleteNode( Edge node )
        {
            return true;
        }


        public bool IsEmpty()
        {
            if ( root != null )
            {
                return false;
            }
            return true;
        }

        public bool IsContain(Edge edge)
        {

            return true;
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
