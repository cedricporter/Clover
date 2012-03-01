using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Clover
{
    public class CloverController
    {
        FaceLayer faceLayer;    /// 面层
        EdgeLayer edgeLayer;    /// 边层
        VertexLayer vertexLayer;/// 点层
        //Paper paper;            /// 纸张实体，ogre的实体，用于画图

        #region get/set
        //public Clover.Paper Paper
        //{
        //    get { return paper; }
        //}
        #endregion

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

        public void Initialize(float width, float height)
        {
            // Create 4 original vertices
            Vertex[] vertices = new Vertex[4];
            vertices[0] = new Vertex(-width / 2, height / 2, 0);
            vertices[1] = new Vertex(width / 2, height / 2, 0);
            vertices[2] = new Vertex(width / 2, -height / 2, 0);
            vertices[3] = new Vertex(-width / 2, -height / 2, 0);

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

        public CloverController()
        {
            faceLayer = new FaceLayer(this);
            edgeLayer = new EdgeLayer(this);
            vertexLayer = new VertexLayer(this);

            //paper = new Paper("paper");
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
            foreach(Face face in faceLayer.Leaves)
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

        ModelVisual3D model = new ModelVisual3D();
        public System.Windows.Media.Media3D.ModelVisual3D Model
        {
            get { return model; }
            set { model = value; }
        }
        public ModelVisual3D UpdatePaper()
        {
            faceLayer.UpdateLeaves();
            //paper.Begin("BaseWhiteNoLight", Mogre.RenderOperation.OperationTypes.OT_TRIANGLE_FAN);



            MeshGeometry3D triangleMesh = new MeshGeometry3D();

            foreach (Vertex v in vertexLayer.Vertices)
            {
                triangleMesh.Positions.Add(new Point3D(v.point.X, v.point.Y, v.point.Z));
            }
             

            foreach (Face face in faceLayer.Leaves)
            {
                face.UpdateVertices();
                for (int i = 1; i < face.Vertices.Count - 1; i++)
                {
                    triangleMesh.TriangleIndices.Add(face.Vertices[0].Index);
                    triangleMesh.TriangleIndices.Add(face.Vertices[i].Index);
                    triangleMesh.TriangleIndices.Add(face.Vertices[i + 1].Index);

                    Debug.WriteLine(face.Vertices[i].point);
                }
            }

            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            triangleModel.BackMaterial = material;
            model.Content = triangleModel;

            return model;
        }
    }
}
