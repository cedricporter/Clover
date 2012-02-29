using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace Clover
{
    /// <summary>
    /// 抽象的点，里面包含渲染的点和其他信息
    /// </summary>
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

    /// <summary>
    /// 抽象的边
    /// </summary>
    class Edge
    {
        #region get/set
        /// <summary>
        /// 设置父亲时不做任何事情
        /// </summary>
        public Clover.Edge Parent
        {
            get { return parent; }
            set { parent = value; }
        }
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
        /// <summary>
        /// 设置孩子时会将孩子的父亲置为自己
        /// </summary>
        public Clover.Edge LeftChild
        {
            get { return leftChild; }
            set { leftChild = value; leftChild.Parent = this; }
        }
        public Clover.Edge RightChild
        {
            get { return rightChild; }
            set { rightChild = value; rightChild.Parent = this; }
        }
        public bool IsLeaf
        {
            get { return leftChild == null && rightChild == null; }
        }
        #endregion

        Edge parent;
        Face face1, face2;
        Vertex vertex1, vertex2;
        Edge leftChild, rightChild;

        public Edge(Vertex v1, Vertex v2)
        {
            vertex1 = v1;
            vertex2 = v2;
        }
    }

    /// <summary>
    /// 抽象的面
    /// </summary>
    class Face
    {
        List<Edge> edges = new List<Edge>();

        Vector3 normal;
        Face leftChild = null;
        Face rightChild = null;
        Face parent = null;

        #region get/set
        public Mogre.Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }
        public Clover.Face LeftChild
        {
            get { return leftChild; }
            set { leftChild = value; }
        }
        public Clover.Face RightChild
        {
            get { return rightChild; }
            set { rightChild = value; }
        }
        public Clover.Face Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        #endregion

        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        public bool RemoveEdge(Edge edge)
        {
            return edges.Remove(edge);
        }

    }
}
