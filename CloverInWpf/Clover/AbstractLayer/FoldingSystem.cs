using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace Clover
{
    public class FoldingSystem
    {
        #region 属性
        List<Face> lastTimeMovedFaces;

        #region get/set
        public List<Face> GetLastTimeMovedFace()
        {
            return lastTimeMovedFaces;
        }
        #endregion

        #endregion

        #region 一些辅助计算的函数
        /// <summary>
        /// 将一堆面的周围的顶点找出并返回一个list
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
        /// 计算顶点纹理坐标
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool CalculateTexcoord(Vertex vertex, Edge edge)
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
        public bool CalculateTexcoord(Vertex vertex, Edge edge, double length)
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
        #endregion

        #region 分割面

        /// <summary>
        /// 切割一个面为两个面
        /// </summary>
        /// <param name="face">要切割的面</param>
        /// <param name="edge">割线，不一定要在边层里面的边，只要两个端点的坐标在面的边上即可</param>
        /// <returns>被边引用的割线</returns>
        public Edge CutFace(Face face, Edge edge)
        {
            Debug.Assert(edge != null);
            if (edge == null)
            {
                System.Windows.MessageBox.Show("There is no edge for CutFace.");
                return null;
            }

            CloverController controller = CloverController.GetInstance();
            RenderController render = controller.RenderController;
            VertexLayer vertexLayer = controller.VertexLayer;
            ShadowSystem shadowSystem = controller.ShadowSystem;

            Vertex newVertex1 = edge.Vertex1.Clone() as Vertex;
            Vertex newVertex2 = edge.Vertex2.Clone() as Vertex;
            Vertex newVertexOld1 = newVertex1;
            Vertex newVertexOld2 = newVertex2;

            // 生成一个面的周围的顶点的环形表
            face.UpdateVertices();
            List<Vertex> vertexList = new List<Vertex>();
            vertexList.AddRange(face.Vertices);
            int count = vertexList.Count + 1;
            while (true)
            {
                if (
                    (
                    !CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[1].GetPoint3D())
                    && CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[0].GetPoint3D(), vertexList[1].GetPoint3D(), 0.001)
                    )
                    || 
                    (
                     !CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[1].GetPoint3D())
                    && CloverMath.IsPointInTwoPoints( newVertex2.GetPoint3D(), vertexList[ 0 ].GetPoint3D(), vertexList[ 1 ].GetPoint3D(), 0.001 )
                    )
                    )
                {
                    break;
                }
                vertexList.Add(vertexList[0]);
                vertexList.RemoveAt(0);

                // 防止死循环
                if (count-- == 0)
                    return null;
            }
            vertexList.Add(vertexList[0]);

            // 要被分割的边
            Edge beCutEdge1 = null;     
            Edge beCutEdge2 = null;

            // 原始边
            List<Edge> rangeA = new List<Edge>();
            List<Edge> rangeB = new List<Edge>();

            // 分割面
            Face f1 = new Face(face.Layer);
            Face f2 = new Face(face.Layer);
            face.LeftChild = f1;
            face.RightChild = f2;

            List<Edge> currentEdgeList = null;
            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                Edge currentEdge = FindEdgeByTwoVertexInAFace(face, vertexList[i], vertexList[i + 1]);

                // 割线过顶点
                if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D()))
                {
                    currentEdgeList = rangeA;
                    newVertex1 = vertexList[i];
                }
                else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D()))
                {
                    currentEdgeList = rangeB;
                    newVertex2 = vertexList[i];
                }
                // 割线过边
                else if (CloverMath.IsPointInTwoPoints(newVertex1.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001)
                    && !CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), vertexList[i + 1].GetPoint3D()))
                {
                    currentEdgeList = rangeA;

                    beCutEdge1 = currentEdge;
                    Edge cutEdge1 = null;
                    Edge cutEdge2 = null;

                    // 两个孩子为空，新建两条边

                    if (beCutEdge1.LeftChild == null && beCutEdge1.RightChild == null)
                    {
                        Debug.Assert(newVertex1 != null);
                        // 分割一条边生成两条新的边
                        cutEdge1 = new Edge(vertexList[i], newVertex1);
                        cutEdge2 = new Edge(newVertex1, vertexList[i + 1]);
                        beCutEdge1.LeftChild = cutEdge1;
                        beCutEdge1.RightChild = cutEdge2;
                        rangeB.Add(cutEdge1);
                        rangeA.Add(cutEdge2);
                    }
                    else
                    {
                        // 对于已经割过的边，我们“必须”使用它原来的顶点
                        if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.LeftChild.Vertex1.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.LeftChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.LeftChild.Vertex2.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.LeftChild.Vertex2;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.RightChild.Vertex1.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.RightChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex1.GetPoint3D(), beCutEdge1.RightChild.Vertex2.GetPoint3D()))
                        {
                            newVertex1 = beCutEdge1.RightChild.Vertex2;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("fuck2");
                        }

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge1.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeB.Add(beCutEdge1.LeftChild);
                            rangeA.Add(beCutEdge1.RightChild);
                        }
                        else
                        {
                            rangeA.Add(beCutEdge1.LeftChild);
                            rangeB.Add(beCutEdge1.RightChild);
                        }
                    }

                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex1, beCutEdge1);
                   
                    continue;
                }
                else if (CloverMath.IsPointInTwoPoints(newVertex2.GetPoint3D(), vertexList[i].GetPoint3D(), vertexList[i + 1].GetPoint3D(), 0.001)
                    && !CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), vertexList[i + 1].GetPoint3D()))
                {
                    currentEdgeList = rangeB;

                    beCutEdge2 = currentEdge;

                    // 两个孩子为空，新建两条边
                    if (beCutEdge2.LeftChild == null && beCutEdge2.RightChild == null)
                    {
                        Debug.Assert(newVertex2 != null);
                        // 分割一条边生成两条新的边
                        Edge cutEdge1 = new Edge(vertexList[i], newVertex2);
                        Edge cutEdge2 = new Edge(newVertex2, vertexList[i + 1]);
                        beCutEdge2.LeftChild = cutEdge1;
                        beCutEdge2.RightChild = cutEdge2;

                        rangeA.Add(cutEdge1);
                        rangeB.Add(cutEdge2);
                    }
                    else
                    {
                        // 对于已经割过的边，我们“必须”使用它原来的顶点
                        if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.LeftChild.Vertex1.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.LeftChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.LeftChild.Vertex2.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.LeftChild.Vertex2;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.RightChild.Vertex1.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.RightChild.Vertex1;
                        }
                        else if (CloverMath.IsTwoPointsEqual(newVertex2.GetPoint3D(), beCutEdge2.RightChild.Vertex2.GetPoint3D()))
                        {
                            newVertex2 = beCutEdge2.RightChild.Vertex2;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("fuck");
                        }

                        if (CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex1.GetPoint3D(), 0.001)
                            || CloverMath.IsTwoPointsEqual(vertexList[i].GetPoint3D(), beCutEdge2.LeftChild.Vertex2.GetPoint3D(), 0.001))
                        {
                            rangeA.Add(beCutEdge2.LeftChild);
                            rangeB.Add(beCutEdge2.RightChild);
                        }
                        else
                        {
                            rangeB.Add(beCutEdge2.LeftChild);
                            rangeA.Add(beCutEdge2.RightChild);
                        }
                    }
                    // 计算newVertex1和newVertex2的纹理坐标
                    CalculateTexcoord(newVertex2, beCutEdge2);
                    continue;
                }
                currentEdgeList.Add(currentEdge);
            }

            Edge newEdge = new Edge(newVertex1, newVertex2);

            controller.EdgeLayer.AddTree(new EdgeTree(newEdge));

            // 是否是新的顶点
            if (newVertex1 == newVertexOld1)
            {
                vertexLayer.InsertVertex(newVertex1);
                //render.AddVisualInfoToVertex( newVertex1 );
            }
            if (newVertex2 == newVertexOld2)
            {
                vertexLayer.InsertVertex(newVertex2);
                //render.AddVisualInfoToVertex( newVertex2 );
            }


            // 更新新边的Faces指针 
            newEdge.Face1 = f1;
            newEdge.Face2 = f2;

            // 给两个新面加新的边
            rangeA.Add(newEdge);
            rangeB.Add(newEdge);

            foreach (Edge e in rangeA)
            {
                f1.AddEdge(e);
            }
            foreach (Edge e in rangeB)
            {
                f2.AddEdge(e);
            }

            // 更新两个新面的法向量标杆
            f1.StartVertex1 = newVertex2;
            f1.StartVertex2 = newVertex1;

            f2.StartVertex1 = newVertex1;
            f2.StartVertex2 = newVertex2;

            // 更新面的都为顶点的顺序
            f1.UpdateVertices();
            f2.UpdateVertices();

            // 更新渲染层的部分
            render.Delete(face);
            render.New(f1);
            render.New(f2);

            //render.AntiOverlap();

            //newVertex1.Update( newVertex1, null );
            //newVertex2.Update( newVertex2, null );
            //render.AddFoldingLine( newVertex1.u, newVertex1.v, newVertex2.u, newVertex2.v );

            // 
            controller.FaceLayer.UpdateLeaves();
           
            //controller.Table.UpdateTableAfterFoldUp(true);

            return newEdge;
        }

        #region 辅助函数
        /// <summary>
        /// 更新面的所有的顶点到在vertexLayer中最新的版本。
        /// </summary>
        public void UpdateFaceVerticesToLastedVersion(Face face)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            foreach (Edge e in face.Edges)
            {
                e.Vertex1 = vertexLayer.GetVertex(e.Vertex1.Index);
                e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);
            }
            face.UpdateVertices();
        }
        #endregion

        /// <summary>
        /// 切割多个面
        /// </summary>
        /// <param name="faceList">要切割的面的列表</param>
        /// <param name="foldingLine">分割线</param>
        /// <returns>在每个面上的折线</returns>
        public List<Edge> CutFaces(List<Face> faceList, Edge foldingLine)
        {
            // 切割面
            List<Edge> newEdges = new List<Edge>();
            foreach (Face face in faceList)
            {
                Edge edge = GetFoldingLineOnAFace(face, foldingLine);

                Debug.Assert(edge != null);

                newEdges.Add(CutFace(face, edge));
                
            }

            return newEdges;
        }

        #endregion

        #region 有关折线的函数

        /// <summary>
        /// 获取折线
        /// </summary>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="pickedPoint">选中的点</param>
        /// <param name="projectionPoint">投影点</param>
        /// <returns></returns>
        public Edge GetFoldingLine(Face pickedFace, Point3D pickedPoint, Point3D projectionPoint)
        {
            return CloverMath.GetPerpendicularBisector3D(pickedFace, pickedPoint, projectionPoint);
        }
        /// <summary>
        /// 找到折线穿过面的那条线段
        /// </summary>
        /// <param name="face">要测试的面</param>
        /// <param name="currentFoldingLine">当前的折线</param>
        /// <returns>对于测试面的折线</returns>
        public Edge GetFoldingLineOnAFace(Face face, Edge currentFoldingLine)
        {
            Vertex vertex1 = new Vertex();
            Vertex vertex2 = new Vertex();

            bool findFirst = false;
            
            foreach (Edge e in face.Edges)
            { 
                Point3D crossPoint = new Point3D();
                if (CloverMath.GetIntersectionOfTwoSegments(e, currentFoldingLine, ref crossPoint) == 1)
                {
                    if (!findFirst)
                    {
                        vertex1.SetPoint3D(crossPoint);
                        findFirst = true;
                    }
                    else if ( crossPoint != vertex1.GetPoint3D())
                    {
                        vertex2.SetPoint3D(crossPoint);
                        Edge foldingLine = new Edge(vertex1, vertex2);
                        return foldingLine;
                    }
                }
            }
            return null;
        }

        #endregion

        #region 有关折叠的函数

        /// <summary>
        /// 直接折叠到投影点上
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="pickedVertex"></param>
        /// <param name="projectionPoint"></param>
        public bool FoldingUpToPoint(Face pickedFace, Point3D originPoint, Point3D projectionPoint, Edge currentFoldingLine)
        {
            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;

            // 查找所有需要移动的面
            List<Face> rotateFaces = AddMovedFace(originPoint, pickedFace, currentFoldingLine);
            lastTimeMovedFaces = rotateFaces;

            // 计算所需旋转角度
            Point3D crossPoint = new Point3D();
            Edge segmentFromOriginToPro = new Edge(new Vertex(originPoint), new Vertex(projectionPoint));

            // 此处应该是求折线所在直线与线段的交点
            // 将当前折线延长
            Vertex longVertex1 = new Vertex(currentFoldingLine.Vertex1.GetPoint3D());
            Vertex longVertex2 = new Vertex(currentFoldingLine.Vertex2.GetPoint3D());
            Vector3D foldingLineVector = longVertex1.GetPoint3D() - longVertex2.GetPoint3D();
            longVertex1.SetPoint3D(longVertex1.GetPoint3D() + foldingLineVector * 100);
            longVertex2.SetPoint3D(longVertex2.GetPoint3D() - foldingLineVector * 100);
            Edge longFoldingLine = new Edge(longVertex1, longVertex2);
            if (1 != CloverMath.GetIntersectionOfTwoSegments(longFoldingLine, segmentFromOriginToPro, ref crossPoint))
            {
                // System.Windows.MessageBox.Show("Fuck 木有交点");
                return false;
            }

            Vector3D v1 = originPoint - crossPoint;
            Vector3D v2 = projectionPoint - crossPoint;

            double angle = Vector3D.AngleBetween(v1, v2);

            // 根据旋转角度对所有移动面进行旋转
            RotateFaces(rotateFaces, currentFoldingLine, angle);

            return true;
        }

        /// <summary>
        /// 测试要移动的面
        /// </summary>
        /// <param name="face">待测试的面</param>
        /// <param name="pickedFace">选中的面</param>
        /// <param name="pickedVertex">选中的点</param>
        /// <returns></returns>
        public bool TestMovedFace(Face face, Face pickedFace, Point3D originPoint)
        {
            // 选定的面一定是移动面
            if (face == pickedFace)
                return true;

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
        /// 判定移动的面
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public List<Face> AddMovedFace(Point3D originPoint, Face pickedFace, Edge foldingLine)
        {
            FaceLayer faceLayer = CloverController.GetInstance().FaceLayer;
            FaceGroupLookupTable table = CloverController.GetInstance().FaceGroupLookupTable;
            //table.UpdateLookupTable();

            List<Face> faceWithFoldingLine = new List<Face>();
            List<Face> faceWithoutFoldingLine = new List<Face>();

           

            CutFaces(faceWithFoldingLine, foldingLine);

            foreach (Face face in faceWithFoldingLine)
            {
                faceWithoutFoldingLine.Add(face.LeftChild);
                faceWithoutFoldingLine.Add(face.RightChild);
            }

            return faceWithoutFoldingLine;
        }
        #endregion

        #region 有关旋转的函数

        /// <summary>
        /// 克隆和更新一个新的顶点到VertexLayer
        /// </summary>
        /// <param name="v"></param>
        void CloneAndUpdateVertex(Vertex v)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            vertexLayer.UpdateVertex(v.Clone() as Vertex, v.Index);
        }

        /// <summary>
        /// 旋转一个面表中除去折痕的所有点 
        /// </summary>
        /// <param name="beRotatedFaceList">待旋转的面表</param>
        /// <param name="foldingLine">折线</param>
        /// <param name="angle">角度</param>
        public List<Vertex> RotateFaces(List<Face> beRotatedFaceList, Edge foldingLine, double angle)
        {
            ShadowSystem shadowSystem = CloverController.GetInstance().ShadowSystem;
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            RenderController render = CloverController.GetInstance().RenderController;
            FaceGroupLookupTable table = CloverController.GetInstance().FaceGroupLookupTable;

            List<Vertex> movedVertexList = new List<Vertex>();
            shadowSystem.CheckUndoTree();

            Dictionary<int, bool> movedVertexDict = new Dictionary<int, bool>();
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    foreach (Vertex v in f.Vertices)
                        movedVertexDict[v.Index] = false;
                }
            }

            // 根据鼠标位移修正所有移动面中不属于折线顶点的其他顶点
            foreach (Face f in beRotatedFaceList)
            {
                foreach (Edge e in f.Edges)
                {
                    if (e.Vertex1.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex1.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !movedVertexDict[e.Vertex1.Index] )
                    {
                        CloneAndUpdateVertex(e.Vertex1);

                        e.Vertex1 = vertexLayer.GetVertex(e.Vertex1.Index);

                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;
                        e.Vertex1.SetPoint3D(rotateTransform.Transform(e.Vertex1.GetPoint3D()));

                        movedVertexDict[e.Vertex1.Index] = true; 
                        movedVertexList.Add(e.Vertex1);
                    }

                    if (e.Vertex2.GetPoint3D() != foldingLine.Vertex1.GetPoint3D() 
                        && e.Vertex2.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !movedVertexDict[e.Vertex2.Index])
                    {
                        CloneAndUpdateVertex(e.Vertex2);

                        e.Vertex2 = vertexLayer.GetVertex(e.Vertex2.Index);

                        Vector3D axis = new Vector3D();
                        axis.X = foldingLine.Vertex1.X - foldingLine.Vertex2.X;
                        axis.Y = foldingLine.Vertex1.Y - foldingLine.Vertex2.Y;
                        axis.Z = foldingLine.Vertex1.Z - foldingLine.Vertex2.Z;
                        axis.Normalize();

                        AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, angle);
                        RotateTransform3D rotateTransform = new RotateTransform3D(rotation);
                        rotateTransform.CenterX = (foldingLine.Vertex1.X + foldingLine.Vertex2.X) / 2;
                        rotateTransform.CenterY = (foldingLine.Vertex1.Y + foldingLine.Vertex2.Y) / 2;
                        rotateTransform.CenterZ = (foldingLine.Vertex1.Z + foldingLine.Vertex2.Z) / 2;

                        e.Vertex2.SetPoint3D(rotateTransform.Transform(e.Vertex2.GetPoint3D()));

                        movedVertexDict[e.Vertex2.Index] = true; 
                        movedVertexList.Add(e.Vertex2); 
                    } 
                }
            }

            // 因为顶点克隆过了，所以所有的面的边都要更新到引用最新的顶点
            foreach (Face f in CloverController.GetInstance().FaceLayer.Leaves)
            {
                CloverTreeHelper.UpdateFaceVerticesToLastedVersion(f);
            }

            // 必须先更新group后更新render
            //table.UpdateLookupTable();
            render.UpdateAll();

            return movedVertexList;
        }

        #endregion
    }
}
