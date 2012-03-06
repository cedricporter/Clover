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
        FoldingSystem foldingSystem = new FoldingSystem();///折叠系统
        LookupTable table;
        public Clover.LookupTable Table
        {
            get { return table; }
        }
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

            table = new LookupTable(face);
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
        /// 当割点在两条边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedTwoVertices(Face face, Edge edge)
        {
            foldingSystem.CutAFaceWithAddedTwoVertices(face, edge);
        }

       
        /// <summary>
        /// 当割点在一条边上时，另一割点为原来顶点时，切割一个面为两个面
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithAddedOneVertex(Face face, Edge edge, Edge cuttedEdge, Vertex cutVertex)
        {
            foldingSystem.CutAFaceWithAddedOneVertex(face, edge, cuttedEdge, cutVertex);
        }


        /// <summary>
        /// 当割点不在边上时，切割一个面为两个面
        /// </summary>
        /// <param name="oldFace"></param>
        /// <param name="leftChild"></param>
        /// <param name="rightChild"></param>
        /// <param name="edge"></param>
        public void CutAFaceWithoutVertex(Face face, Edge edge)
        {
            foldingSystem.CutAFaceWithoutAddedVertex(face, edge);
        }

        /// <summary>
        /// 切割一个面为两个面
        /// </summary>
        /// <param name="face">需要切割的面。</param>
        /// <param name="edge">割线，割线的两个端点必须在面的边上</param>
        /// <remarks>新产生的两个面会自动作为原来的面的孩子，所以就已经在面树里面了。</remarks>
        void CutAFace(Face face, Edge edge)
        {
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

            // type 0:没有增加顶点， type 1：增加了一个顶点 type 2：增加了两个顶点
            switch (type)
            { 
                case 0:
                    // 种类1 不新增加顶点。只创建一条边并加入边层
                    CutAFaceWithoutVertex(face, edge);
                    break;
                case 1:
                    // 种类2 新增加一个顶点，即只为一条边做切割，并更新面节点
                    edgeLayer.AddTree(new EdgeTree(edge));
                    
                    // 取其中一个进行面分割的边.
                    if (isVertex1Cut)
                    {
                        CutAFaceWithAddedOneVertex(face, edge, e1, edge.Vertex1);
                    }

                    if (isVertex2Cut)
                    {
                        CutAFaceWithAddedOneVertex(face, edge, e2, edge.Vertex2);
                    }
                    break; 
                case 2:
                    CutAFaceWithAddedTwoVertices(face, edge);
                    // 种类3
                    break;
                default:
                    break;
            }

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

            
            table.DeleteFace( face );
            table.AddFace( f1 );
            table.AddFace( f2 );

            

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
        public bool TestFoldingLineCrossed(Face face, Edge currentFoldingLine)
        {
            int crossCount = 0;
            foreach (Edge e in face.Edges)
            { 
                Point3D crossPoint = new Point3D();
                if (CloverMath.GetIntersectionOfTwoSegments(e, currentFoldingLine, ref crossPoint) == 1)
                    crossCount++;
            }
            return crossCount >= 2;
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
            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.Vertex1.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex1.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex1.Moved )
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        //TranslateTransform3D translateToOrigin = new TranslateTransform3D( -e.Vertex1.X, -e.Vertex1.Y, -e.Vertex1.Z);
                        //TranslateTransform3D translateBack = new TranslateTransform3D(e.Vertex1.X, e.Vertex1.Y, e.Vertex1.Z);
                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;
                        //e.Vertex1.SetPoint3D(translateToOrigin.Transform(e.Vertex1.GetPoint3D()));
                        e.Vertex1.SetPoint3D(rotateTransform.Transform(e.Vertex1.GetPoint3D()));
                        //e.Vertex1.SetPoint3D(translateBack.Transform(e.Vertex1.GetPoint3D()));
                        e.Vertex1.Moved = true;
                    }

                    if (e.Vertex2.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex2.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex2.Moved)
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        //TranslateTransform3D translateToOrigin = new TranslateTransform3D( -e.Vertex1.X, -e.Vertex1.Y, -e.Vertex1.Z);
                        //TranslateTransform3D translateBack = new TranslateTransform3D(e.Vertex1.X, e.Vertex1.Y, e.Vertex1.Z);
                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;

                        //e.Vertex2.SetPoint3D(translateToOrigin.Transform(e.Vertex2.GetPoint3D()));
                        e.Vertex2.SetPoint3D(rotateTransform.Transform(e.Vertex2.GetPoint3D()));
                        //e.Vertex2.SetPoint3D(translateBack.Transform(e.Vertex2.GetPoint3D()));
                        e.Vertex2.Moved = true;
                    }
                }
            }

            // 判断是否贴合，若有贴合更新组

            // For Testing
            foreach (List<Vertex> list in vertexLayer.VertexCellTable)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    list[i].SetPoint3D(list[list.Count - 1].GetPoint3D());
                }
            }


            // 修正所有点的移动属性
            foreach (Vertex v in vertexLayer.Vertices)
            {
                v.Moved = false; 
            }

            renderController.UpdateAll();

            table.UpdateLookupTable();
        }

        public void RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, float xRel, float yRel)
        {
            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.Vertex1.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex1.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex1.Moved )
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, 0.1 * yRel);

                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);

                        e.Vertex1.SetPoint3D(rotateTransform.Transform(e.Vertex1.GetPoint3D()));
                        e.Vertex1.Moved = true;
                    }

                    if (e.Vertex2.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex2.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !e.Vertex2.Moved)
                    {
                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;

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

            table.UpdateLookupTable();
        }

        /// <summary>
        /// 根据鼠标位移在每个渲染帧前更新结构
        /// </summary>
        /// <param name="xRel">鼠标的x位移</param>
        /// <param name="yRel">鼠标的y位移</param>
        /// <param name="faceList">折叠所受影响的面</param>
        public void Update(float xRel, float yRel, Vertex pickedVertex, Face pickedFace)
        {
            table.UpdateLookupTable();
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
            //imb.ViewportUnits = BrushMappingMode.Absolute;
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
            return;
        }
        #endregion
    }
}
