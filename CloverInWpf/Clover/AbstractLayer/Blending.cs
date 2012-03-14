using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Media.Media3D;

/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	逻辑层处理Blending操作的类
**/

namespace Clover.AbstractLayer
{
    /// <summary>
    /// Rank1 Prerequisite 不满足以下条件该类会崩溃
    /// 所有面都被正确地添加到组中
    /// 组是已排序的
    /// 
    /// Rank2 Prerequisite 不满足一下条件可能会导致无法预料的结果
    /// 纸张被正确地散开
    /// 
    /// Todo
    /// 增加组的法线的正负向判断
    /// 磁性
    /// </summary>
    class Blending
    {

        #region 成员变量

        Vertex pickedVertex = null;                         /// 当前被拾取的点
        Face nearestFace = null;                            /// 被拾取点所在的面
        FaceGroup group = null;                             /// 被拾取点所在的面所在的组
        Edge foldLine = null;                               /// Blending的折线
        List<Face> faceContainVertex = null;                /// 所有拥有被拾取点的面
        List<Face> beBlendedFaces = null;                   /// 所有需要被Blending的面
        int checkMask = 0;                                  /// 校验位

        #endregion

        #region 进入折叠模式

        /// <summary>
        /// 进入Blending模式，初始化变量，找折线和要折叠的面。
        /// </summary>
        /// <param name="pickedVertex">当前选取的顶点</param>
        /// <param name="nearestFace">包含当前顶点的离摄像机最近的面</param>
        public void EnterBlendingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.pickedVertex = pickedVertex;
            checkMask += 1;
            this.nearestFace = nearestFace;
            checkMask += 2;
            FindBeBlendedFaces();
            FindFoldLine();
        }

        /// <summary>
        /// 寻找所有要折叠（旋转）的面，放在beBlendedFaces中。
        /// </summary>
        void FindBeBlendedFaces()
        {
            // 寻找所有与nearestFace同一group的Face
            group = CloverController.GetInstance().FaceGroupLookupTable.GetGroup(nearestFace);
            checkMask += 4;
            if (group == null)
                System.Windows.MessageBox.Show("这怎么可能？一个face居然不在group中！--Blending");
            // 找到所有拥有pickedVertex的Face
            faceContainVertex = new List<Face>();
            checkMask += 8;
            foreach (Face face in group.GetFaceList())
            {
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face))
                    faceContainVertex.Add(face);
            }
            // 找到所有拥有pickedVertex中layer最“低”的那个面
            // 这里需要加一个正负法线方向的判断 Todo
            Face baseFace = faceContainVertex[0];
            foreach (Face face in faceContainVertex)
            {
                if (face.Layer < baseFace.Layer)
                    baseFace = face;
            }
            // group中所有layer高于baseFace的面即为beBlendedFaces
            beBlendedFaces = new List<Face>();
            checkMask += 16;
            foreach (Face face in group.GetFaceList())
            {
                if (face.Layer >= baseFace.Layer)
                    beBlendedFaces.Add(face);
            }
        }

        /// <summary>
        /// 寻找折线，折线会被放置在foldLine中。
        /// </summary>
        void FindFoldLine()
        {
            // 寻找折线的三个判定条件
            // 一，折线在pickedVertex所在的面上
            foreach (Face face in faceContainVertex)
            {
                foreach (Edge edge in face.Edges)
                {
                    // 二，pickedVertex不可能在折线上
                    if (CloverTreeHelper.IsVertexInEdge(pickedVertex, edge))
                        continue;
                    // 三，折线关联两个面,且一个面位于beBlendedFaces，另一个面则不是
                    if (edge.Face1 != null && edge.Face2 != null)
                    {
                        int judgement = 1;
                        if (beBlendedFaces.Contains(edge.Face1))
                            judgement--;
                        if (beBlendedFaces.Contains(edge.Face2))
                            judgement--;
                        if (judgement == 0) // 找到了！
                        {
                            foldLine = edge;
                            break;
                        }
                    }
                }
                // 第一个找到的边就是折线
                if (foldLine != null)
                {
                    checkMask += 32;
                    break;
                }
            }
        }

        #endregion

        #region 鼠标拖动时执行旋转

        /// <summary>
        /// 根据鼠标的X朝向位移来控制Blending角度
        /// </summary>
        /// <param name="offsetX"></param>
        public void OnDrag(Double offsetX)
        {
            // 未通过验证条件
            if (checkMask != 63)
            {
                //Debug.WriteLine(checkMask);
                return;
            }
            Debug.WriteLine(offsetX);
            RotateFaces(beBlendedFaces, foldLine, offsetX);
        }

        /// <summary>
        /// 克隆和更新一个新的顶点到VertexLayer
        /// 这个函数是完全从FoldingSystem拷贝过来的
        /// </summary>
        /// <param name="v"></param>
        void CloneAndUpdateVertex(Vertex v)
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            vertexLayer.UpdateVertex(v.Clone() as Vertex, v.Index);
        }

        /// <summary>
        /// 旋转一个面表中除去折痕的所有点 
        /// 这个函数是完全从FoldingSystem拷贝过来的
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
                        && e.Vertex1.GetPoint3D() != foldingLine.Vertex2.GetPoint3D() && !movedVertexDict[e.Vertex1.Index])
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


        /// <summary>
        /// 退出Blending模式
        /// </summary>
        public void ExitBlendingMode()
        {
            pickedVertex = null;
            nearestFace = null;
            foldLine = null;
            group = null;
            beBlendedFaces = null;
            faceContainVertex = null;
            checkMask = 0;
        }

        
    }
}
