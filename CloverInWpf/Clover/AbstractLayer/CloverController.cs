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
using Clover.AbstractLayer;
using Clover.IO;
using System.Threading;

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
        FoldingSystem foldingSystem = new FoldingSystem();///折叠系统
        FaceGroupLookupTable faceGroupLookupTable;
        CloverFileWriter fileWriter = new CloverFileWriter();
        CloverFileLoader fileLoader = new CloverFileLoader();
        #endregion

        #region get/set

        public Clover.FaceGroupLookupTable FaceGroupLookupTable
        {
            get { return faceGroupLookupTable; }
            set { faceGroupLookupTable = value; }
        }
        public Clover.FoldingSystem FoldingSystem
        {
            get { return foldingSystem; }
        }
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
        public Clover.ShadowSystem ShadowSystem
        {
            get { return shadowSystem; }
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

        #region 文件操作
        public void SaveFile(string filename)
        {
            fileWriter.SaveFile(filename, faceLayer, edgeLayer, vertexLayer);
        }
        public void LoadFile(string filename)
        {
            fileLoader.LoadFromFile(filename);

            faceLayer = fileLoader.FaceLayer;
            edgeLayer = fileLoader.EdgeLayer;
            vertexLayer = fileLoader.VertexLayer;

            renderController.DeleteAll();
            //renderController.RedrawFoldLine();
            foreach (Face face in faceLayer.Leaves)
            {
                renderController.New(face);
            }
        }
        #endregion

        #region 动画
        delegate void RotateFacesHandle(List<Face> list, Edge foldLine, double angle);
        delegate void CutFacesHandle(List<Face> list, Edge foldLine);

        public void AnimatedCutFaces(List<Face> beCutFaceList, Edge edge)
        {
            List<Face> faceList = new List<Face>();
            faceList.AddRange(beCutFaceList);

            mainWindow.Dispatcher.Invoke(new CutFacesHandle(CloverController.GetInstance().foldingSystem.CutFaces),
                faceList, edge);
        }

        public void AnimatedRotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            List<Face> faceList = new List<Face>();
            faceList.AddRange(beRotatedFaceList);

            double interval = angle / 1000.0;

            for (int i = 0; i < 1000; i++)
            {
                mainWindow.Dispatcher.Invoke(new RotateFacesHandle(CloverController.GetInstance().foldingSystem.RotateFaces),
                    faceList, foldingLine, interval);
            }
        }
        #endregion

        public void FlipFace(Face face)
        {
            face.Flip();
            renderController.Update(face);
        }

        public Vertex GetPrevVersion(Vertex vertex)
        {
            List<Vertex> vGroup = vertexLayer.VertexCellTable[vertex.Index];
            if (vGroup.Count < 2)
                return null;
            return vGroup[vGroup.Count - 2];
        }

        /// <summary>
        /// 更新折线
        /// </summary>
        /// <param name="face"></param>
        /// <param name="pickedPoint"></param>
        /// <param name="projectionPoint"></param>
        /// <returns></returns>
 
        public Edge UpdateFoldingLine(Face face, Point3D pickedPoint, Point3D projectionPoint)
        {   
            Edge e = UpdateFoldingLine(face, new Vertex(pickedPoint), new Vertex(projectionPoint));
            //currentFoldingLine = e;
            //renderController.AddFoldingLine(e.Vertex1.u, e.Vertex1.v, e.Vertex2.u, e.Vertex2.v);
            return e;
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

                //renderController.AddVisualInfoToVertex(v);
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
                edges[i].Face1 = face;
            }

            // use root to initialize facecell tree and lookuptable
            faceLayer.Initliaze(face);

            face.UpdateVertices();
            faceLayer.UpdateLeaves();

            faceGroupLookupTable = new FaceGroupLookupTable(face);

            // 此处也应该拍一张快照
            SnapshotNode node = new SnapshotNode(faceLayer.Leaves);
            // 为了方便revert设计，详情联系 ET
            node.Type = SnapshotNodeKind.CutKind;
            node.OriginVertexListCount = vertexLayer.VertexCellTable.Count;
            node.OriginEdgeListCount = edgeLayer.Count;
            shadowSystem.Snapshot(node);
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
            faceLayer = new FaceLayer();
            edgeLayer = new EdgeLayer();
            vertexLayer = new VertexLayer();
            this.mainWindow = mainWindow;
            renderController = RenderController.GetInstance();
            //paper = new Paper("paper");
        }
        #endregion

        #region 测试折叠


        /// <summary>
        /// 当割点在两条边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public void CutFace(Face face, Edge edge)
        {
            foldingSystem.CutFace(face, edge);
          
        }

       
        public void CutFaces(List<Face> faces,  Edge edge)
        {
            foldingSystem.CutFaces(faces, edge);
            //CloverController.GetInstance().FaceGroupLookupTable.UpdateTableAfterFoldUp();
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

        public void Undo()
        {
            shadowSystem.Undo();
        }

        public void Redo()
        {
            shadowSystem.Redo();
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
            //faces = faceLayer.Leaves;

            //shadowSystem.SaveOriginState();

            //// 假定只有一个face现在
            //Face face = faces[0];

            //shadowSystem.UpdateFaceVerticesToLastedVersion(face);

            //Face f1 = new Face(face.Layer);
            //Face f2 = new Face(face.Layer);
            
            //face.LeftChild = f1;
            //face.RightChild = f2;

            //Vertex newV1 = vertexLayer.GetVertex(0).Clone() as Vertex;
            ////vertexLayer.UpdateVertex(newV1, newV1.Index);
            //Vertex newV2 = vertexLayer.GetVertex(2).Clone() as Vertex;
            ////vertexLayer.UpdateVertex(newV2, newV2.Index);

            //Edge newEdge = new Edge(newV1, newV2);
            //edgeLayer.AddTree(new EdgeTree(newEdge));

            //f1.AddEdge(face.Edges[0]);
            //f1.AddEdge(face.Edges[1]);
            //f1.AddEdge(newEdge);

            //f2.AddEdge(face.Edges[2]);
            //f2.AddEdge(face.Edges[3]);
            //f2.AddEdge(newEdge);

            //// for testing
            //currentFoldingLine = newEdge;

            //f1.UpdateVertices();
            //f2.UpdateVertices();

            
            //table.DeleteFace( face );
            //table.AddFace( f1 );
            //table.AddFace( f2 );

            //// 保存新的面的所有顶点的历史
            //List<Vertex> totalVertices = f1.Vertices.Union(f2.Vertices).ToList();
            //shadowSystem.SaveVertices(totalVertices);

            //shadowSystem.UpdateFaceVerticesToLastedVersion(f1);
            //shadowSystem.UpdateFaceVerticesToLastedVersion(f2);

            ////vertexLayer.GetVertex(3).Z = 50;

            //renderController.Delete(face);
            //renderController.New(f1);
            //renderController.New(f2);

            //// new a transparent face
            //Face tranFace = f2.Clone() as Face;
            //shadowSystem.CreateTransparentFace(tranFace);

            //faceLayer.UpdateLeaves(face);
        }

        /// <summary>
        /// 通过索引获取点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vertex GetVertex(int index)
        {
            return vertexLayer.GetVertex(index);
        }

        /// <summary>
        /// 获取一个面的折线
        /// </summary>
        /// <param name="face"></param>
        /// <param name="originVertex"></param>
        /// <param name="newVertex"></param>
        /// <returns></returns>
        public Edge GetFoldingLine(Face face, Vertex originVertex, Vertex newVertex)
        {
            return UpdateFoldingLine(face, originVertex, newVertex);
        }

        public Edge UpdateFoldingLine(Face face, Vertex originVertex, Vertex ProjectionVertex)
        {
            Vertex vertex1 = originVertex.Clone() as Vertex;
            Vertex vertex2 = ProjectionVertex.Clone() as Vertex;
            
            // 求空间折线
            Edge  foldingLine = CloverMath.GetPerpendicularBisector3D(face, originVertex.GetPoint3D(), ProjectionVertex.GetPoint3D());
            if (foldingLine == null)
                return null;

            // 计算纹理坐标
            foreach (Edge e in face.Edges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.001))
                {
                    foldingSystem.CalculateTexcoord(foldingLine.Vertex1, e);
                }

                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.001))
                {
                    foldingSystem.CalculateTexcoord(foldingLine.Vertex2, e);
                }
            }
            return foldingLine;
        }

        #endregion

        #region 与FoldingUp相关

        #region 成员变量
        // 与FoldingUp有关的成员属性
        bool firstCut = true;
        List<Face> foldingFaces = new List<Face>();
        List<Face> movingFaces = new List<Face>();
        Edge lastFoldingLine = null;
        List<Edge> lastCuttedEdges = new List<Edge>();
        List<Face> originFaces = new List<Face>();

        // set/get
        public bool FirstCut
        {
            get { return firstCut; }
            set { firstCut = value; }
        }
        #endregion

        // 相关的函数
        /// <summary>
        /// 测试FoldingUp成立条件
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="originVertex"></param>
        /// <param name="projectionPoint"></param>
        /// <param name="foldingLine"></param>
        /// <returns></returns>
        public bool DetermineFoldingUpConditionEstablished(Face pickedFace, Vertex originVertex, Point3D projectionPoint, ref Edge foldingLine)
        {
            // 不成立条件一：投影点和原始点是同一个点
            if (originVertex.GetPoint3D() == projectionPoint)
                return false;

            // 不成立条件二：投隐点和原始点的连线以及折线有穿过与该面不同组的面
            Edge segmentFromOriginToProjection = new Edge(originVertex, new Vertex(projectionPoint));
            foldingLine = foldingSystem.GetFoldingLine(pickedFace, originVertex.GetPoint3D(), projectionPoint);

            if (foldingLine == null || segmentFromOriginToProjection == null)
                return false;

            //System.Windows.MessageBox.Show("杨旭瑜改了group没法编译，只能把下面的代码注释了");

            //找到所有与该面不同组的面
            //List<Face> facesInDifferentGroup = faceLayer.Leaves.Except(faceGroupLookupTable.GetGroup(pickedFace).GetFaceList()).ToList();
            List<Face> facesInDifferentGroup = new List<Face>();
            foreach (Face face in facesInDifferentGroup)
            {
                // 求线段和面的交点 
                Point3D crossPoint = new Point3D();
                if (CloverMath.IntersectionOfLineAndFace(segmentFromOriginToProjection.Vertex1.GetPoint3D(),
                            segmentFromOriginToProjection.Vertex2.GetPoint3D(), face, ref crossPoint))
                {
                    if (CloverMath.IsPointInTwoPoints(crossPoint, segmentFromOriginToProjection.Vertex1.GetPoint3D(),
                                                        segmentFromOriginToProjection.Vertex2.GetPoint3D(), 0.0001))
                        return false;
                }

                if (CloverMath.IntersectionOfLineAndFace(foldingLine.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D(),
                                                        face, ref crossPoint))
                {
                    if (CloverMath.IsPointInTwoPoints(crossPoint, foldingLine.Vertex1.GetPoint3D(),
                                                        foldingLine.Vertex2.GetPoint3D(), 0.0001))
                        return false;
                }
            }

            return true; 
        }

        /// <summary>
        /// 判断本次拖拽是否有切割新的面
        /// </summary>
        /// <param name="foldingFaces"></param>
        /// <param name="originVertex"></param>
        /// <param name="projetionPoint"></param>
        /// <returns></returns>
        public bool JudgeCutAnotherFace(Face pickedFace, List<Face> foldingFaces, Vertex originVertex, Point3D projectionPoint, Edge foldingLine)
        {
            // 取出与当前折叠面在同一组并且更高层的所有面  
            int currentLayer = 0;
            foreach (Face face in foldingFaces)
            {
                if (face.Layer > currentLayer)
                    currentLayer = face.Layer;
            }

            List<Face> group = faceGroupLookupTable.GetGroup(pickedFace).GetFaceList();
            List<Face> upperFaces = new List<Face>();
            foreach (Face face in group)
            {
                if (face.Layer > currentLayer)
                    upperFaces.Add(face);
            }
           

            if (foldingLine == null)
                return false;

            // 判断是否有切割新的面
            foreach (Face face in upperFaces)
            {
                // 求线段和面的交点 
                Point3D crossPoint = new Point3D();
                foreach (Edge edge in face.Edges)
                {
                    if (1 == CloverMath.GetIntersectionOfTwoSegments(edge, foldingLine, ref crossPoint))
                        return true;
                }
            }

            return false;  
        }

        /// <summary>
        /// 判断是否有切割其他的边
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        /// <returns></returns>
        public bool JudgeCutAnotherEdge(Face pickedFace, Edge foldingLine)
        {
            bool findVertex1 = false;
            bool findVertex2 = false;
            foreach (Edge e in lastCuttedEdges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    findVertex1 = true; 
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    findVertex2 = true;
            }

            if (findVertex1 && findVertex2)
                return false;
            return true;
        }

        public void RecordCuttedEdges(Face pickedFace, Edge foldingLine)
        {
            // 记录本次切割所切的两条边
            lastCuttedEdges.Clear();
            foreach (Edge e in pickedFace.Edges)
            {
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
                if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    lastCuttedEdges.Add(e);
            }
            return; 
        }

        /// <summary>
        /// 移动所有点到新的位置
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public void MoveToNewPosition(Vertex originVertex, Point3D projectionPoint, Face pickedFace, Edge foldingLine)
        {
            // 和折线共边的那个点
            Vertex currentVertex = vertexLayer.GetVertex(originVertex.Index);
            Vector3D vectorFromLastFoldingLineToOrginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();

            foreach (Face face in foldingSystem.GetLastTimeMovedFace())
            {
                // 移动折线到新的位置
                foreach (Vertex v in face.Vertices)
                {
                    if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
                    {
                        foreach (Edge e in face.Edges)
                        {
                            // 计算原来选中点和折线连线的那条边的向量
                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                                 CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), currentVertex.GetPoint3D()))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                                break;
                            }

                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                                CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), currentVertex.GetPoint3D()))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                                break;
                            }
                        }
                        v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                        // 重新计算纹理坐标
                        foreach (Edge e in pickedFace.Edges)
                        {
                            if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
                            {
                                foldingSystem.CalculateTexcoord(v, e);
                                break;
                            }
                        }
                    }
                    if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                    {
                        foreach (Edge e in face.Edges)
                        {
                            // 计算原来选中点和折线连线的那条边的向量
                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                                 CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), currentVertex.GetPoint3D()))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                                break;
                            }

                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                                CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), currentVertex.GetPoint3D()))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                                break;
                            }
                        }
                        v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                        // 重新计算纹理坐标
                        foreach (Edge e in pickedFace.Edges)
                        {
                            if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
                            {
                                foldingSystem.CalculateTexcoord(v, e);
                                break;
                            }
                        }
                    }

                }
                foreach (Edge e in face.Edges)
                {
                    if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), currentVertex.GetPoint3D()))
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
                        break;
                    }

                    if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), currentVertex.GetPoint3D()))
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex2.GetPoint3D();
                        break;
                    }
                }

                //renderController.Update(face);
                // 求旋转和平移
                // 取得移动点和之前折线点的向量,要求折线点一定是和移动点共边的那个点

                // 求得旋转量,并创建旋转矩阵
                Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
                axis.Normalize();
                axis.Negate();
                double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
                AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
                RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
                rotateTransform.CenterX = projectionPoint.X;
                rotateTransform.CenterY = projectionPoint.Y;
                rotateTransform.CenterZ = projectionPoint.Z;

                // 创建平移矩阵
                Vector3D vectorFromProjToOrigin = projectionPoint - currentVertex.GetPoint3D();
                TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

                // 对于选定面除了折线点，其他点做旋转和平移操作
                foreach (Vertex v in face.Vertices)
                {
                    if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) &&
                        !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex2.GetPoint3D()))
                    {
                        v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                        v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                    }
                }
            }
        }

        /// <summary>
        /// FoldingUp到一个投影点上
        /// </summary>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="originVertex">选中的点</param>
        /// <param name="projectionPoint">投影点</param>
        /// <returns>折线边</returns>
        public Edge FoldingUpToAPoint(List<Face> foldingFaces, Vertex originVertex, Point3D projectionPoint)
        {
            // 本次的折线
            Edge foldingLine = null;

            // 如果是第一次折叠，记录当前的面表
            foreach (Face face in foldingFaces)
            {
                originFaces.Add(face.Clone() as Face);
            }

            int index = 0;
            foreach (Face pickedFace in foldingFaces)
            {
                Face originFace = originFaces[index];
                // 判定FoldingUp的成立条件，若成立则进行FoldingUp，若不成立返回null
                if (!DetermineFoldingUpConditionEstablished(originFace, originVertex, projectionPoint, ref foldingLine))
                    return null;
                if (foldingLine == null)
                    return null;

                // 是否是第一次折叠
                if (firstCut)
                {
                    if (!foldingSystem.FoldingUpToPoint(pickedFace, originVertex, projectionPoint, foldingLine))
                        //折叠没有成功，直接返回
                        return null;
                    // 记录本次切割的边
                    RecordCuttedEdges(pickedFace, foldingLine);
                    lastFoldingLine = foldingLine;
                    firstCut = false;
                }
                else
                { 
                    // 不是第一次折叠
                    // 判断本次是否切割了一个新的面
                    if (JudgeCutAnotherFace(pickedFace, foldingFaces, originVertex, projectionPoint, foldingLine))
                    {
                        //撤消之前的折叠
                        shadowSystem.Undo();
                        shadowSystem.Undo();
                        shadowSystem.CheckUndoTree();

                        if (!foldingSystem.FoldingUpToPoint(pickedFace, originVertex, projectionPoint, foldingLine))
                            //折叠没有成功，直接返回
                            return null;
                        // 记录本次切割所切的两条边
                        RecordCuttedEdges(pickedFace, foldingLine);
                        lastFoldingLine = foldingLine;
                    }
                    else
                    {
                        if (JudgeCutAnotherEdge(pickedFace, foldingLine))
                        {
                            shadowSystem.Undo();
                            shadowSystem.Undo();
                            shadowSystem.CheckUndoTree();
                            if (!foldingSystem.FoldingUpToPoint(pickedFace, originVertex, projectionPoint, foldingLine))
                                //折叠没有成功，直接返回
                                return null;
                            // 记录本次切割所切的两条边
                            RecordCuttedEdges(pickedFace, foldingLine);
                            lastFoldingLine = foldingLine;
                        }
                        else
                        {
                            MoveToNewPosition(originVertex, projectionPoint, pickedFace, foldingLine);
                            lastFoldingLine = foldingLine;
                        }
                    }
                }
                index++;
            }


            //List<Face> movedFaceList = foldingSystem.GetLastTimeMovedFace();

            //foreach (Face face in foldingFaces)
            //{
            //    faceGroupLookupTable.AddFace(face.LeftChild);
            //    faceGroupLookupTable.AddFace(face.RightChild);

            //    faceGroupLookupTable.RemoveFace(face);
            //}

            //// 根据layer排序
            //movedFaceList.Sort(new FaceSort());
            //int maxLayer = movedFaceList[movedFaceList.Count - 1].Layer;

            //for (int i = 0; i < movedFaceList.Count / 2; i++)
            //{
            //    int tempLayer = movedFaceList[i].Layer;
            //    movedFaceList[i].Layer = movedFaceList[movedFaceList.Count - i].Layer;
            //    movedFaceList[movedFaceList.Count - i].Layer = tempLayer;
            //}


            //faceGroupLookupTable.

            
            // 更新重绘
            //foreach (Face f in faceLayer.Leaves)
            //    renderController.Update(f);
            renderController.UpdateAll();

            return foldingLine;
        }
        #endregion

        #region 与TuckingIn相关

        #endregion

        #region 更新
        /// <summary>
        /// 
        /// </summary>
        public void UpdateFaceGroupTable()
        {
            faceGroupLookupTable.UpdateTableAfterFoldUp();
        }

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



        /// <summary>
        /// 通过点找面
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Face> FindFacesByVertex(Vertex vertex)
        {
            return CloverTreeHelper.FindFacesFromVertex(faceLayer.Leaves, vertex);
            //return CloverTreeHelper.GetReferenceFaces(vertex);
        }

        public List<Face> FindFacesByVertex(int index)
        {
            return CloverTreeHelper.FindFacesFromVertex(faceLayer.Leaves, vertexLayer.GetVertex(index));
            //return CloverTreeHelper.GetReferenceFaces(vertexLayer.GetVertex(index));
        }

        public void RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            foldingSystem.RotateFaces(beRotatedFaceList, foldingLine, angle);
            //CloverController.GetInstance().FaceGroupLookupTable.UpdateTableAfterFoldUp();
        }

        /// <summary>
        /// 根据鼠标位移在每个渲染帧前更新结构
        /// </summary>
        /// <param name="xRel">鼠标的x位移</param>
        /// <param name="yRel">鼠标的y位移</param>
        /// <param name="faceList">折叠所受影响的面</param>
        public void Update(float xRel, float yRel, Vertex pickedVertex, Face pickedFace)
        {
            //table.UpdateLookupTable();
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
                if ( foldingSystem.TestMovedFace(face, pickedFace, pickedVertex))
                {
                    if ( foldingSystem.TestFoldingLineCrossed(face, currentFoldingLine))
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
                //CutAFace(face, currentFoldingLine); 
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

            ImageBrush imb = new ImageBrush();
            //imb.ViewportUnits = BrushMappingMode.Absolute;
            imb.ImageSource = new BitmapImage(new Uri(@"media/paper/paper1.jpg", UriKind.Relative));
            DiffuseMaterial mgf = new DiffuseMaterial(imb);
            DiffuseMaterial mgb = new DiffuseMaterial(new SolidColorBrush(Colors.OldLace));
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
            //Edge e = foldingSystem.GetFoldingLineOnAFace(FaceLayer.Leaves[0], new Edge(new Vertex(100, 100, 0), new Vertex(-100, -100, 0)));
            return;
        }
        #endregion
    }
}
