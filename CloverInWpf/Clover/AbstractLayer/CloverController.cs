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
            //foreach (Edge e in newEdges)
            //{
            //    renderController.AddFoldingLine(e.Vertex1.u, e.Vertex1.v, e.Vertex2.u, e.Vertex2.v);
            //}
            node.NewEdges = newEdges;
            shadowSystem.Snapshot(node);

            return newEdges;
        }

        public Vertex GetPrevVersion(Vertex vertex)
        {
            List<Vertex> vGroup = vertexLayer.VertexCellTable[vertex.Index];
            if (vGroup.Count < 2)
                return null;
            return vGroup[vGroup.Count - 2];
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
        Face nearestFace;
        Face pickedFace;
        Face originFace;
        Vertex pickedVertex;
        Point3D originPoint;
        Point3D lastProjectionPoint;
        Point3D projectionPoint;
        Edge foldingLine;

        public void EnterFoldingMode(Face nearestFace, Vertex pickedVertex)
        {
            // 修订选中的面为拥有选定点的同层面中最下面的那个面
            List<Face> faceList = CloverController.GetInstance().FaceGroupLookupTable.GetGroup(nearestFace).GetFaceList();

            this.nearestFace = nearestFace;
            pickedFace = nearestFace;
            this.pickedVertex = pickedVertex;

            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
            lastProjectionPoint = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            isFirstCut = true;

            foreach (Face face in faceList)
            {
                if (face.Vertices.Contains(pickedVertex) && face.Layer < pickedFace.Layer)
                    pickedFace = face;
                if (face.Vertices.Contains(pickedVertex) && face.Layer > this.nearestFace.Layer)
                    this.nearestFace = face;
            }

            originFace = pickedFace.Clone() as Face;
        }

        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;
            return FoldingUpToAPoint();
        }

        public void ExitFoldingMode()
        {
            UpdateFaceGroupTable();
        }
        #endregion


        /// <summary>
        /// 更新折线
        /// </summary>
        /// <param name="face"></param>
        /// <param name="pickedPoint"></param>
        /// <param name="projectionPoint"></param>
        /// <returns></returns>

        //public Edge UpdateFoldingLine(Face face, Point3D pickedPoint)
        //{
        //    Edge e = UpdateFoldingLine(face, new Vertex(pickedPoint), new Vertex(projectionPoint));
        //    //currentFoldingLine = e;
        //    //renderController.AddFoldingLine(e.Vertex1.u, e.Vertex1.v, e.Vertex2.u, e.Vertex2.v);
        //    return e;
        //}

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

        #endregion

        #region 与FoldingUp相关

        /// <summary>
        /// FoldingUp到一个投影点上
        /// </summary>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="originVertex">选中的点</param>
        /// <param name="projectionPoint">投影点</param>
        /// <returns>折线边</returns>
        private Edge FoldingUpToAPoint()
        {
            // 判定FoldingUp的成立条件，若成立则进行FoldingUp，若不成立返回null
            if (!DetermineFoldingUpConditionEstablished() || foldingLine == null)
                return null;

            // 是否是第一次折叠
            if (isFirstCut)
            {
                if (!FirstCut())
                    return null;
            }
            else
            {
                // 不是第一次折叠
                // 判断本次是否切割了一个新的面
                if (JudgeCutAnotherFace())
                {
                    if (!UndoLastCutAndDoFolding())
                        return null;
                }
                else
                {
                    if (JudgeCutAnotherEdge())
                    {
                        if (!UndoLastCutAndDoFolding())
                            return null;
                    }
                    else
                    {
                        //MoveToNewPosition();
                        MoveToANewPosition();
                        lastFoldingLine = foldingLine;
                        lastProjectionPoint = projectionPoint;
                    }
                }
            }

            // 更新重绘
            renderController.UpdateAll();

            return foldingLine;
        }

        /// <summary>
        /// 测试FoldingUp成立条件
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="originVertex"></param>
        /// <param name="projectionPoint"></param>
        /// <param name="foldingLine"></param>
        /// <returns></returns>
        private bool DetermineFoldingUpConditionEstablished( )
        {
            // 不成立条件一：投影点和原始点是同一个点
            if (originPoint == projectionPoint)
                return false;

            // 不成立条件二：投隐点和原始点的连线以及折线有穿过与该面不同组的面
            foldingLine = foldingSystem.GetFoldingLine(originFace, originPoint, projectionPoint);

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
        private bool JudgeCutAnotherFace()
        {
            // 取出比选顶点离屏幕最近面层数还要高的面
            List<Face> group = faceGroupLookupTable.GetGroup(nearestFace).GetFaceList();
            List<Face> upperFaces = new List<Face>();
            foreach (Face face in group)
            {
                if (face.Layer > nearestFace.Layer)
                    upperFaces.Add(face);
            }
            if (upperFaces.Count == 0)
                return false;

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
        private bool JudgeCutAnotherEdge( )
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

        private void RecordCuttedEdges( )
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
        private void MoveToNewPosition()
        {
            // 和折线共边的那个点
            Vertex currentVertex = vertexLayer.GetVertex(pickedVertex.Index);
            Vector3D vectorFromLastFoldingLineToOrginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();
            Edge edgeForCalculate = null;
            bool seqForCalculate = true; // 如果是true，则后面为vertex2 减去 vertex 1, 为false则相反

            Face pickedFaceAfterCutting = null;
            List<Face> lastTimeMovedFace = foldingSystem.GetLastTimeMovedFace();
            // 从上次移动的面中找到带有选中点的那个面
            foreach (Face face in lastTimeMovedFace)
            {
                foreach (Vertex v in face.Vertices)
                {
                    if (v == currentVertex)
                    {
                        pickedFaceAfterCutting = face;
                        break;
                    } 
                }
                if (pickedFaceAfterCutting != null)
                    break;
            }

            // 根据选中面计算平移和旋转矩阵
            // 此处需要记录计算向量所用的两个顶点，不然在三角形的时候是没有办法判断的，之前就悲剧的出问题了，囧！！！
            foreach (Vertex v in pickedFaceAfterCutting.Vertices)
            {
                if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
                {
                    foreach (Edge e in pickedFaceAfterCutting.Edges)
                    {
                        // 计算原来选中点和折线连线的那条边的向量
                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                             CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = true;
                            break;
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = false;
                            break;
                        }
                    }
                    v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                    v.Moved = true;
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
                    foreach (Edge e in pickedFaceAfterCutting.Edges)
                    {
                        // 计算原来选中点和折线连线的那条边的向量
                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
                             CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = true;
                            break;
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
                            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
                        {
                            vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
                            edgeForCalculate = e;
                            seqForCalculate = false;
                            break;
                        }
                    }
                    v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                    v.Moved = true;
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
            
            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (e == edgeForCalculate)
                {
                    if (seqForCalculate)
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
                    }
                    else
                    {
                        vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex2.GetPoint3D(); 
                    }
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

            // 选定面的移动
            foreach (Vertex v in pickedFaceAfterCutting.Vertices)
            {
                if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) &&
                    !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex2.GetPoint3D()))
                {
                    if (v.Moved == false)
                    {
                        v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                        v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                        v.Moved = true;
                    }
                }
            }

            // 对于上次移动的面除了折线点，其他点做旋转和平移操作
            foreach (Face face in lastTimeMovedFace)
            {
                if (face != pickedFaceAfterCutting)
                {
                    foreach (Vertex v in face.Vertices)
                    {
                        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
                        {
                            if (!v.Moved)
                            { 
                                // 对于折线点要计算新的纹理坐标
                                v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                                foreach (Edge edge in face.Edges)
                                {
                                    if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.0001))
                                        foldingSystem.CalculateTexcoord(v, edge);
                                }
                                v.Moved = true;
                            }
                        }

                        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                        {
                            if (!v.Moved)
                            { 
                                v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                                foreach (Edge edge in face.Edges)
                                {
                                    if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), edge.Vertex1.GetPoint3D(), edge.Vertex2.GetPoint3D(), 0.0001))
                                        foldingSystem.CalculateTexcoord(v, edge);
                                }
                                v.Moved = true;
                            }
                        }
                       
                        if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()) &&
                            !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
                        {
                            if (v.Moved == false)
                            {
                                v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                                v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                                v.Moved = true;
                            }
                        }
                    }
                }
            }

            foreach (Vertex v in vertexLayer.Vertices)
            {
                v.Moved = false; 
            }
            //foreach (Face face in foldingSystem.GetLastTimeMovedFace())
            //{
            //    // 移动折线到新的位置
            //    foreach (Vertex v in face.Vertices)
            //    {
            //        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex1.GetPoint3D()))
            //        {
            //            foreach (Edge e in face.Edges)
            //            {
            //                // 计算原来选中点和折线连线的那条边的向量
            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
            //                     CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
            //                    break;
            //                }

            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
            //                    CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
            //                    break;
            //                }
            //            }
            //            v.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
            //            // 重新计算纹理坐标
            //            foreach (Edge e in pickedFace.Edges)
            //            {
            //                if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
            //                {
            //                    foldingSystem.CalculateTexcoord(v, e);
            //                    break;
            //                }
            //            }
            //        }
            //        if (CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), lastFoldingLine.Vertex2.GetPoint3D()))
            //        {
            //            foreach (Edge e in face.Edges)
            //            {
            //                // 计算原来选中点和折线连线的那条边的向量
            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), v.GetPoint3D()) &&
            //                     CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex2.GetPoint3D() - e.Vertex1.GetPoint3D();
            //                    break;
            //                }

            //                if (CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), v.GetPoint3D()) &&
            //                    CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //                {
            //                    vectorFromLastFoldingLineToOrginVertex = e.Vertex1.GetPoint3D() - e.Vertex2.GetPoint3D();
            //                    break;
            //                }
            //            }
            //            v.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
            //            // 重新计算纹理坐标
            //            foreach (Edge e in pickedFace.Edges)
            //            {
            //                if (CloverMath.IsPointInTwoPoints(v.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D(), 0.0001))
            //                {
            //                    foldingSystem.CalculateTexcoord(v, e);
            //                    break;
            //                }
            //            }
            //        }

            //    }
            //    foreach (Edge e in face.Edges)
            //    {
            //        if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), lastProjectionPoint))
            //        {
            //            vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex1.GetPoint3D();
            //            break;
            //        }

            //        if ((CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) ||
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex2.GetPoint3D(), foldingLine.Vertex2.GetPoint3D())) &&
            //            CloverMath.AreTwoPointsSameWithDeviation(e.Vertex1.GetPoint3D(), lastProjectionPoint))
            //        {
            //            vectorFromCurrentFoldingLineToProjVertex = projectionPoint - e.Vertex2.GetPoint3D();
            //            break;
            //        }
            //    }

            //    //renderController.Update(face);
            //    // 求旋转和平移
            //    // 取得移动点和之前折线点的向量,要求折线点一定是和移动点共边的那个点

            //    // 求得旋转量,并创建旋转矩阵
            //    Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            //    axis.Normalize();
            //    axis.Negate();
            //    double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOrginVertex);
            //    AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
            //    RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
            //    rotateTransform.CenterX = projectionPoint.X;
            //    rotateTransform.CenterY = projectionPoint.Y;
            //    rotateTransform.CenterZ = projectionPoint.Z;

            //    // 创建平移矩阵
            //    Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
            //    TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

            //    // 对于选定面除了折线点，其他点做旋转和平移操作
            //    foreach (Vertex v in face.Vertices)
            //    {
            //        if (!CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex1.GetPoint3D()) &&
            //            !CloverMath.AreTwoPointsSameWithDeviation(v.GetPoint3D(), foldingLine.Vertex2.GetPoint3D()))
            //        {
            //            v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
            //            v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
            //        }
            //    }
            //}
        }

        private bool MoveToANewPosition()
        { 
            // 用于计算矩阵的两个关键向量
            Vector3D vectorFromLastFoldingLineToOriginVertex = new Vector3D();
            Vector3D vectorFromCurrentFoldingLineToProjVertex = new Vector3D();

            // 更新当前选中点到最新的状态
            Vertex currentVertex = vertexLayer.GetVertex(pickedVertex.Index);

            // 从上次移动的面中找到带有选中点的那个面
            Face pickedFaceAfterCutting = null;
            List<Face> lastTimeMovedFace = foldingSystem.GetLastTimeMovedFace();

            // 从上次移动的面中找到带有选中点的那个面
            foreach (Face face in lastTimeMovedFace)
            {
               foreach (Vertex v in face.Vertices)
                {
                    if (v == currentVertex)
                    {
                        if (pickedFaceAfterCutting == null || pickedFaceAfterCutting.Layer > face.Layer)
                            pickedFaceAfterCutting = face;
                        break;
                    }
                }
            }

            if (pickedFaceAfterCutting == null)
                return false;

            // 在选定的面上找到与折线边顶点位置相等的那条边
            Edge foldingLineOnFace = null;
            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (CloverMath.AreTwoEdgesEqual(e, lastFoldingLine))
                    foldingLineOnFace = e;
            }

            if (null == foldingLineOnFace)
                return false;

            // 在找到选顶点与折线的一条连线边，计算出计算角度所需的向量1
            Edge edgeBeforeMoving = null;
            foreach (Edge e in pickedFaceAfterCutting.Edges)
            {
                if (e.Vertex1 == currentVertex && e.Vertex2 == foldingLineOnFace.Vertex1)
                { 
                    edgeBeforeMoving = e;
                    break;
                }
                if (e.Vertex2 == currentVertex && e.Vertex1 == foldingLineOnFace.Vertex1)
                { 
                    edgeBeforeMoving = e;
                    break;
                }
                if (e.Vertex1 == currentVertex && e.Vertex2 == foldingLineOnFace.Vertex2)
                { 
                    edgeBeforeMoving = e;
                    break;                
                }
                if (e.Vertex2 == currentVertex && e.Vertex1 == foldingLineOnFace.Vertex2)
                { 
                    edgeBeforeMoving = e;
                    break;                
                }
            }

            // 得到关键向量1
            vectorFromLastFoldingLineToOriginVertex = edgeBeforeMoving.Vertex2.GetPoint3D() - edgeBeforeMoving.Vertex1.GetPoint3D();

            // 在原始面中找到折线顶点所在的两条边，并用这两条边计算折线的纹理坐标
            foreach (Edge e in pickedFaceAfterCutting.Parent.Edges)
            {
                // 判断折线的两个顶点，更新坐标并计算纹理
                // 顶点1
                if (CloverMath.IsPointInTwoPoints(foldingLineOnFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                {
                    foldingSystem.CalculateTexcoord(foldingLineOnFace.Vertex1, e);
                    if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    {
                        foldingLineOnFace.Vertex1.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                        foldingLineOnFace.Vertex1.Moved = true;
                    }
                    else
                    {
                        foldingLineOnFace.Vertex1.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                        foldingLineOnFace.Vertex1.Moved = true;
                    }
                }

                // 顶点2
                if (CloverMath.IsPointInTwoPoints(foldingLineOnFace.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                {
                    foldingSystem.CalculateTexcoord(foldingLineOnFace.Vertex2, e);
                    if (CloverMath.IsPointInTwoPoints(foldingLine.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                    {
                        foldingLineOnFace.Vertex2.SetPoint3D(foldingLine.Vertex1.GetPoint3D());
                        foldingLineOnFace.Vertex2.Moved = true;
                    }
                    else
                    {
                        foldingLineOnFace.Vertex2.SetPoint3D(foldingLine.Vertex2.GetPoint3D());
                        foldingLineOnFace.Vertex2.Moved = true;
                    }
                }
            }
            // 设置选中点的新位置为projectinPoint;
            currentVertex.SetPoint3D(projectionPoint);
            currentVertex.Moved = true;

            // 找到选中点和折线之间的向量,得到关键向量2
            vectorFromCurrentFoldingLineToProjVertex = edgeBeforeMoving.Vertex2.GetPoint3D() - edgeBeforeMoving.Vertex1.GetPoint3D();

            // 计算所有上次移动面的折线，对于折线完全相等的直接更新到新的位置，对于不相等的要重新找打对应的边，然后计算折线位置，再更新折线
            foreach(Face face in lastTimeMovedFace)
            {
                if (face == pickedFaceAfterCutting)
                    continue;

                Edge foldingLineForThisFace = null;
                foreach (Edge e in face.Edges)
                { 
                    // 找到与原来折线共线的那条边
                    Point3D crossedPoint = new Point3D();
                    if (2 == CloverMath.GetIntersectionOfTwoSegments(lastFoldingLine, e, ref crossedPoint))
                    { 
                        foldingLineForThisFace = e;
                        break;
                    }
                }

                // 假如木有折线，或者折线中的其中一个点和上面的折线拥有相同点返回
                if (foldingLineForThisFace == null)
                { 
                    // 采用点更新法 
                    foreach (Edge e in face.Edges)
                    { 
                         // 假如其中有一条边的一个顶点等于新折线，而另一个顶点等于旧折线中的点，折更新旧点到新点
                    }
                    continue;
                }

                // 计算现在的折线位置
                Edge currentFoldingLineForThisFace = foldingSystem.GetFoldingLineOnAFace(face, foldingLine);
                if (currentFoldingLineForThisFace == null)
                    continue;

                // 通过该面的原始面查询折线并更新折线位置和纹理坐标
                foreach (Edge e in face.Parent.Edges)
                {
                    // 判断折线的两个顶点，更新坐标并计算纹理
                    // 顶点1
                    if (CloverMath.IsPointInTwoPoints(foldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()) &&
                        !foldingLineForThisFace.Vertex1.Moved)
                    {
                        foldingSystem.CalculateTexcoord(foldingLineForThisFace.Vertex1, e);
                        if (CloverMath.IsPointInTwoPoints(currentFoldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                        {
                            foldingLineOnFace.Vertex1.SetPoint3D(currentFoldingLineForThisFace.Vertex1.GetPoint3D());
                            foldingLineOnFace.Vertex1.Moved = true;
                        }
                        else
                        {
                            foldingLineOnFace.Vertex1.SetPoint3D(currentFoldingLineForThisFace.Vertex2.GetPoint3D());
                            foldingLineOnFace.Vertex1.Moved = true;
                        }
                    }

                    // 顶点2
                    if (CloverMath.IsPointInTwoPoints(foldingLineForThisFace.Vertex2.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()) &&
                        !foldingLineForThisFace.Vertex2.Moved)
                    {
                        foldingSystem.CalculateTexcoord(foldingLineForThisFace.Vertex2, e);
                        if (CloverMath.IsPointInTwoPoints(currentFoldingLineForThisFace.Vertex1.GetPoint3D(), e.Vertex1.GetPoint3D(), e.Vertex2.GetPoint3D()))
                        {
                            foldingLineOnFace.Vertex2.SetPoint3D(currentFoldingLineForThisFace.Vertex1.GetPoint3D());
                            foldingLineOnFace.Vertex2.Moved = true;
                        }
                        else
                        {
                            foldingLineOnFace.Vertex2.SetPoint3D(currentFoldingLineForThisFace.Vertex2.GetPoint3D());
                            foldingLineOnFace.Vertex2.Moved = true;
                        }
                    }
                }
            }

            // 求得旋转量,并创建旋转矩阵
            Vector3D axis = Vector3D.CrossProduct(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOriginVertex);
            axis.Normalize();
            axis.Negate();
            double angle = Vector3D.AngleBetween(vectorFromCurrentFoldingLineToProjVertex, vectorFromLastFoldingLineToOriginVertex);
            AxisAngleRotation3D asixRotation = new AxisAngleRotation3D(axis, angle);
            RotateTransform3D rotateTransform = new RotateTransform3D(asixRotation);
            rotateTransform.CenterX = projectionPoint.X;
            rotateTransform.CenterY = projectionPoint.Y;
            rotateTransform.CenterZ = projectionPoint.Z;

            // 创建平移矩阵
            Vector3D vectorFromProjToOrigin = projectionPoint - lastProjectionPoint;
            TranslateTransform3D translateTransform = new TranslateTransform3D(vectorFromProjToOrigin);

            // 移动所有没有移动过的顶点
            foreach (Face face in lastTimeMovedFace)
            {
                foreach (Vertex v in face.Vertices)
                {
                    if (!v.Moved)
                    {
                        v.SetPoint3D(translateTransform.Transform(v.GetPoint3D()));
                        v.SetPoint3D(rotateTransform.Transform(v.GetPoint3D()));
                        v.Moved = true;
                    }
                }
            }

            // 最终更改所有顶点移动属性复位
            foreach (Face face in lastTimeMovedFace)
            {
                foreach (Vertex v in face.Vertices)
                {
                    v.Moved = false;
                }
            }

            return true;
        }


        private bool UndoLastCutAndDoFolding()
        {
            //撤消之前的折叠
            shadowSystem.Undo();
            shadowSystem.Undo();
            shadowSystem.CheckUndoTree();

            if (!foldingSystem.ProcessFoldingUp(pickedFace, pickedVertex, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割所切的两条边
            RecordCuttedEdges();
            lastFoldingLine = foldingLine;
            lastProjectionPoint = projectionPoint;
            return true;
        }
        private bool FirstCut()
        {
            if (!foldingSystem.ProcessFoldingUp(pickedFace, pickedVertex, originPoint, projectionPoint, foldingLine))
                //折叠没有成功，直接返回
                return false;
            // 记录本次切割的边
            RecordCuttedEdges();
            lastFoldingLine = foldingLine;
            isFirstCut = false;
            lastProjectionPoint = projectionPoint;
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
