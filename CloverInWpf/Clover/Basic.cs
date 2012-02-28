using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace Clover
{
    class Vertex
    {
        public Vector3 point;

        public float u = 0;
        public float v = 0;

        public Vertex(float x, float y, float z)
        {
            point.x = x;
            point.y = y;
            point.z = z;
        }
    }

    class Edge
    {
        public Edge parent;
        public Face face1, face2;
        public Vertex vertex1, vertex2;
        public Edge leftChild, rightChild;

        public Edge(Vertex v1, Vertex v2)
        {
            vertex1 = v1;
            vertex2 = v2;
        }
    }

    class Face
    {
        List<Edge> edges;

        public Vector3 normal;
        public Face leftChild = null;
        public Face rightChild = null;
        public Face parent = null;

        public void AddEdge( Edge edge )
        {
            edges.Add( edge );
        }

        public bool RemoveEdge( Edge edge )
        {
            return edges.Remove( edge );
        }

    }
}
