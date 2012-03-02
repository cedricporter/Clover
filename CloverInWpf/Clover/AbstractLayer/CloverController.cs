﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Clover.RenderLayer;

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
        public RenderController RenderController
        {
            get { return renderController; }
            //set { renderController = value; }
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
            vertices[1] = new Vertex(width / 2, height / 2, 0);
            vertices[2] = new Vertex(width / 2, -height / 2, 0);
            vertices[3] = new Vertex(-width / 2, -height / 2, 0);
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
            Face face = new Face();

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
        }

        private CloverController(MainWindow mainWindow)
        {
            faceLayer = new FaceLayer(this);
            edgeLayer = new EdgeLayer(this);
            vertexLayer = new VertexLayer(this);
            this.mainWindow = mainWindow;
            renderController = new Clover.RenderLayer.RenderController(mainWindow);
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
            Face f1 = new Face();
            Face f2 = new Face();

            face.LeftChild = f1;
            face.RightChild = f2;

            Vertex v1 = edge.Vertex1;
            Vertex v2 = edge.Vertex2;

            int index1 = -1, index2 = -1;
            for ( int i = 0; i < face.Edges.Count; i++)
            {
                if (face.Edges[i].IsVerticeIn(v1))
                    index1 = i;
                if (face.Edges[i].IsVerticeIn(v2))
                    index2 = i;
            }

            Debug.Assert(index1 != -1 && index2 != -1);



        }

        /// <summary>
        /// 当前都影响的面，在拖动的过程中需要实时计算，因为随时会有新的受影响
        /// 的产生或者老的受影响的面被移除。
        /// </summary>
        List<Face> affectedFaceList = new List<Face>();

        int originLastVertexIndex = -1;    /// 顶点列表的最后一个下标

        /// <summary>
        /// 进入折叠模式前的叶子节点表，用于恢复
        /// </summary>
        List<Face> originFaceList = new List<Face>();

        Edge currentFoldingLine = new Edge(new Vertex(), new Vertex());

    	public Edge CurrentFoldingLine
    	{
    		get { return currentFoldingLine; }
    	}

        void UpdateAffectedFaceList()
        {

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
        void StartFoldingModel(List<Face> faces)
        {
            originFaceList.Clear();
            foreach ( Face f in faceLayer.Leaves)
            {
                originFaceList.Add(f);
            }

            
            // 
            Edge foldingLine = null;

            foreach (Face face in faces)
            {
                CutAFace(face, foldingLine);         // 分割面
                faceLayer.UpdateLeaves(face);   // 更新叶节点，局部更新
            }




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
        /// 根据鼠标位移更新折线
        /// </summary>
        /// <param name="xRel"></param>
        /// <param name="yRel"></param>
        void CalculateFoldingLine(float xRel, float yRel)
        {
            
        }

        /// <summary>
        /// 判定是否有新添或者删除数据结构中的信息
        /// </summary>

        bool TestMovedFace(Face face, Face PickedFace, Point3D pickedVertex)
        {
            return true;
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
            u.X = currentFoldingLine.Vertex1.X - currentFoldingLine.Vertex2.X;
            u.Y = currentFoldingLine.Vertex1.Y - currentFoldingLine.Vertex2.Y;
            u.Z = currentFoldingLine.Vertex1.Z - currentFoldingLine.Vertex2.Z;

            // 判定面中的每条边与折线是否相交，若有两条相交则折线分割该平面
            int crossCount = 0;
            foreach (Edge edge in face.Edges)
            {
                Vector3D v = new Vector3D();
                v.X = edge.Vertex1.X - edge.Vertex2.Y;
                v.Y = edge.Vertex1.Y - edge.Vertex2.Y;
                v.Z = edge.Vertex1.Z - edge.Vertex2.Z;

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
        public void Update(float xRel, float yRel, Point3D pickedVertex, Face pickedFace)
        {
            // 计算初始折线
            CalculateFoldingLine(xRel, yRel);

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

            MaterialGroup mgf = new MaterialGroup();
            mgf.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            ImageBrush imb = new ImageBrush();
            imb.ImageSource = new BitmapImage(new Uri(@"media/paper/paper1.jpg", UriKind.Relative));
            mgf.Children.Add(new EmissiveMaterial(imb));
            MaterialGroup mgb = new MaterialGroup();
            mgb.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            mgb.Children.Add(new EmissiveMaterial(new SolidColorBrush(Colors.OldLace)));
            //mg.Children.Add(new EmissiveMaterial(new SolidColorBrush(Colors.Red)));
            //Material material = new EmissiveMaterial(new SolidColorBrush(Colors.Yellow));
            renderController.FrontMaterial = mgf;
            renderController.BackMaterial = mgb;

            faceLayer.UpdateLeaves();
            foreach (Face face in faceLayer.Leaves)
            {
                face.UpdateVertices();
                renderController.New(face);
            }
            model = renderController.Entity;
           
            return model;
        }
        #endregion
    }
}
