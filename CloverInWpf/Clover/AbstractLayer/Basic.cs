using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Diagnostics;

namespace Clover
{
    public delegate void Update(object sender, EventArgs e);

    public enum FoldingOp
    {
        Blend,
        FoldUp,
        TuckIn
    }

    public enum BendTpye
    {
        BlendZero,
        BlendSemiCycle,
        BlendNormally,
        

    }

    /// <summary>
    /// 抽象的点，里面包含渲染的点和其他信息
    /// </summary>
    [Serializable]
    public class Vertex : ICloneable
    {
        #region 成员变量
        Point3D point = new Point3D();          /// 逻辑层的坐标
        Point3D renderPoint = new Point3D();    /// 渲染层的坐标
        /// 
        public int Index = -1;      /// 在VertexLayer里面的索引，所有的孩子都有相同的index

        double _u = 0;
        double _v = 0;

        bool moved = false;

        public int Version = 1;

        [NonSerialized]
        public Update Update;

        static int vertex_count = 0;
        public static int Vertex_count
        {
            get { return vertex_count; }
            set { vertex_count = value; }
        }
        int id;
        #endregion

        #region get/set
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public Point3D GetPoint3D()
        {
            return point;
        }

        public void SetPoint3D(Point3D vertex)
        {
            point.X = vertex.X;
            point.Y = vertex.Y;
            point.Z = vertex.Z;

            if (Update != null)
                Update(this, null);
        }
        public System.Windows.Media.Media3D.Point3D RenderPoint
        {
            get { return renderPoint; }
            set { renderPoint = value; }
        }
        public double u
        {
            get { return _u; }
            set { _u = value; if (Update != null) Update(this, null); }
        }
        public double v
        {
            get { return _v; }
            set { _v = value; if (Update != null) Update(this, null); }
        }
        public bool Moved
        {
            get { return moved; }
            set { moved = value; }
        }
        public double X
        {
            get { return point.X; }
            set { point.X = value; if (Update != null) Update(this, null);  }
        }
        public double Y
        {
            get { return point.Y; }
            set { point.Y = value; if (Update != null) Update(this, null);  }
        }
        public double Z
        {
            get { return point.Z; }
            set { point.Z = value; if (Update != null) Update(this, null);}
        }
        #endregion

        #region 构造函数
        public object Clone()
        {
            Vertex v = this.MemberwiseClone() as Vertex;
            v.id = vertex_count++;

            v.point = new Point3D(point.X, point.Y, point.Z);
            v.renderPoint = new Point3D(renderPoint.X, renderPoint.Y, renderPoint.Z);

            v.Version = this.Version + 1;

            return v;
        }

        public Vertex(Point3D vertex)
        {
            point = vertex;

            id = vertex_count++;
        }

        public Vertex(Vertex vertex)
        {
            point = new Point3D(vertex.X, vertex.Y, vertex.Z);
            renderPoint = new Point3D(vertex.renderPoint.X, vertex.renderPoint.Y, vertex.renderPoint.Z);

            Index = vertex.Index;
            _u = vertex._u;
            _v = vertex._v;

            id = vertex_count++;
        }

        public Vertex(double x = 0, double y = 0, double z = 0, int index = -1)
        {
            point.X = x;
            point.Y = y;
            point.Z = z;

            Index = index;

            id = vertex_count++;
        }
        #endregion

        #region 重载==运算符
        //public override bool Equals(System.Object v)
        //{
        //    // If parameter is null return false:
        //    if ((object)v == null)
        //    {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return ((Vertex)v).GetPoint3D() == this.GetPoint3D();
        //}

        //public override int GetHashCode()
        //{
        //    return Index ^ _v.GetHashCode() ^ _v.GetHashCode() ^ moved.GetHashCode() ^ Version ^ point.GetHashCode() ^ Update.GetHashCode();
        //}

        //public static bool operator==(Vertex lhs, Vertex rhs)
        //{
        //    // If both are null, or both are same instance, return true.
        //    if (System.Object.ReferenceEquals(lhs, rhs))
        //        return true;

        //    // If one is null, but not both, return false.
        //    if (((object)lhs == null) || ((object)rhs == null))
        //    {
        //        return false;
        //    }

        //    // Return true if the fields match:
        //    return lhs.GetPoint3D() == rhs.GetPoint3D();
        //}

        //public static bool operator!=(Vertex lhs, Vertex rhs)
        //{
        //    return !(lhs == rhs);
        //}
        #endregion
    }

    /// <summary>
    /// 抽象的边
    /// </summary>
    public class Edge
    {
        #region get/set
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// 设置父亲时不做任何事情
        /// </summary>
        public Clover.Edge Parent
        {
            get { return parent; }
            set 
            {
                if (value == null && parent != null)
                {
                    if (parent.leftChild == null) parent.leftChild = null;
                    if (parent.rightChild == null) parent.rightChild = null;
                }
                parent = value;
            }
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
            set { leftChild = value; if (leftChild != null) leftChild.Parent = this; }
        }
        public Clover.Edge RightChild
        {
            get { return rightChild; }
            set { rightChild = value; if (rightChild != null) rightChild.Parent = this; }
        }
        public bool IsLeaf
        {
            get { return leftChild == null && rightChild == null; }
        }
        #endregion

        #region 成员变量
        Edge parent;
        Face face1, face2;
        Vertex vertex1, vertex2;
        Edge leftChild, rightChild;
        int id = -1;
        static int edge_count = 0;
        #endregion

        public Edge(Vertex v1, Vertex v2)
        {
            vertex1 = v1;
            vertex2 = v2;
            id = edge_count++;
        }

        #region 辅助函数
        public bool IsVerticeIn(Vertex v)
        {
            return IsVerticeIn(v.GetPoint3D());
        }
        public bool IsVerticeIn(Point3D p)
        {
            return CloverMath.IsPointInTwoPoints(p, vertex1.GetPoint3D(), vertex2.GetPoint3D(), 0.001);
        }
        #endregion

    }

    /// <summary>
    /// 抽象的面
    /// </summary>
    public class Face : ICloneable
    {
        ///// <summary>
        ///// 将一个面反过来
        ///// </summary>
        //public void Flip()
        //{
        //    Vertex temp = StartVertex1;
        //    StartVertex1 = startVertex2;
        //    startVertex2 = temp;
        //    UpdateVertices();
        //}

        #region 构造
        public Face(int layer)
        {
            this.layer = layer;
            id = face_count++;
        }

        /// <summary>
        /// 返回一个新的面，但是他的边和点都不在折叠树里面。
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Face newFace = MemberwiseClone() as Face;

            newFace.normal = new Vector3D(normal.X, normal.Y, normal.Z);

            newFace.vertices = new List<Vertex>();
            newFace.edges = new List<Edge>();

            Dictionary<Vertex, Vertex> vertexDict = new Dictionary<Vertex, Vertex>();
            foreach (Edge e in edges)
            {

                Vertex n1, n2;
                if (vertexDict.ContainsKey(e.Vertex1))
                {
                    n1 = vertexDict[e.Vertex1];
                }
                else
                {
                    n1 = e.Vertex1.Clone() as Vertex;
                    vertexDict[e.Vertex1] = n1;
                }

                if (vertexDict.ContainsKey(e.Vertex2))
                {
                    n2 = vertexDict[e.Vertex2];
                }
                else
                {
                    n2 = e.Vertex2.Clone() as Vertex;
                    vertexDict[e.Vertex2] = n2;
                }

                newFace.edges.Add(new Edge(n1, n2));
                if (e.Vertex1 == startVertex1)
                    newFace.startVertex1 = n1;
                if (e.Vertex2 == startVertex2)
                    newFace.startVertex2 = n2;
            }

            newFace.UpdateVertices();

            return newFace;
        }
        #endregion

        #region 成员变量
        List<Vertex> vertices = new List<Vertex>();
        List<Edge> edges = new List<Edge>();
        Vector3D normal;
        Face leftChild = null;
        Face rightChild = null;
        Face parent = null;
        int layer = 0; // 一个组中平面的顺序，越大表示面处于组中的较上方
        static int face_count = 0;
        int id = -1;
        /// <summary>
        /// 两者决定这个面的法向量
        /// </summary>
        Vertex startVertex1;
        Vertex startVertex2;
        
        #endregion

        #region get/set
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public Clover.Vertex StartVertex1
        {
            get { return startVertex1; }
            set { startVertex1 = value; }
        }
        public Clover.Vertex StartVertex2
        {
            get { return startVertex2; }
            set { startVertex2 = value; }
        }
        public List<Edge> Edges
        {
            get { return edges; }
            set { edges = value; }
        }
        public int Layer
        {
            get { return layer; }
            set { layer = value; }
        }
        public List<Vertex> Vertices
        {
            get { return vertices; }
        }
        public Vector3D Normal
        {
            get { UpdateNormal(); Debug.WriteLine(normal.ToString()); return normal; }
            set { normal = value; }
        }
        public Clover.Face LeftChild
        {
            get { return leftChild; }
            set { leftChild = value; if ( leftChild != null ) leftChild.parent = this; }
        }
        public Clover.Face RightChild
        {
            get { return rightChild; }
            set { rightChild = value; if ( rightChild != null ) rightChild.parent = this; }
        }
        public Clover.Face Parent
        {
            get { return parent; }
            set
            {
                if ( value == null )
                {
                    if ( parent.leftChild == this ) parent.leftChild = null;
                    if ( parent.rightChild == this ) parent.rightChild = null;
                }
                parent = value;
            }
        }
        #endregion

        #region 更新


        /// <summary>
        /// 更新面的点，方便绘制时使用
        /// </summary>
        public void UpdateVertices()
        {
            // testing
            CloverController ctrl = CloverController.GetInstance();

            // 可能在这里要对边进行排序，才可以得到有序的点，否则就要保证添加边的时候按顺序。
            vertices.Clear();

            vertices.Add(startVertex1);
            vertices.Add(startVertex2);

            //vertices.Add(edges[0].Vertex1);
            //vertices.Add(edges[0].Vertex2);

            Vertex currentVertex = startVertex2;
            //Vertex currentVertex = edges[0].Vertex2;

            List<Edge> ignoreList = new List<Edge>();
            //ignoreList.Add(edges[0]);
            foreach (Edge edge in edges)
            {
                if (edge.IsVerticeIn(startVertex1) && edge.IsVerticeIn(startVertex2))
                {
                    ignoreList.Add(edge);
                    break;
                }
            }

            int counter = vertices.Count + 1;

            while (ignoreList.Count < edges.Count - 1)
            {
                foreach (Edge e in edges)
                {
                    if (!ignoreList.Contains(e))
                    {
                        //if (CloverMath.IsTwoPointsEqual(e.Vertex1.GetPoint3D(), currentVertex.GetPoint3D()))
                        if (e.Vertex1 == currentVertex)
                        {
                            ignoreList.Add(e);
                            if (!vertices.Contains(e.Vertex2))
                                vertices.Add(currentVertex = e.Vertex2);
                        }
                        //else if (CloverMath.IsTwoPointsEqual(e.Vertex2.GetPoint3D(), currentVertex.GetPoint3D()))
                        else if (e.Vertex2 == currentVertex)
                        {
                            ignoreList.Add(e);
                            if (!vertices.Contains(e.Vertex1))
                                vertices.Add(currentVertex = e.Vertex1);
                        }
                    }
                }

                if (counter-- == 0)
                    return;
            }

            Edge e2 = null;
            foreach (Edge e in edges)
            {
                if (!ignoreList.Contains(e))
                {
                    e2 = e;
                    break;
                }
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

            Vector3D v1 = p[ 2 ].GetPoint3D() - p[ 0 ].GetPoint3D();
            Vector3D v2 = p[ 1 ].GetPoint3D() - p[ 0 ].GetPoint3D();

            normal = Vector3D.CrossProduct( v1, v2 );
            normal.Normalize();

            normal.Negate();

            return true;
        }
        #endregion

        #region 对边的操作
        public void AddEdge(Edge edge)
        {
            // 初始化，用第一条边
            if (edges.Count == 0)
            {
                startVertex1 = edge.Vertex1;
                startVertex2 = edge.Vertex2;
            }
            
            edges.Add(edge);
            // UpdateVertices();
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

    public class FoldUpInfo
    {
        public AbstractLayer.FaceGroup fgFix;
        public AbstractLayer.FaceGroup fgMov;
        public double angle;
        public bool IsOver;
    }
}
