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
        ModelVisual3D model = null; /// 纸张的模型

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
        public System.Windows.Media.Media3D.ModelVisual3D Model
        {
            get { return model; }
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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow"></param>
        private CloverController(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            renderController = RenderController.GetInstance();
        }
        #endregion

        #region 文件操作
        public void SaveFile(string filename)
        {
            fileWriter.SaveFile(filename, faceLayer, edgeLayer, vertexLayer, shadowSystem);
        }
        public void LoadFile(string filename)
        {
            fileLoader.LoadFromFile(filename);

            // 重置所有的数据结构
            faceLayer = fileLoader.FaceLayer;
            edgeLayer = fileLoader.EdgeLayer;
            vertexLayer = fileLoader.VertexLayer;
            shadowSystem = fileLoader.ShadowSystem;

            renderController.DeleteAll();
            //renderController.RedrawFoldLine();
            foreach (Face face in faceLayer.Leaves)
            {
                renderController.New(face);
            }
        }
        #endregion

        #region 折叠
        public void RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            List<Vertex> movedVertexList = foldingSystem.RotateFaces(beRotatedFaceList, foldingLine, angle);

            SnapshotNode node = new SnapshotNode(CloverController.GetInstance().FaceLayer.Leaves);
            node.MovedVertexList = movedVertexList;
            node.OriginEdgeListCount = CloverController.GetInstance().EdgeLayer.Count;
            node.OriginVertexListCount = CloverController.GetInstance().VertexLayer.VertexCellTable.Count;
            node.Type = SnapshotNodeKind.RotateKind;
            shadowSystem.Snapshot(node);
        }


        public List<Edge> CutFaces(List<Face> faces, Edge edge)
        {
            // 拍快照
            CloverController controller = CloverController.GetInstance();
            ShadowSystem shadowSystem = controller.ShadowSystem;
            shadowSystem.CheckUndoTree();

            SnapshotNode node = new SnapshotNode(controller.FaceLayer.Leaves);
            node.Type = SnapshotNodeKind.CutKind;
            node.OriginVertexListCount = controller.VertexLayer.VertexCellTable.Count;
            node.OriginEdgeListCount = controller.EdgeLayer.Count;

            // 割面
            List<Edge> newEdges = foldingSystem.CutFaces(faces, edge);
            foreach (Edge e in newEdges)
            {
                renderController.AddFoldingLine(e.Vertex1.u, e.Vertex1.v, e.Vertex2.u, e.Vertex2.v);
            }
            node.NewEdges = newEdges;
            shadowSystem.Snapshot(node);

            return newEdges;
        }
        #endregion

        #region 动画
        delegate List<Vertex> RotateFacesHandle(List<Face> list, Edge foldLine, double angle);
        delegate List<Edge> CutFacesHandle(List<Face> list, Edge foldLine);

        System.Diagnostics.Stopwatch animaWatch = new System.Diagnostics.Stopwatch();
        long animationDuration = 1000;
        public void AnimatedCutFaces(List<Face> beCutFaceList, Edge edge)
        {
            List<Face> faceList = new List<Face>();
            faceList.AddRange(beCutFaceList);

            mainWindow.Dispatcher.Invoke(new CutFacesHandle(CutFaces), faceList, edge);
        }

        public void AnimatedRotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            List<Face> faceList = new List<Face>();
            faceList.AddRange(beRotatedFaceList);
            List<Vertex> movedVertexList = null;

            AutoCamera(beRotatedFaceList[0]);

            // 控制时间
            animaWatch.Restart();
            long lastTime = 0;
            long thisTime = 0;
            double interval = angle / animationDuration; //0.5秒
            while (thisTime <= animationDuration)
            {
                thisTime = animaWatch.ElapsedMilliseconds;
                double step;
                if (thisTime <= animationDuration)
                    step = interval * (thisTime - lastTime);
                else
                    step = interval * (animationDuration - lastTime);
                List<Vertex> list = mainWindow.Dispatcher.Invoke(
                    new RotateFacesHandle(foldingSystem.RotateFaces),
                     faceList, foldingLine, step) as List<Vertex>;

                // 取第一次的移动的顶点列表
                if (movedVertexList == null)
                {
                    movedVertexList = list;
                }

                lastTime = thisTime;
            }
            animaWatch.Stop();

            // 快照
            SnapshotNode node = new SnapshotNode(CloverController.GetInstance().FaceLayer.Leaves);
            node.MovedVertexList = movedVertexList;
            node.OriginEdgeListCount = CloverController.GetInstance().EdgeLayer.Count;
            node.OriginVertexListCount = CloverController.GetInstance().VertexLayer.VertexCellTable.Count;
            node.Type = SnapshotNodeKind.RotateKind;
            shadowSystem.Snapshot(node);
        }

        /// <summary>
        /// 自动在折纸过程中旋转摄像机
        /// </summary>
        /// <param name="beRotatedFaceList"></param>
        private void AutoCamera(Face face)
        {
            // 自动旋转摄像头
            Vector3D vector1 = face.Normal;
            Vector3D vector2 = new Vector3D(0, 0, 1);
            Quaternion quat;
            if (vector1 == new Vector3D(0, 0, 1))
                quat = new Quaternion();
            else if (vector1 == new Vector3D(0, 0, -1))
                quat = new Quaternion(new Vector3D(0, 1, 0), 180);
            else
            {
                Vector3D axis = Vector3D.CrossProduct(vector1, vector2);
                axis.Normalize();
                Double deg = Vector3D.AngleBetween(vector1, vector2);
                quat = new Quaternion(axis, deg);
            }
            renderController.BeginRotationSlerp(quat);
        }
        #endregion

        #region 折叠模式

        List<Face> foldingFaces = new List<Face>();
        List<Face> movingFaces = new List<Face>();
        Edge lastFoldingLine = null;
        List<Edge> lastCuttedEdges = new List<Edge>();
        List<Face> originFaces = new List<Face>();

        bool isFirstCut = true;
        Face pickedFace;
        Point3D originPoint;
        Point3D lastProjectionPoint;
        public void EnterFoldingMode(Face nearestFace, Vertex pickedVertex)
        {
            // 修订选中的面为拥有选定点的同层面中最下面的那个面
            List<Face> faceList = CloverController.GetInstance().FaceGroupLookupTable.GetGroup(nearestFace).GetFaceList();

            pickedFace = nearestFace;

            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
            lastProjectionPoint = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            isFirstCut = true;

            foreach (Face face in faceList)
            {
                if (face.Vertices.Contains(pickedVertex) && face.Layer < nearestFace.Layer)
                    pickedFace = face;
            }
        }

        public void ExitFoldingMode()
        {

        }
        #endregion

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
            if (!CloverMath.GetMidperpendicularInFace(f, pOriginal, pDestination, ref t, ref p0))
                return null;

            Point3D p1 = p0 + t * Double.MaxValue;
            Point3D p2 = p0 + t * Double.MinValue;
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
                    CloverMath.GetIntersectionOfTwoSegments(newe, e, ref p);
                    if (vresult1 == null)
                    {
                        vresult1 = new Vertex(p);
                    }
                    else
                    {
                        vresult2 = new Vertex(p);
                    }
                }
            }
            if (vresult1 == null || vresult2 == null)
            {
                return null;
            }
            return new Edge(vresult1, vresult2);
        }

        #region 初始化

        /// <summary>
        /// 根据给定的长和宽初始化纸张
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Initialize(float width, float height)
        {
            faceLayer = new FaceLayer();
            edgeLayer = new EdgeLayer();
            vertexLayer = new VertexLayer();

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

                renderController.AddVisualInfoToVertex(v);
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

            // 调用渲染层，更新纸张
            CreatePaper(face);
        }

        /// <summary>
        /// 在渲染层创建纸张的模型
        /// </summary>
        /// <param name="face">初始的face</param>
        public void CreatePaper(Face face)
        {
            if (model != null)
            {
                renderController.DeleteAll();
            }

            // 初始化纹理
            ImageBrush imb = new ImageBrush();
            imb.ImageSource = new BitmapImage(new Uri(@"media/paper/paper1.jpg", UriKind.Relative));
            DiffuseMaterial mgf = new DiffuseMaterial(imb);
            DiffuseMaterial mgb = new DiffuseMaterial(new SolidColorBrush(Colors.OldLace));
            renderController.FrontMaterial = mgf;
            renderController.BackMaterial = mgb;

            // 初始化模型
            renderController.New(face);
            model = renderController.Entity;
        }

        #endregion

        #region 测试折叠



        public void UpdateVertexPosition(Vertex vertex, double xOffset, double yOffset)
        {
            vertex = vertexLayer.GetVertex(3);
            vertex.X += xOffset;
            vertex.Y += yOffset;

            renderController.Update(faceLayer.Leaves[1]);
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
            Edge foldingLine = CloverMath.GetPerpendicularBisector3D(face, originVertex.GetPoint3D(), ProjectionVertex.GetPoint3D());
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

        // set/get
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
        public bool DetermineFoldingUpConditionEstablished( Point3D projectionPoint, ref Edge foldingLine)
        {
            // 不成立条件一：投影点和原始点是同一个点
            if (originPoint == projectionPoint)
                return false;

            // 不成立条件二：投隐点和原始点的连线以及折线有穿过与该面不同组的面
            foldingLine = foldingSystem.GetFoldingLine(pickedFace, originPoint, projectionPoint);

            if (foldingLine == null)
                return false;

            //找到所有与该面不同组的面
            List<Face> facesInDifferentGroup = faceGroupLookupTable.GetFaceExcludeGroupFoundByFace(pickedFace);
            foreach (Face face in facesInDifferentGroup)
            {
                // 求线段和面的交点 
                Point3D crossPoint = new Point3D();
                if (CloverMath.IntersectionOfLineAndFace(originPoint, projectionPoint, face, ref crossPoint))
                {
                    if (CloverMath.IsPointInTwoPoints(crossPoint, originPoint, projectionPoint, 0.0001))
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
        public bool JudgeCutAnotherFace( List<Face> foldingFaces, Point3D projectionPoint, Edge foldingLine)
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
        public bool JudgeCutAnotherEdge( Edge foldingLine)
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

        public void RecordCuttedEdges( Edge foldingLine)
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
        public void MoveToNewPosition(Point3D projectionPoint,  Edge foldingLine)
        {
            // 和折线共边的那个点
            // Vertex currentVertex = vertexLayer.GetVertex(originVertex.Index);
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
                                 CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                                break;
                            }

                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                                CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
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
                                 CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                            {
                                vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                                break;
                            }

                            if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                                CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
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
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
                        break;
                    }

                    if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
                        CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
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
                Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
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
        public Edge FoldingUpToAPoint(Point3D projectionPoint)
        {
            // 本次的折线
            Edge foldingLine = null;

            // 判定FoldingUp的成立条件，若成立则进行FoldingUp，若不成立返回null
            if (!DetermineFoldingUpConditionEstablished(projectionPoint, ref foldingLine) || foldingLine == null)
                return null;

            // 是否是第一次折叠
            if (isFirstCut)
            {
                if (!FirstCut(projectionPoint, foldingLine))
                    return null;
            }
            else
            {
                // 不是第一次折叠
                // 判断本次是否切割了一个新的面
                if (JudgeCutAnotherFace(foldingFaces, projectionPoint, foldingLine))
                {
                    if (!UndoLastCutAndDoFolding( projectionPoint, foldingLine))
                        return null;
                }
                else
                {
                    if (JudgeCutAnotherEdge(foldingLine))
                    {
                        if (!UndoLastCutAndDoFolding(projectionPoint, foldingLine))
                            return null;
                    }
                    else
                    {
                        MoveToNewPosition( projectionPoint, foldingLine);
                        lastFoldingLine = foldingLine;
                    }
                }
            }




            // 更新重绘
            renderController.UpdateAll();

            return foldingLine;
        }

        private bool UndoLastCutAndDoFolding(Point3D projectionPoint, Edge foldingLine)
        {
            //撤消之前的折叠
            shadowSystem.Undo();
            shadowSystem.Undo();
            shadowSystem.CheckUndoTree();

            if (!foldingSystem.FoldingUpToPoint(pickedFace, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割所切的两条边
            RecordCuttedEdges( foldingLine);
            lastFoldingLine = foldingLine;

            return true;
        }
        private bool FirstCut(Point3D projectionPoint, Edge foldingLine)
        {
            if (!foldingSystem.FoldingUpToPoint(pickedFace, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割的边
            RecordCuttedEdges( foldingLine);
            lastFoldingLine = foldingLine;
            isFirstCut = false;

            return true;
        }
        private void SaveOriginFaces(List<Face> foldingFaces)
        {
            // 如果是第一次折叠，记录当前的面表
            foreach (Face face in foldingFaces)
            {
                originFaces.Add(face.Clone() as Face);
            }
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

        /// <summary>
        /// 通过点找面
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public List<Face> FindFacesByVertex(Vertex vertex)
        {
            return CloverTreeHelper.FindFacesFromVertex(faceLayer.Leaves, vertex);
        }

        public List<Face> FindFacesByVertex(int index)
        {
            return CloverTreeHelper.FindFacesFromVertex(faceLayer.Leaves, vertexLayer.GetVertex(index));
        }

        #endregion

    }
}
