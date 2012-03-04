using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Clover.RenderLayer;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Clover
{
    public class CloverController
    {
        #region 成员变量
        FaceLayer faceLayer;    /// 面层
        EdgeLayer edgeLayer;    /// 边层
        VertexLayer vertexLayer;/// 点层
        MainWindow mainWindow;  /// 你懂得
        #endregion

        #region get/set
        RenderController renderController;///渲染层
        //public RenderController RenderController
        //{
        //    get { return renderController; }
        //    //set { renderController = value; }
        //}
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

        #region 初始化
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
            vertices[1].u = 1; vertices[1].v = 0;
            vertices[2].u = 1; vertices[2].v = 1;
            vertices[3].u = 0; vertices[3].v = 1;

            // add to vertex layer
            foreach (Vertex v in vertices)
            {
                vertexLayer.InsertVertex(v);
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
        /// 切割一个面为两个面
        /// </summary>
        /// <param name="face">需要切割的面。</param>
        /// <param name="edge">割线，割线的两个端点必须在面的边上</param>
        /// <remarks>新产生的两个面会自动作为原来的面的孩子，所以就已经在面树里面了。</remarks>
        void CutAFace(Face face, Edge edge)
        {
            return;
            //旭瑜注释的呃。。。。。。。。。。。。。。。。。。。。。。。。。
            //Face f1 = new Face();
            //Face f2 = new Face();

            //face.LeftChild = f1;
            //face.RightChild = f2;

            //// 创建v1,v2，加入到verticesLayer
            //Vertex v1 = new Vertex(edge.Vertex1);
            //Vertex v2 = new Vertex(edge.Vertex2);

            //// edge index
            //int index1 = -1, index2 = -1;
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


        /// <summary>
        /// 进入折叠模式前的叶子节点表，用于恢复
        /// </summary>
        List<Face> originFaceList = new List<Face>();

        int originEdgeListCount = -1;
        int originVertexListCount = -1;

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


        /// <summary>
        /// 保存到原始叶节点表
        /// </summary>
        /// <param name="leaves">当前的叶子</param>
        /// <remarks>
        /// 当撤销的时候，只需将originFaceList里面的face的孩子都清空就可以还原面树了。
        /// </remarks>
        void SaveOriginState()
        {
            originFaceList.Clear();
            foreach ( Face f in faceLayer.Leaves)
            {
                originFaceList.Add(f);
            }

            originEdgeListCount = edgeLayer.Count;
            //originVertexListCount = vertexLayer.
        }

        /// <summary>
        /// 还原到originLeaves
        /// </summary>
        public void Revert()
        {
            // 保存进入折叠模式前的叶子节点的所有边
            List<Edge> originEdgeList = new List<Edge>();
            foreach (Face face in originFaceList)
            {
                foreach (Edge e in face.Edges)
                {
                    originEdgeList.Add(e);
                }
            }

            // 在折叠模式中的面树叶子的所有的边
            List<Edge> currentEdgeList = new List<Edge>();
            foreach (Face face in faceLayer.Leaves)
            {
                foreach (Edge e in face.Edges)
                {
                    currentEdgeList.Add(e);
                }
            }

            List<Edge> beDeletedEdges = currentEdgeList.Except(originEdgeList).ToList();

            foreach (Edge edge in beDeletedEdges)
            {
                edge.Parent = null;
            }

            edgeLayer.EdgeTreeList.RemoveRange(originEdgeListCount, edgeLayer.EdgeTreeList.Count - originEdgeListCount);

            foreach (Face face in originFaceList)
            {
                if (face.LeftChild != null && face.RightChild != null)
                {
                    renderController.Delete(face.LeftChild);
                    renderController.Delete(face.RightChild);
                    renderController.New(face);
                }
                face.LeftChild = face.RightChild = null;
            }

            //renderController.UpdateAll();

            faceLayer.UpdateLeaves();

            //UpdatePaper();
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

            SaveOriginState();

            
            // 
            //Edge foldingLine = null;

            //foreach (Face face in faces)
            //{
            //    CutAFace(face, foldingLine);    // 分割面
            //    faceLayer.UpdateLeaves(face);   // 更新叶节点，局部更新
            //}

            // 假定只有一个face现在
            Face face = faces[0];

            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            
            face.LeftChild = f1;
            face.RightChild = f2;

            Vertex newV1 = vertexLayer.GetVertex(0).Clone() as Vertex;
            vertexLayer.UpdateVertex(newV1, newV1.Index);
            Vertex newV2 = vertexLayer.GetVertex(2).Clone() as Vertex;
            vertexLayer.UpdateVertex(newV2, newV2.Index);

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

            //vertexLayer.GetVertex(3).Z = 50;

            renderController.Delete(face);
            renderController.New(f1);
            renderController.New(f2);

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

        Edge CalcaluteFoldingLine(Face face, Vertex pickedVertex)
        {
            // 找到所有包含此点的面
            Face f = face;
            {
                Point3D vertex1 = new Point3D();
                Point3D vertex2 = new Point3D();

                bool findFirstVertex = false;
                bool CalculateFinished = false;
                foreach (Edge e in f.Edges)
                {

                    if (e.Vertex1 == pickedVertex)
                    {

                        Vector3D v = new Vector3D();
                        v.X = e.Vertex1.X - e.Vertex2.X;
                        v.Y = e.Vertex1.Y - e.Vertex2.Y;
                        v.Z = e.Vertex1.Z - e.Vertex2.Z;

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

                    if (e.Vertex2 == pickedVertex)
                    {

                        Vector3D v = new Vector3D();
                        v.X = e.Vertex2.X - e.Vertex1.X;
                        v.Y = e.Vertex2.Y - e.Vertex1.Y;
                        v.Z = e.Vertex2.Z - e.Vertex1.Z;

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

            return crossCount >= 2 ? true : false;
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
                    if (e.Vertex1 != currentFoldingLine.Vertex1 && e.Vertex1 != currentFoldingLine.Vertex2 && !e.Vertex1.Moved )
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

                    if (e.Vertex2 != currentFoldingLine.Vertex1 && e.Vertex2 != currentFoldingLine.Vertex2 && !e.Vertex2.Moved)
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
            renderController.AddFoldingLine(0, 0, 1, 1);
            renderController.AddFoldingLine(0, 1, 1, 0);
            //renderController.Testfuck();
            //renderController.UpdateAll();
            //renderController.DeleteAll();

            model = renderController.Entity;
           
            return model;
        }
        #endregion
    }
}
