using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    /// <summary>
    /// Basic vertex structure.
    /// </summary>
    class VertexLayer
    {
        //////////////////////////////////////////////////////////////////////////
        // Attributes.
        //////////////////////////////////////////////////////////////////////////

        // Vertexcell lookup table.
        List<List<Vertex>> VertexCellTable;

        CloverController controller;

        // Constructor and Destroyer.
        public VertexLayer(CloverController ctrl)
        {
            controller = ctrl;
            VertexCellTable = new List<List<Vertex>>();
        }

        //////////////////////////////////////////////////////////////////////////
        // Public Interfaces.
        //////////////////////////////////////////////////////////////////////////

        public bool IsVertexExist(int index) 
        {
            if (VertexCellTable.Count < index)
                return false;
            return true;  
        }
        
        public bool IsEmpty() 
        { 
            if ( VertexCellTable.Count == 0 )
                return true;
            return false;
        }

        public Vertex GetVertex(int index)
        {
            if (!IsVertexExist(index))
                return null;
            return VertexCellTable[index][0];
        }

        /// <summary>
        /// 插入一个全新的顶点，会建立一个新的顶点链表
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public int InsertVertex(Vertex vertex)
        {
            List<Vertex> vl =  new List<Vertex>();
            vl.Add(vertex);
            VertexCellTable.Add(vl);
            // 是不是返回新的顶点的索引？是的话是不是应该减一呢？ —— Cedric Porter
            // old: return VertexCellTable.Count;
            return VertexCellTable.Count - 1;
        }

        /// <summary>
        /// 更新顶点
        /// </summary>
        /// <param name="vertex">插入在下标为index的顶点</param>
        /// <param name="index">这个顶点的索引</param>
        /// <remarks>下标越界会抛出异常</remarks>
        public void UpdateVertex(Vertex vertex, int index)
        {
            VertexCellTable[index].Add(vertex);
            return ;
        }

        public void DeleteVertex(int index)
        {
            VertexCellTable.RemoveAt(index);
        }

        public void ClearTable()
        {
            VertexCellTable.Clear();
        }
    }
}
