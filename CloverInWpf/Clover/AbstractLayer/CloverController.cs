using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Clover
{
    public class CloverController
    {
        #region 成员变量
        FaceLayer faceLayer;    /// 面层
        EdgeLayer edgeLayer;    /// 边层
        VertexLayer vertexLayer;/// 点层
        MainWindow mainWindow;  /// 你懂得
        Clover.RenderLayer.RenderController renderController;///渲染层
        #endregion

        #region get/set
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

        public CloverController(MainWindow mainWindow)
        {
            faceLayer = new FaceLayer(this);
            edgeLayer = new EdgeLayer(this);
            vertexLayer = new VertexLayer(this);
            this.mainWindow = mainWindow;
            renderController = new Clover.RenderLayer.RenderController(mainWindow);
            //paper = new Paper("paper");
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

        Edge currentFoldingLine = new Edge(null, null);
    	public Edge CurrentFoldingLine
    	{
    		get { return currentFoldingLine; }
    	}

        float currentAngel;
        Point3D currentVertex;
        
        List<Edge> shadowEdges = new List<Edge>();
        List<Vertex> shadowVertice = new List<Vertex>();
        List<Face> shadowFaces = new List<Face>();


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

        bool TestFoldingLineCrossed(Face face, Edge currentFoldingLine)
        {

            return true;
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
