﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Clover
{

    /// <summary>
    /// 抽象的点，里面包含渲染的点和其他信息
    /// </summary>
    public class Vertex 
    {
        Point3D point = new Point3D();

        public Point UVW = new Point();     /// 纹理坐标
        public float u = 0;
        public float v = 0;

        public int Index = -1;      /// 在VertexLayer里面的索引，所有的孩子都有相同的index

        #region get/set
        public double X
        {
            get { return point.X; }
            set { point.X = value; }
        }
        public double Y
        {
            get { return point.Y; }
            set { point.Y = value; }
        }
        public double Z
        {
            get { return point.Z; }
            set { point.Z = value; }
        }
        #endregion

        public Point3D GetPoint3D()
        {
            return point;
        }

        public Vertex(double x = 0, double y = 0, double z = 0, int index = -1)
        {
            point.X = x;
            point.Y = y;
            point.Z = z;

            Index = index;
        }
    }

    /// <summary>
    /// 抽象的边
    /// </summary>
    public class Edge
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

        public bool IsVerticeIn(Point3D p)
        {
            if ( p.Equals( Vertex1.GetPoint3D() ) || p.Equals( Vertex2.GetPoint3D() ) )
                return true;
            return false;
        }
    }

    /// <summary>
    /// 抽象的面
    /// </summary>
    public class Face
    {
        #region 成员变量
        List<Edge> edges = new List<Edge>();
        Point3D normal;
        Face leftChild = null;
        Face rightChild = null;
        Face parent = null;
        List<Vertex> vertices = new List<Vertex>();
        #endregion

        #region get/set
        public List<Edge> Edges
        {
            get { return edges; }
            set { edges = value; }
        }
        #endregion

        #region get/set
        public List<Vertex> Vertices
        {
            get { return vertices; }
        }
        public Point3D Normal
        {
            get { UpdateNormal(); return normal; }
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

        #region 更新
        /// <summary>
        /// 更新面的点，方便绘制时使用
        /// </summary>
        public void UpdateVertices()
        {
            // 可能在这里要对边进行排序，才可以得到有序的点，否则就要保证添加边的时候按顺序。
            vertices.Clear();
            foreach (Edge e in edges)
            {
                if (!vertices.Contains(e.Vertex1))
                    vertices.Add(e.Vertex1);
                if (!vertices.Contains(e.Vertex2))
                    vertices.Add(e.Vertex2);
            }
        }

        /// <summary>
        /// 更行面的法向量
        /// </summary>
        /// <returns></returns>
        bool UpdateNormal()
        {
            if (vertices.Count < 3)
                return false;
            Vertex[] p = new Vertex[3];
            p[0] = vertices[0];
            p[1] = Vertices[1];
            p[2] = vertices[2];
            // 取任意位于面上的向量

            // 被注释了
            //Point3D v1 = p[0].point - p[1].point;
            //Point3D v2 = p[0].point - p[2].point;
            //normal = v1.CrossProduct( v2 );

            return true;
        }
        #endregion

        #region 对边的操作
        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
            // UpdateVertices();
        }

        public void SortEdge()
        {
            // 对边进行排序
            Edge currentedge = edges[0];
            edges.RemoveAt(0);
            int edgecount = edges.Count;
            List<Edge> orderelist = new List<Edge>();
            orderelist.Add(currentedge);
            for (int i = 0; i < edgecount - 1; i++)
            {
                foreach (Edge e in edges)
                {
                    if (currentedge.IsVerticeIn(e.Vertex1.GetPoint3D()) || currentedge.IsVerticeIn(e.Vertex2.GetPoint3D()))
                    {
                        orderelist.Add(e);
                        edges.Remove(e);
                        break;
                    }
                }
            }
            edges = orderelist;
            UpdateVertices();
        }

        public bool RemoveEdge(Edge edge)
        {
            bool ret = edges.Remove(edge);
            UpdateVertices();
            return ret;
        }
        #endregion

    }

    /// <summary>
    /// 边到2D的映射
    /// </summary>
    public class Edge2D
    {
        public Edge2D(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
        public Point p1, p2;
    }
}
