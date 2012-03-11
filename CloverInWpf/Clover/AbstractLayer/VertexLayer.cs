using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    /// <summary>
    /// Basic vertex structure.
    /// </summary>
    [Serializable]
    public class VertexLayer
    {
        #region Attributes

        #region get/set
        public List<List<Vertex>> VertexCellTable
        {
            get { return vertexCellTable; }
        }
        #endregion

        // Vertexcell lookup table.
        List<List<Vertex>> vertexCellTable = new List<List<Vertex>>();

        #endregion

        public List<Vertex> Vertices
        {
            get
            {
                List<Vertex> list = new List<Vertex>();
                foreach (List<Vertex> l in vertexCellTable)
                {
                    if (l.Count < 1)
                        continue;
                    list.Add(l[l.Count - 1]);
                }
                return list;
            }
        }

        #region Public Interfaces.

        // Constructor and Destroyer.
        public VertexLayer()
        {
        }

        public bool IsVertexExist(int index)
        {
            // 这里逻辑也有问题吧？ —— Cedric Porter
            //if (VertexCellTable.Count < index)
            //    return false;
            //return true;  
            return index < vertexCellTable.Count;
        }

        public bool IsEmpty()
        {
            return vertexCellTable.Count == 0;
        }

        public Vertex GetVertex(int index)
        {
            if (!IsVertexExist(index))
                return null;
            return vertexCellTable[index][vertexCellTable[index].Count - 1];
        }

        /// <summary>
        /// 查找顶点是否在顶点列表中，若存在返回该顶点的索引，若不存在返回-1
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public int IsVertexExist(Vertex vertex)
        {
            int index = 0;
            foreach (List<Vertex> vl in vertexCellTable)
            {
                if (vl[vl.Count - 1].GetPoint3D() == vertex.GetPoint3D())
                    return index;
                index++;
            }
            return -1;
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
            vertexCellTable.Add(vl);
            // 是不是返回新的顶点的索引？是的话是不是应该减一呢？ —— Cedric Porter
            // old: return VertexCellTable.Count;

            // 设置顶点的index，后续插入的顶点都有同样的index，方便索引
            vertex.Index = vertexCellTable.Count - 1;

            return vertex.Index;
        }

        /// <summary>
        /// 更新顶点
        /// </summary>
        /// <param name="vertex">插入在下标为index的顶点</param>
        /// <param name="index">这个顶点的索引</param>
        /// <remarks>下标越界会抛出异常</remarks>
        public void UpdateVertex(Vertex vertex, int index)
        {
            vertex.Index = index;
            vertexCellTable[index].Add(vertex);
        }
        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="oldVertex">要被更新的节点</param>
        /// <param name="newVertex">新的节点</param>
        public void UpdateVertex(Vertex oldVertex, Vertex newVertex)
        {
            newVertex.Index = oldVertex.Index;
            vertexCellTable[newVertex.Index].Add(newVertex);
        }

        public void DeleteVertex(int index)
        {
            vertexCellTable.RemoveAt(index);
        }

        /// <summary>
        /// 将当前节点之后的版本删掉
        /// </summary>
        /// <param name="vertex"></param>
        public void DeleteNextVersionToEnd(Vertex vertex)
        {
            int index = VertexCellTable[vertex.Index].IndexOf(vertex) + 1;
            if (index != -1 && index < VertexCellTable[vertex.Index].Count)
            {
                VertexCellTable[vertex.Index].RemoveRange(index, VertexCellTable[vertex.Index].Count - index);
            }
        }

        /// <summary>
        /// 把当前节点和之后的节点都删除
        /// </summary>
        /// <param name="vertex"></param>
        public void DeleteThisVersionToEnd(Vertex vertex)
        {
            int index = VertexCellTable[vertex.Index].IndexOf(vertex);
            if (index == -1)
                return;
            VertexCellTable[vertex.Index].RemoveRange(index, VertexCellTable[vertex.Index].Count - index);
        }

        /// <summary>
        /// 删除最新版的节点
        /// </summary>
        /// <param name="index"></param>
        public void DeleteLastVersion(int index)
        {
            if (index < vertexCellTable.Count && vertexCellTable[index].Count > 0)
                vertexCellTable[index].RemoveAt(vertexCellTable[index].Count - 1);
        }

        public void ClearTable()
        {
            vertexCellTable.Clear();
        }
        #endregion
    }
}
