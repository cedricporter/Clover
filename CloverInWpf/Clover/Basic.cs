using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace Clover
{

    class Vertex
    {
        public Vector3 p;

        public float u = 0;
        public float v = 0;

        public Vertex(float x, float y, float z)
        {
            p.x = x;
            p.y = y;
            p.z = z;
        }
    }

    class Edge
    {
        Edge parent;
        public Clover.Edge Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        Face face1, face2;
        public Clover.Face Face2
        {
            get { return face2; }
            set { face2 = value; }
        }
        public Clover.Face Face1
        {
            get { return face1; }
            set { face1 = value; }
        }
        Vertex vertex1, vertex2;
        public Clover.Vertex Vertex2
        {
            get { return vertex2; }
            set { vertex2 = value; }
        }
        public Clover.Vertex Vertex1
        {
            get { return vertex1; }
            set { vertex1 = value; }
        }

        Edge leftChild, rightChild;
        public Clover.Edge RightChild
        {
            get { return rightChild; }
            set { rightChild = value; }
        }
        public Clover.Edge LeftChild
        {
            get { return leftChild; }
            set { leftChild = value; }
        }
        public Edge(Vertex v1, Vertex v2)
        {
            vertex1 = v1;
            vertex2 = v2;
        }
    }

    class Face
    {
        List<Edge> edges;

        Vector3 normal;
        public Mogre.Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        public void AddEdge( Edge edge )
        {
            edges.Add( edge );
        }

        public bool RemoveEdge( Edge edge )
        {
            return edges.Remove( edge );
        }

        Face leftChild = null;
        public Clover.Face LeftChild
        {
            get { return leftChild; }
            set { leftChild = value; }
        }
        Face rightChild = null;
        public Clover.Face RightChild
        {
            get { return rightChild; }
            set { rightChild = value; }
        }
        Face parent = null;
        public Clover.Face Parent
        {
            get { return parent; }
            set { parent = value; }
        }

    }
}
