using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Shapes;
using Clover.Visual;

namespace Clover
{
    public class CloverController
    {
        
        #region 成员变量
        FaceLayer faceLayer;    /// 面层
        EdgeLayer edgeLayer;    /// 边层
        VertexLayer vertexLayer;/// 点层
        MainWindow mainWindow;  /// 你懂得
        RenderController renderController;///渲染层
        ShadowSystem shadowSystem = new ShadowSystem();/// 影子
        #endregion

        #region get/set
        public Clover.RenderController RenderController
        {
            get { return renderController; }
        }
        public Clover.FaceLayer FaceLayer
        {
            get { return faceLayer; }
        }
        public Clover.EdgeLayer EdgeLayer
        {
            get { return edgeLayer; }
        }
        public Clover.VertexLayer VertexLayer
        {
            get { return vertexLayer; }
        }
        public List<Edge> Edges
        {
            get 
            {
                faceLayer.UpdateLeaves();
                List<Edge> list = new List<Edge>();
                foreach (Face f in faceLayer.Leaves)
                    foreach (Edge e in f.Edges)
                        list.Add(e);
                return list;
            }
        }
        public List<Face> FaceLeaves
        {
            get { return faceLayer.Leaves; }
        }
        #endregion

        #region Singleton
        static CloverController instance = null;
        static MainWindow window = null;

        public static void InitializeInstance(MainWindow mainWindow)
        {
            window = mainWindow;
        }

        public static CloverController GetInstance()
        {
            if (instance == null)
            {
                Debug.Assert(window != null);
                instance = new CloverController(window);
            }

            return instance;

        }
        #endregion

        /// <summary>
        /// 计算顶点纹理坐标
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        bool CalculateTexcoord(Vertex vertex, Edge edge)
        {
            // 判断该点是否在直线上
            if (!CloverMath.IsPointInTwoPoints(vertex.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.001))
                return false;

            // 取中间点到其中一点的距离，以及直线长度
            Vector3D v1 = vertex.GetPoint3D() - edge.Vertex1.GetPoint3D();
            Vector3D v2 = edge.Vertex2.GetPoint3D() - edge.Vertex1.GetPoint3D();

            double proportion = v1.Length / v2.Length;

            vertex.u = edge.Vertex1.u + proportion * (edge.Vertex2.u - edge.Vertex1.u);
            vertex.v = edge.Vertex1.v + proportion * (edge.Vertex2.v - edge.Vertex1.v);

            return true; 
        }

        /// <summary>
        /// 计算顶点纹理坐标
        /// </summary>
        /// <param name="vertex">需要计算纹理坐标的点</param>
        /// <param name="edge">该点所在的边</param>
        /// <param name = "length">该边根节点的总长度</param>>
        bool CalculateTexcoord(Vertex vertex, Edge edge, double length)
        {
            // 确认该点在该直线上
            double a = vertex.X - edge.Vertex1.X;
            double b = edge.Vertex2.X - edge.Vertex1.X;
            double c = vertex.Y - edge.Vertex1.Y;
            double d = edge.Vertex2.Y - edge.Vertex1.Y;
            double e = vertex.Z = edge.Vertex1.Z;
            double f = edge.Vertex2.Z - edge.Vertex1.Z;

            if (Math.Abs(a / b - c / d) > 0.1)
                return false;

            if (Math.Abs(a / b - e / f) > 0.1)
                return false;


            // 取edge两边纹理坐标的较小值
            double u, v;
            if (edge.Vertex1.u == edge.Vertex2.u)
            {
                u = edge.Vertex1.u;
            }
            else
            {
                if (edge.Vertex1.u < edge.Vertex2.u)
                {
                    // 求两点之间的距离
                    Vector3D vd = edge.Vertex1.GetPoint3D() - vertex.GetPoint3D();
                    u = edge.Vertex1.u + vd.Length / length;
                }
                else
                {
                    Vector3D vd = edge.Vertex2.GetPoint3D() - vertex.GetPoint3D();
                    u = edge.Vertex2.u + vd.Length / length;
                }
            }

            if (edge.Vertex1.v == edge.Vertex2.v)
            {
                v = edge.Vertex2.v;
            }
            else
            {
                if (edge.Vertex1.v <= edge.Vertex2.v)
                {
                    Vector3D vd = edge.Vertex1.GetPoint3D() - vertex.GetPoint3D();
                    v = edge.Vertex1.v + vd.Length / length;
                }
                else
                {
                    Vector3D vd = edge.Vertex2.GetPoint3D() - vertex.GetPoint3D();
                    v = edge.Vertex2.v + vd.Length / length;
                }
            }
            vertex.u = u;
            vertex.v = v;

            return true;
        }

        public Vertex GetPrevVersion(Vertex vertex)
        {
            List<Vertex> vGroup = vertexLayer.VertexCellTable[vertex.Index];
            if (vGroup.Count < 2)
                return null;
            return vGroup[vGroup.Count - 2];
        }

        /// <summary>
        /// 创建一条折线
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pOriginal"></param>
        /// <param name="pDestination"></param>
        /// <returns>返回创建的折线，失败则返回null</returns>
        public Edge GenerateEdge(Face f, Vertex pOriginal, Vertex pDestination)
        {
            Vector3D t = new Vector3D();
            Point3D p0 = new Point3D();
            if( !CloverMath.GetMidperpendicularInFace(f, pOriginal, pDestination, ref t, ref p0) )
                return null;

            Point3D p1 = p0 + t*Double.MaxValue;
            Point3D p2 = p0 + t*Double.MinValue;
            Vertex v1, v2;
            v1 = new Vertex(p1);
            v2 = new Vertex(p2);

            Edge newe = new Edge(v1, v2);
            Vertex vresult1 = null;
            Vertex vresult2 = null;
            foreach (Edge e in f.Edges)
            {
                if (e.IsVerticeIn(pOriginal))
                {
                    Point3D p = new Point3D();
                    CloverMath.GetIntersectionOfTwoSegments( newe, e, ref p );
                    if (vresult1 == null)
                    {
                        vresult1 = new Vertex( p );
                    }
                    else
                    {
                        vresult2 = new Vertex( p );
                    }
                }
            }
            if (vresult1 == null || vresult2 == null)
            {
                return null;
            }
            return new Edge( vresult1, vresult2 );
        }

        #region 初始化
        void AddVisualInfoToVertex(Vertex v)
        {
            VertexInfoVisual vi = new VertexInfoVisual(v);
            VisualController.GetSingleton().AddVisual(vi);
            v.Update += vi.UpdateInfoCallBack;
            //CubeNavigator.GetInstance().Update += vi.UpdateInfoCallBack;
            vi.Start();
        }

        public void Initialize(float width, float height)
        {
            // Create 4 original vertices
            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex(-width / 2, height / 2, 0);
            vertices[1] = new Vertex(-width / 2, -height / 2, 0);
            vertices[2] = new Vertex(width / 2, -height / 2, 0);
            vertices[3] = new Vertex(width / 2, height / 2, 0);
            // 初始化纹理坐标
            vertices[0].u = 0; vertices[0].v = 0;
            vertices[1].u = 0; vertices[1].v = 1;
            vertices[2].u = 1; vertices[2].v = 1;
            vertices[3].u = 1; vertices[3].v = 0;

            // add to vertex layer
            foreach (Vertex v in vertices)
            {
                vertexLayer.InsertVertex(v);

                AddVisualInfoToVertex(v);
            }

            // create a face
            Face face = new Face(0);

            // creates 4 edges
            Edge[] edges = new Edge[4];

            // create one face and four edges
            for (int i = 0; i < 4; i++)
            {
                edges[i] = new Edge(vertices[i], vertices[i + 1 < 4 ? i + 1 : 0]);
                EdgeTree tree = new EdgeTree(edges[i]);
                edgeLayer.AddTree(tree);

                face.AddEdge(edges[i]);
            }

            // use root to initialize facecell tree and lookuptable
            faceLayer.Initliaze(face);

            face.UpdateVertices();
            faceLayer.UpdateLeaves();
        }

        /// <summary>
        /// 获取所有拥有该顶点的面
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Face> GetReferencedFaces(Vertex vertex)
        {
            List<Face> faces = new List<Face>();

            foreach (Face f in faceLayer.Leaves)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.IsVerticeIn(vertex))
                    {
                        faces.Add(f);
                        break;
                    }
                }
            }
            return faces;
        }

        private CloverController(MainWindow mainWindow)
        {
            faceLayer = new FaceLayer(this);
            edgeLayer = new EdgeLayer(this);
            vertexLayer = new VertexLayer(this);
            this.mainWindow = mainWindow;
            renderController = RenderController.GetInstance();
            //paper = new Paper("paper");
        }
        #endregion

        #region 测试折叠

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        List<Vertex> UnionVertex(params Face[] faces)
        {
            List<Vertex> vertices = new List<Vertex>();
            Dictionary<Vertex, int> dict = new Dictionary<Vertex, int>();

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i].UpdateVertices();
                foreach (Vertex v in faces[i].Vertices)
                {
                    dict[v] = 0;
                }
            }

            foreach (KeyValuePair<Vertex, int> pair in dict)
            {
                vertices.Add(pair.Key);
            }

            return vertices;
        }

        /// <summary>
        /// 在一个面中，通过两个顶点找到包含这两个顶点的边
        /// </summary>
        /// <param name="face"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        Edge FindEdgeByTwoVertexInAFace(Face face, Vertex v1, Vertex v2)
        {
            foreach (Edge e in face.Edges)
            {
                if (e.IsVerticeIn(v1) && e.IsVerticeIn(v2))
                {
                    return e;
                }
            }
            return null;
        }
        /// <summary>
        /// 当割点在一条边上时，另一割点为原来顶点时，切割一个面为两个面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedOneVertex(Face face, Edge edge, Edge cuttedEdge, Vertex cutVertex)
        {
            // 找出需要添加的点和另外一个已经存在的点 
            Vertex newVertex; Vertex otherVertex;

            newVertex = cutVertex.Clone() as Vertex;
            vertexLayer.InsertVertex(newVertex);

            if (edge.Vertex1 != cutVertex)
            {
                otherVertex = edge.Vertex1;
            }
            else
            {
                otherVertex = edge.Vertex2; 
            }
            foreach (Vertex v in face.Vertices)
            {
                if (v.GetPoint3D() == otherVertex.GetPoint3D())
                {
                    otherVertex = v;
                    break;
                }
            }


            // 切割边
            Edge newEdge1 = new Edge(cuttedEdge.Vertex1, newVertex);
            Edge newEdge2 = new Edge(newVertex, cuttedEdge.Vertex2);

            // 将新生成的边添加到原来边的左右子树
            cuttedEdge.LeftChild = newEdge1;
            cuttedEdge.RightChild = newEdge2;


            
        }

        /// <summary>
        /// 当割点在两条边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedTwoVertices(Face face, Edge edge)
        {
            Vertex newVertex1 = edge.Vertex1.Clone() as Vertex;
            Vertex newVertex2 = edge.Vertex2.Clone() as Vertex;

            AddVisualInfoToVertex(newVertex1);
            AddVisualInfoToVertex(newVertex2);

            Edge newEdge = new Edge(newVertex1, newVertex2);

            vertexLayer.InsertVertex(newVertex1);
            vertexLayer.InsertVertex(newVertex2);


            // e1包含newVertex1，e2包含newVertex2
            Edge e1 = null, e2 = null;
            foreach (Edge e in face.Edges)
            {
                if (e.IsVerticeIn(newVertex1))
                {
                    e1 = e;
                }
                else if (e.IsVerticeIn(newVertex2))
                {
                    e2 = e;
                }
            }

            // 计算newVertex1和newVertex2的纹理坐标
            CalculateTexcoord(newVertex1, e1);
            CalculateTexcoord(newVertex2, e2);

            Debug.Assert(e1 != null && e2 != null);

            // 分割e1
            Edge edge1_1 = new Edge(e1.Vertex1.Clone() as Vertex, newVertex1);
            Edge edge1_2 = new Edge(e1.Vertex2.Clone() as Vertex, newVertex1);
            e1.LeftChild = edge1_1;
            e1.RightChild = edge1_2;

            // 分割e2
            Edge edge2_1 = new Edge(e2.Vertex1.Clone() as Vertex, newVertex2);
            Edge edge2_2 = new Edge(e2.Vertex2.Clone() as Vertex, newVertex2);
            e2.LeftChild = edge2_1;
            e2.RightChild = edge2_2;


            // 生成一个面的周围的顶点的环形表
            face.UpdateVertices();
            List<Vertex> vertexList = new List<Vertex>();
            vertexList.AddRange(face.Vertices);
            vertexList.Add(face.Vertices[0]);

            // 割点的要插入的下标
            int indexV1 = -1;
            int indexV2 = -1;
            // 要被分割的边
            Edge beCutEdge1 = null;     
            Edge beCutEdge2 = null;

            // 原始边
            List<Edge> rangeA = new List<Edge>();
            List<Edge> rangeB = new List<Edge>();
            List<Edge> lastRange = null;

            // 分割面
            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            face.LeftChild = f1;
            face.RightChild = f2;

            // 更新新边的Faces指针 
            newEdge.Face1 = f1;
            newEdge.Face2 = f2;

            // 将边分成两组，一个面得到一组
            bool bFirstRange = true;
            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                if (bFirstRange)
                {
                    rangeA.Add(FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]));
                }
                else
                {
                    rangeB.Add(FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]));
                }

                if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    bFirstRange = false;

                    // 更新纹理坐标，这个应该算个比例，我先取中点作为新点的坐标，到时再改
                    newVertex1.u = (vertexList[i].u + vertexList[i + 1].u) / 2;
                    newVertex1.v= (vertexList[i].v + vertexList[i + 1].v) / 2;

                    indexV1 = i;
                    beCutEdge1 = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);
                    rangeA.RemoveAt(rangeA.Count - 1);
                    Edge cutEdge1 = new Edge(vertexList[i], newVertex1);
                    Edge cutEdge2 = new Edge(newVertex1, vertexList[i + 1]);
                    rangeA.Add(cutEdge1);
                    rangeA.Add(newEdge);
                    rangeB.Add(cutEdge2);
                }
                if (CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001))
                {
                    if (bFirstRange)
                    {
                        bFirstRange = false;
                        List<Edge> temp = rangeA;
                        rangeA = rangeB;
                        rangeB = temp;
                    }
                    else
                    {
                        bFirstRange = true;
                    }

                    // 更新纹理坐标，这个应该算个比例，我先取中点作为新点的坐标，到时再改
                    newVertex2.u = (vertexList[i].u + vertexList[i + 1].u) / 2;
                    newVertex2.v= (vertexList[i].v + vertexList[i + 1].v) / 2;

                    indexV2 = i;
                    beCutEdge2 = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);
                    rangeB.RemoveAt(rangeB.Count - 1);
                    Edge cutEdge1 = new Edge(vertexList[i], newVertex2);
                    Edge cutEdge2 = new Edge(newVertex2, vertexList[i + 1]);
                    rangeB.Add(cutEdge1);
                    rangeB.Add(newEdge);
                    rangeA.Add(cutEdge2);
                }
            }

            Debug.Assert(indexV1 != -1 && indexV2 != -1);


            foreach (Edge e in rangeA)
            {
                f1.AddEdge(e);
            }
            foreach (Edge e in rangeB)
            {
                f2.AddEdge(e);
            }

            f1.UpdateVertices();
            f2.UpdateVertices();

            // 找到所有需要保存到VertexLayer历史的顶点
            List<Vertex> oldVertexList = UnionVertex(f1, f2);
            oldVertexList.Remove(newVertex1);
            oldVertexList.Remove(newVertex2);

            // 为所有的顶点生成一个副本插到历史中。
            shadowSystem.SaveVertices(oldVertexList);

            // 更新新的面的顶点到最新版
            shadowSystem.UpdateFaceVerticesToLastedVersion(f1);
            shadowSystem.UpdateFaceVerticesToLastedVersion(f2);

            // 更新渲染层的部分
            renderController.Delete(face);
            renderController.New(f1);
            renderController.New(f2);

            newVertex1.Update(newVertex1, null);
            newVertex2.Update(newVertex2, null);
            //renderController.AddFoldingLine(newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v);

            FaceLayer.UpdateLeaves();
        }
       

        /// <summary>
        /// 切割一个面为两个面
        /// </summary>
        /// <param name="face">需要切割的面。</param>
        /// <param name="edge">割线，割线的两个端点必须在面的边上</param>
        /// <remarks>新产生的两个面会自动作为原来的面的孩子，所以就已经在面树里面了。</remarks>
        void CutAFace(Face face, Edge edge)
        {
            return;
            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);

            face.LeftChild = f1;
            face.RightChild = f2;

            Edge e1 = null, e2 = null;
            foreach (Edge e in face.Edges)
            {
                if (e.IsVerticeIn(edge.Vertex1))
                    e1 = e;
                if (e.IsVerticeIn(edge.Vertex2))
                    e2 = e;
            }

            int type = 0;
            bool isVertex1Cut = false;
            bool isVertex2Cut = false;

            if (!CloverMath.IsTwoPointsEqual(edge.Vertex1.GetPoint3D(), e1.Vertex1.GetPoint3D(), 0.0001) &&
                 !CloverMath.IsTwoPointsEqual(edge.Vertex1.GetPoint3D(), e1.Vertex2.GetPoint3D(), 0.0001))
            {
                type++;
                isVertex1Cut = true;
            }

            if (!CloverMath.IsTwoPointsEqual(edge.Vertex2.GetPoint3D(), e2.Vertex1.GetPoint3D(), 0.0001) &&
                !CloverMath.IsTwoPointsEqual(edge.Vertex2.GetPoint3D(), e2.Vertex2.GetPoint3D(), 0.0001))
            {
                type++;
                isVertex2Cut = true;
            }
            //// 判断折线的两个顶点是否是当前顶点列表中所存在的顶点
            //int index1, index2;
            
            //// 创建v1,v2，加入到verticesLayer
            //Vertex v1 = new Vertex(edge.Vertex1);
            //Vertex v2 = new Vertex(edge.Vertex2);
    
            //// 判断顶点在原来的vertexLayer中是否存在，所不存在则插入，并返回新顶点的索引
            //index1 = vertexLayer.IsVertexExist(v1);
            //index2 = vertexLayer.IsVertexExist(v2);

            //if (-1 == index1)
            //{
            //    type++; 
            //    index1 = vertexLayer.InsertVertex(v1);
            //}

            //if (-1 == index2)
            //{
            //    type++;
            //    index2 = vertexLayer.InsertVertex(v2);
            //}

            // type 0:没有增加顶点， type 1：增加了一个顶点 type 2：增加了两个顶点
            switch (type)
            { 
                case 0:
                    // 种类1 不新增加顶点。只创建一条边并加入边层
                    edgeLayer.AddTree(new EdgeTree(edge));
                    f1.AddEdge(edge);
                    f2.AddEdge(edge);

                    // 按照顶点序环绕，查找边，并注册到面

                    break;
                case 1:
                    // 种类2 新增加一个顶点，即只为一条边做切割，并更新面节点
                    edgeLayer.AddTree(new EdgeTree(edge));
                    
                    // 取其中一个进行面分割的边.
                    if (isVertex1Cut)
                    {
                        Vertex v = (Vertex)(edge.Vertex1.Clone());
                        int index = vertexLayer.InsertVertex(v);
                        if (index < 0) return;

                        e1.LeftChild = new Edge(e1.Vertex1, v);
                        e1.RightChild = new Edge(v, e1.Vertex2);

                        // 将生成的边分别注册给新生成的两个面
                        f1.AddEdge(edge);
                        f1.AddEdge(e1.LeftChild);

                        f2.AddEdge(edge);
                        f2.AddEdge(e1.RightChild);


                    }
                    break; 
                case 2:
                    // 种类3
                    break;
                default:
                    break;
            }

            //// 将折线作为一颗边树的根，插入到边层
            //edgeLayer.AddTree(new EdgeTree(edge));

            //// 根据折线点分割边节点
            //foreach (EdgeTree et in edgeLayer.EdgeTreeList)
            //{
            //    foreach (Edge e in et.Leaves())
            //    {
            //        if (CloverMath.IsPointInTwoPoints(v1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0))
            //        { 
            //            e.LeftChild = new Edge(e.Vertex1, v1);
            //            e.RightChild = new Edge(v1, e.Vertex2);
            //        }
                        
            //    }
            //}

            //for ( int i = 0; i < face.Edges.Count; i++)
            //{
            //    if (face.Edges[i].IsVerticeIn(v1))
            //    {
            //        index1 = i;
            //        Edge e1 = new Edge(face.Edges[i].Vertex1.Clone() as Vertex, v1);
            //        Edge e2 = new Edge(v1, face.Edges[i].Vertex2.Clone() as Vertex);

            //        face.Edges[i].LeftChild = e1;
            //        face.Edges[i].RightChild = e2;
            //    }
            //    if (face.Edges[i].IsVerticeIn(v2))
            //    {
            //        index2 = i;
            //        Edge e1 = new Edge(face.Edges[i].Vertex1.Clone() as Vertex, v2);
            //        Edge e2 = new Edge(v2, face.Edges[i].Vertex2.Clone() as Vertex);

            //        face.Edges[i].LeftChild = e1;
            //        face.Edges[i].RightChild = e2;
            //    }
            //}

            //Debug.Assert(index1 != -1 && index2 != -1);

            //// 确保逻辑上v1在v2前面
            //if (index1 > index2)
            //{
            //    Vertex temp = v1;
            //    v1 = v2;
            //    v2 = temp;
            //    int tempIndex = index1;
            //    index1 = index2;    
            //    index2 = tempIndex;
            //}

            //Edge newCutEdge = new Edge(v1, v2);


            //face.UpdateVertices();
            //for ( int i = 0; i <= index1; i++)
            //{
            //    f1.AddEdge(new Edge(new Vertex(face.Vertices[i]), new Vertex(face.Vertices[i + 1])));
            //}
            ////f1.AddEdge(newEdge);
            //for ( int i = 0; i <= index1; i++)
            //{
            //    f1.AddEdge(new Edge(new Vertex(face.Vertices[i]), new Vertex(face.Vertices[i + 1])));
            //}

        }


        /// <summary>
        /// 当前都影响的面，在拖动的过程中需要实时计算，因为随时会有新的受影响
        /// 的产生或者老的受影响的面被移除。
        /// </summary>
        List<Face> affectedFaceList = new List<Face>();



        Edge currentFoldingLine = new Edge(new Vertex(), new Vertex());

    	public Edge CurrentFoldingLine
    	{
    		get { return currentFoldingLine; }
    	}

        void UpdateAffectedFaceList()
        {

        }

        public void UpdateVertexPosition(Vertex vertex, double xOffset, double yOffset)
        {
            vertex = vertexLayer.GetVertex(3);
            vertex.X += xOffset;
            vertex.Y += yOffset;

            renderController.Update(faceLayer.Leaves[1]);

        }

        public void Revert()
        {
            shadowSystem.Revert();
        }

        /// <summary>
        /// 开始折叠模式
        /// </summary>
        /// <param name="faces">需要折叠的面</param>
        /// <remarks>
        /// 首先保存原始面树的叶子。
        /// 当撤销的时候，只需将originFaceList里面的face的孩子都清空就可以还原面树了。
        /// 对于边树，我们将在当前叶子节点的面的边而不在originLeaves的边移除。
        /// </remarks>
        public void StartFoldingModel(List<Face> faces)
        {
            faces = faceLayer.Leaves;

            shadowSystem.SaveOriginState();

            // 假定只有一个face现在
            Face face = faces[0];
            shadowSystem.UpdateFaceVerticesToLastedVersion(face);

            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            
            face.LeftChild = f1;
            face.RightChild = f2;

            Vertex newV1 = vertexLayer.GetVertex(0).Clone() as Vertex;
            //vertexLayer.UpdateVertex(newV1, newV1.Index);
            Vertex newV2 = vertexLayer.GetVertex(2).Clone() as Vertex;
            //vertexLayer.UpdateVertex(newV2, newV2.Index);

            Edge newEdge = new Edge(newV1, newV2);
            edgeLayer.AddTree(new EdgeTree(newEdge));

            f1.AddEdge(face.Edges[0]);
            f1.AddEdge(face.Edges[1]);
            f1.AddEdge(newEdge);

            f2.AddEdge(face.Edges[2]);
            f2.AddEdge(face.Edges[3]);
            f2.AddEdge(newEdge);

            // for testing
            currentFoldingLine = newEdge;

            f1.UpdateVertices();
            f2.UpdateVertices();

            // 保存新的面的所有顶点的历史
            List<Vertex> totalVertices = f1.Vertices.Union(f2.Vertices).ToList();
            shadowSystem.SaveVertices(totalVertices);

            shadowSystem.UpdateFaceVerticesToLastedVersion(f1);
            shadowSystem.UpdateFaceVerticesToLastedVersion(f2);

            //vertexLayer.GetVertex(3).Z = 50;

            renderController.Delete(face);
            renderController.New(f1);
            renderController.New(f2);

            // new a transparent face
            Face tranFace = f2.Clone() as Face;
            shadowSystem.CreateTransparentFace(tranFace);

            faceLayer.UpdateLeaves(face);
        }

        #endregion

        #region 更新
        public void InitializeBeforeFolding(Vertex vertex)
        {
            // 计算和创建一条新的折线

            // 新增数据结构的信息
            //   1.顶点
            //   2.边
            //   3.面
            //   over...

            // 
        }


        float currentAngel;
        Point3D currentVertex;

        
        /// <summary>
        /// 创建初始的折线顶点·
        /// </summary>
        /// <param name="xRel"></param>
        /// <param name="yRel"></param>
        Edge CalculateFoldingLine(Vertex pickedVertex)
        {
            // 找到所有包含此点的面
            foreach(Face f in faceLayer.Leaves)
            {
                Point3D vertex1 = new Point3D();
                Point3D vertex2 = new Point3D();

                bool findFirstVertex = false;
                bool CalculateFinished = false;
                foreach (Edge e in f.Edges)
                {
                    // 边的第一个顶点是否是选中点
                    if (e.Vertex1 == pickedVertex)
                    {

                        Vector3D v = new Vector3D();
                        v.X = e.Vertex2.X - e.Vertex1.X;
                        v.Y = e.Vertex2.Y - e.Vertex1.Y;
                        v.Z = e.Vertex2.Z - e.Vertex1.Z;

                        v.Normalize();
                        if (!findFirstVertex)
                        {
                            vertex1.X = e.Vertex1.X + v.X;
                            vertex1.Y = e.Vertex1.Y + v.Y;
                            vertex1.Z = e.Vertex1.Z + v.Z;
                            findFirstVertex = true;
                        }
                        else
                        {
                            vertex2.X = e.Vertex1.X + v.X;
                            vertex2.Y = e.Vertex1.Y + v.Y;
                            vertex2.Z = e.Vertex1.Z + v.Z;
                            CalculateFinished = true;
                        }
                    }
                    
                    // 边的第二个顶点是否是选中点
                    if (e.Vertex2 == pickedVertex)
                    {

                        Vector3D v = new Vector3D();
                        v.X = e.Vertex1.X - e.Vertex2.X;
                        v.Y = e.Vertex1.Y - e.Vertex2.Y;
                        v.Z = e.Vertex1.Z - e.Vertex2.Z;

                        v.Normalize();

                        if (!findFirstVertex)
                        {
                            vertex1.X = e.Vertex2.X + v.X;
                            vertex1.Y = e.Vertex2.Y + v.Y;
                            vertex1.Z = e.Vertex2.Z + v.Z;
                            findFirstVertex = true;
                        }
                        else
                        {
                            vertex2.X = e.Vertex2.X + v.X;
                            vertex2.Y = e.Vertex2.Y + v.Y;
                            vertex2.Z = e.Vertex2.Z + v.Z;
                            CalculateFinished = true;
                        }


                        
                    }
                    if (CalculateFinished)
                    {
                        Vertex cVertex1 = new Vertex(vertex1);
                        Vertex cVertex2 = new Vertex(vertex2);

                        Edge edge = new Edge(cVertex1, cVertex2);
                        return edge;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 判断当前面是否是个需要移动的面
        /// </summary>

        bool TestMovedFace(Face face, Face pickedFace, Vertex pickedVertex)
        {
            // 选定的面一定是移动面
            if (face == pickedFace)
                return true;

            // 所有和移动面有共同边的面都是移动面,即拥有选择点的面
            foreach (Edge e in face.Edges)
            {
                if (e.Vertex1 == pickedVertex || e.Vertex2 == pickedVertex)
                {
                    return true; 
                }
            }

            // 若有面覆盖在该面上，也为移动面
            // 需要面分组中的层次信息
            // bla bla bla.

            return false;
        }

        /// <summary>
        /// 判断折线是否通过该平面
        /// </summary>
        /// <param name="face">当前判定平面</param>
        /// <param name="currentFoldingLine">折线亮点坐标</param>
        /// <returns></returns>
        bool TestFoldingLineCrossed(Face face, Edge currentFoldingLine)
        {
            // 求出折线向量
            Vector3D u = new Vector3D();
            u.X = currentFoldingLine.Vertex2.X - currentFoldingLine.Vertex1.X;
            u.Y = currentFoldingLine.Vertex2.Y - currentFoldingLine.Vertex1.Y;
            u.Z = currentFoldingLine.Vertex2.Z - currentFoldingLine.Vertex1.Z;

            // 判定面中的每条边与折线是否相交，若有两条相交则折线分割该平面
            int crossCount = 0;
            foreach (Edge edge in face.Edges)
            {
                Vector3D v = new Vector3D();
                v.X = edge.Vertex2.X - edge.Vertex1.X;
                v.Y = edge.Vertex2.Y - edge.Vertex1.Y;
                v.Z = edge.Vertex2.Z - edge.Vertex1.Z;

                Vector3D w = new Vector3D();
                w.X = currentFoldingLine.Vertex1.X - edge.Vertex1.X;
                w.Y = currentFoldingLine.Vertex1.Y - edge.Vertex1.Y;
                w.Z = currentFoldingLine.Vertex1.Z - edge.Vertex1.Z;

                double a = Vector3D.DotProduct(u, u);
                double b = Vector3D.DotProduct(u, v);
                double c = Vector3D.DotProduct(v, v);
                double d = Vector3D.DotProduct(u, w);
                double e = Vector3D.DotProduct(v, w);
                double D = a * c - b * b;
                double sc, tc;

                // 两条线平行
                if (D < 0.00001)
                {
                    return false;
                }
                else
                {
                    sc = (b * e - c * d) / D;
                    tc = (a * e - b * d) / D;

                    // 判断折线点是否在线段上
                    if (sc != 0.0f && sc != 1.0f)
                    {
                        continue;
                    }
                }

                // sc, tc 分别为两条直线上的比例参数
                Vector3D dp = new Vector3D();
                dp = w + (sc * u) - (tc * v);

                if (dp.Length < 0.00001)
                {
                    crossCount++;
                }
            }

            return crossCount >= 2;
        }

        /// <summary>
        /// 根据鼠标位移在每个渲染帧前更新结构
        /// </summary>
        /// <param name="xRel">鼠标的x位移</param>
        /// <param name="yRel">鼠标的y位移</param>
        /// <param name="faceList">折叠所受影响的面</param>
        public void Update(float xRel, float yRel, Vertex pickedVertex, Face pickedFace)
        {
            // testing
            if (faceLayer.Leaves.Count < 2)
                return;

            // 假设已经选取了左上角的点，主平面
            pickedVertex = vertexLayer.GetVertex(3);
            pickedFace = faceLayer.Leaves[1];
            //pickedFace = faceLayer.FacecellTree.Root;

            // 计算初始折线
            //currentFoldingLine = CalculateFoldingLine(pickedVertex);

            // 创建移动面分组
            List<Face> faceWithFoldingLine = new List<Face>();
            List<Face> faceWithoutFoldingLine = new List<Face>();

            // 根据面组遍历所有面，判定是否属于移动面并分组插入
            foreach (Face face in faceLayer.Leaves)
            {
                if (TestMovedFace(face, pickedFace, pickedVertex))
                {
                    if (TestFoldingLineCrossed(face, currentFoldingLine))
                    {
                        faceWithFoldingLine.Add(face);
                    }
                    else
                    {
                        faceWithoutFoldingLine.Add(face);
                    }
                }
            }

            // 对于所有有折线经过的面，对面进行切割
            foreach (Face face in faceWithFoldingLine)
            {
                CutAFace(face, currentFoldingLine); 
                // 选取有拾取点的那个面为移动面，加入到没有折线面分组

                bool findMovedFace = false;
                foreach (Edge e in face.LeftChild.Edges)
                {
                    if (e.Vertex1 == pickedVertex || e.Vertex2 == pickedVertex)
                    {
                        faceWithoutFoldingLine.Add(face.LeftChild);
                        findMovedFace = true;
                        break;
                    }
                }

                if (!findMovedFace)
                    faceWithoutFoldingLine.Add(face.RightChild);
            }

            // Testing
            faceWithoutFoldingLine.Add(pickedFace);

            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in faceWithoutFoldingLine)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.Vertex1.GetPoint3D() != currentFoldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex1.GetPoint3D() != currentFoldingLine.Vertex2.GetPoint3D() && !e.Vertex1.Moved )
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = currentFoldingLine.Vertex1.X - currentFoldingLine.Vertex2.X;
                        axis.Y = currentFoldingLine.Vertex1.Y - currentFoldingLine.Vertex2.Y;
                        axis.Z = currentFoldingLine.Vertex1.Z - currentFoldingLine.Vertex2.Z;

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, 0.1 * yRel);

                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);

                        e.Vertex1.SetPoint3D(rotateTransform.Transform(e.Vertex1.GetPoint3D()));
                        e.Vertex1.Moved = true;
                    }

                    if (e.Vertex2.GetPoint3D() != currentFoldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex2.GetPoint3D() != currentFoldingLine.Vertex2.GetPoint3D() && !e.Vertex2.Moved)
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = currentFoldingLine.Vertex1.X - currentFoldingLine.Vertex2.X;
                        axis.Y = currentFoldingLine.Vertex1.Y - currentFoldingLine.Vertex2.Y;
                        axis.Z = currentFoldingLine.Vertex1.Z - currentFoldingLine.Vertex2.Z;

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, 0.1 * yRel);

                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);

                        e.Vertex2.SetPoint3D(rotateTransform.Transform(e.Vertex2.GetPoint3D()));
                        e.Vertex2.Moved = true;
                    }
                }
            }

            // 判断是否贴合，若有贴合更新组


            // 修正所有点的移动属性
            foreach (Vertex v in vertexLayer.Vertices)
            {
                v.Moved = false; 
            }

            renderController.UpdateAll();
        }

        #endregion

        #region 更新图形层的模型
        ModelVisual3D model = new ModelVisual3D();
        public System.Windows.Media.Media3D.ModelVisual3D Model
        {
            get { return model; }
            set { model = value; }
        }
        public ModelVisual3D UpdatePaper()
        {
            //faceLayer.UpdateLeaves();
            ////paper.Begin("BaseWhiteNoLight", Mogre.RenderOperation.OperationTypes.OT_TRIANGLE_FAN);



            //MeshGeometry3D triangleMesh = new MeshGeometry3D();

            //foreach (Vertex v in vertexLayer.Vertices)
            //{
            //    triangleMesh.Positions.Add(new Point3D(v.X, v.Y, v.Z));
            //}

            //foreach (Face face in faceLayer.Leaves)
            //{
            //    face.UpdateVertices();
            //    for (int i = 1; i < face.Vertices.Count - 1; i++)
            //    {
            //        triangleMesh.TriangleIndices.Add(face.Vertices[0].Index);
            //        triangleMesh.TriangleIndices.Add(face.Vertices[i].Index);
            //        triangleMesh.TriangleIndices.Add(face.Vertices[i + 1].Index);

            //        Debug.WriteLine(face.Vertices[i].point);
            //    }
            //}

            //Material material = new DiffuseMaterial(
            //    new SolidColorBrush(Colors.DarkKhaki));
            //GeometryModel3D triangleModel = new GeometryModel3D(
            //    triangleMesh, material);
            //triangleModel.BackMaterial = material;
            //model.Content = triangleModel;

            if (renderController == null)
                return model;

            renderController.DeleteAll();

            MaterialGroup mgf = new MaterialGroup();
            ImageBrush imb = new ImageBrush();
            imb.ImageSource = new BitmapImage(new Uri(@"media/paper/paper1.jpg", UriKind.Relative));
            mgf.Children.Add(new DiffuseMaterial(imb));

            MaterialGroup mgb = new MaterialGroup();
            mgb.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.OldLace)));
            renderController.FrontMaterial = mgf;
            renderController.BackMaterial = mgb;

            faceLayer.UpdateLeaves();
            foreach (Face face in faceLayer.Leaves)
            {
                face.UpdateVertices();
                renderController.New(face);
            }
            
            //test
            //renderController.AddFoldingLine(0, 0, 1, 1);
            //renderController.AddFoldingLine(0, 1, 1, 0);
            //renderController.Testfuck();
            //renderController.UpdateAll();
            //renderController.DeleteAll();

            model = renderController.Entity;
           
            return model;
        }
        #endregion

        #region Neil测试
        public void NeilTest()
        {
            //创建4个顶点
            Vertex v0 = new Vertex(123, -50, 23.4);
            Vertex v1 = new Vertex(-50, 40.2304234, 30.23423423);
            Vertex v2 = new Vertex(-50, 40.2304234, 30.23423423);
            Vertex v3 = new Vertex(95.234234, 22, 0);

            Edge e1 = new Edge(v0, v1);
            Edge e2 = new Edge(v2, v3);
            Point3D p = new Point3D();

            double x = CloverMath.GetDistanceBetweenTwoSegments(e1, e2);
            int y = CloverMath.GetIntersectionOfTwoSegments(e1, e2, ref p);
            return;
        }
        #endregion
    }
}
