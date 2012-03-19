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
        ModelVisual3D shadowModel = null; /// 纸张的虚像，FoldingUp时候用的
        FoldingUp foldingUp = new FoldingUp();
        Blending blending = new Blending();
        Tucking tucking = new Tucking();
        
        #endregion

        #region get/set

        public Clover.FoldingUp FoldingUp
        {
            get { return foldingUp; }
        }
        public Clover.AbstractLayer.Blending Blending
        {
            get { return blending; }
        }
        public Clover.Tucking Tucking
        {
            get { return tucking; }
        }
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
        public System.Windows.Media.Media3D.ModelVisual3D ShadowModel
        {
            get { return shadowModel; }
            set { shadowModel = value; }
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
            SnapshotNode node = new SnapshotNode(CloverController.GetInstance().FaceLayer.Leaves);

            List<Vertex> movedVertexList = foldingSystem.RotateFaces(beRotatedFaceList, foldingLine, angle);

            // 记录本次移动了的顶点
            node.MovedVertexList = movedVertexList;
            node.OriginEdgeListCount = CloverController.GetInstance().EdgeLayer.Count;
            node.OriginVertexListCount = CloverController.GetInstance().VertexLayer.VertexCellTable.Count;
            node.Type = SnapshotNodeKind.RotateKind;
            shadowSystem.Snapshot(node);
        }

        #region 原CutFaces

        public SnapshotNode SnapshotBeforeCut()
        {
            // 拍快照
            shadowSystem.CheckUndoTree();
            SnapshotNode node = new SnapshotNode(faceLayer.Leaves);
            node.Type = SnapshotNodeKind.CutKind;
            node.OriginVertexListCount = vertexLayer.VertexCellTable.Count;
            node.OriginEdgeListCount = edgeLayer.Count;
            return node;
        }

        public void SnapshotAfterCut(SnapshotNode node, List<Edge> newEdges)
        {
            node.NewEdges = newEdges;
            shadowSystem.Snapshot(node);
        }

        public List<Edge> CutFaces(List<Face> faces, Edge edge)
        {
            SnapshotNode node = SnapshotBeforeCut();
            // 割面
            List<Edge> newEdges = foldingSystem.CutFaces(faces, edge);
            SnapshotAfterCut(node, newEdges);

            return newEdges;
        }

        #endregion

        

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

        //#region 折叠模式

        
        //#endregion


        ///// <summary>
        ///// 更新折线
        ///// </summary>
        ///// <param name="face"></param>
        ///// <param name="pickedPoint"></param>
        ///// <param name="projectionPoint"></param>
        ///// <returns></returns>

        ////public Edge UpdateFoldingLine(Face face, Point3D pickedPoint)
        ////{
        ////    Edge e = UpdateFoldingLine(face, new Vertex(pickedPoint), new Vertex(projectionPoint));
        ////    //currentFoldingLine = e;
        ////    //renderController.AddFoldingLine(e.Vertex1.u, e.Vertex1.v, e.Vertex2.u, e.Vertex2.v);
        ////    return e;
        ////}

        ///// <summary>
        ///// 创建一条折线
        ///// </summary>
        ///// <param name="f"></param>
        ///// <param name="pOriginal"></param>
        ///// <param name="pDestination"></param>
        ///// <returns>返回创建的折线，失败则返回null</returns>
        //public Edge GenerateEdge(Face f, Vertex pOriginal, Vertex pDestination)
        //{
        //    Vector3D t = new Vector3D();
        //    Point3D p0 = new Point3D();
        //    if (!CloverMath.GetMidperpendicularInFace(f, pOriginal, pDestination, ref t, ref p0))
        //        return null;

        //    Point3D p1 = p0 + t * Double.MaxValue;
        //    Point3D p2 = p0 + t * Double.MinValue;
        //    Vertex v1, v2;
        //    v1 = new Vertex(p1);
        //    v2 = new Vertex(p2);

        //    Edge newe = new Edge(v1, v2);
        //    Vertex vresult1 = null;
        //    Vertex vresult2 = null;
        //    foreach (Edge e in f.Edges)
        //    {
        //        if (e.IsVerticeIn(pOriginal))
        //        {
        //            Point3D p = new Point3D();
        //            CloverMath.GetIntersectionOfTwoSegments(newe, e, ref p);
        //            if (vresult1 == null)
        //            {
        //                vresult1 = new Vertex(p);
        //            }
        //            else
        //            {
        //                vresult2 = new Vertex(p);
        //            }
        //        }
        //    }
        //    if (vresult1 == null || vresult2 == null)
        //    {
        //        return null;
        //    }
        //    return new Edge(vresult1, vresult2);
        //}

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
            shadowModel = renderController.Shadow;
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
