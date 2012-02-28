using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class VertexLayer
    {
        // Basic vertex structure.
        struct Vertex
        {
            float x;
            float y;
            float z;
        };

        // Attributes.
        // Vertexcell lookup table.
        List<List<Vertex>> VertexCellTable;

        // Constructor and Destroyer.
        public void VertexLayer()
        {
            VertexCellTable = new List<List<Vertex>>();
        }

        // Public Interfaces.
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

        public bool GetVertex(Vertex vertex, int index)
        {
            if (!IsVertexExist(index))
                return false;
            vertex = VertexCellTable[index][0];
            return true;
        }

        public int InsertVertex(Vertex vertex)
        {
            List<Vertex> vl =  new List<Vertex>();
            vl.Add(vertex);
            VertexCellTable.Add(vl);
            return VertexCellTable.Count;
        }

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
            return ;
        }
    }
}
