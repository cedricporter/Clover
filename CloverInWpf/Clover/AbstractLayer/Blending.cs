using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.AbstractLayer
{
    class Blending
    {

        #region 成员变量

        Vertex pickedVertex;
        Face nearestFace;
        Edge foldLine = null;
        List<Face> beBlendedFaces = null;

        #endregion

        #region 进入折叠模式

        /// <summary>
        /// 进入折叠模式，初始化变量，找折线和要折叠的面。
        /// </summary>
        /// <param name="pickedVertex">当前选取的顶点</param>
        /// <param name="nearestFace">包含当前顶点的离摄像机最近的面</param>
        public void EnterBlendingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.pickedVertex = pickedVertex;
            this.nearestFace = nearestFace;
        }

        /// <summary>
        /// 寻找所有要折叠（旋转）的面，放在beBlendedFaces中。
        /// </summary>
        void FindBeBlendedFaces()
        {
            // 一，由nearestFace开始，自底向上寻找最初拥有pickedVertex的面
            Face rootFace = nearestFace;
            Boolean reachedRoot;
            while (rootFace.Parent != null)
            {
                reachedRoot = true;
                foreach (Vertex v in rootFace.Parent.Vertices)
                {
                    if (pickedVertex.Index == v.Index)
                    {
                        reachedRoot = false;
                        break;
                    }
                }
                if (reachedRoot)
                    break;
                rootFace = rootFace.Parent;
            }
            // 二，自顶向下地找到rootFace的所有叶子节点
            beBlendedFaces = CloverTreeHelper.GetLeaves(rootFace);
        }

        /// <summary>
        /// 寻找折线，折线会被放置在foldLine中。
        /// </summary>
        void FindFoldLine()
        {
            FaceGroup group = CloverController.GetInstance().FaceGroupLookupTable.GetGroup(nearestFace);
            if (group == null)
                System.Windows.MessageBox.Show("这怎么可能？一个face居然不在group中！--Blending");
            // 寻找折线的三个判定条件
            // 一，折线在pickedVertex所在的面上
            List<Face> candidateFaces = new List<Face>();
            foreach (Face face in group.GetFaceList())
            {
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face))
                    candidateFaces.Add(face);
            }

            foreach (Face face in candidateFaces)
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
                    break;
            }
        }

        #endregion

        

        public void ExitBlendingMode()
        {

        }

        public void OnDrag()
        {

        }
    }
}
