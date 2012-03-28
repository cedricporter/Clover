using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Clover
{

    /// <summary>
    /// 影子，用于恢复折纸数据结构
    /// </summary>
    public class ShadowSystem
    {
        #region get/set
        public int OperationLevel
        {
            get { return operationLevel; }
        }
        public List<SnapshotNode> SnapshotList
        {
            get { return snapshotList; }
        }
        #endregion

        #region 成员变量
        int operationLevel = -1;                                            /// 当前level
        List<SnapshotNode> snapshotList = new List<SnapshotNode>();         /// 用以保存Snapshot
        #endregion

        #region 影子系统的基本操作
        /// <summary>
        /// 拍快照
        /// </summary>
        public void Snapshot(SnapshotNode node)
        {
            //CheckUndoTree();

            snapshotList.Add(node);
            operationLevel++;
        }

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            // 没有历史记录了
            if (operationLevel == 0)
                return;

            CloverController controller = CloverController.GetInstance();

            // 将当前状态置为Undoing
            controller.FaceLayer.CurrentLeaves = snapshotList[operationLevel].FaceLeaves;
            controller.FaceLayer.State = FacecellTreeState.Undoing;

            SnapshotNode node = snapshotList[operationLevel];
            switch (node.Type)
            {
                case SnapshotNodeKind.CutKind:
                    // 更新渲染层
                    controller.RenderController.DeleteAll();
                    foreach (Face f in controller.FaceLayer.CurrentLeaves)
                    {
                        controller.RenderController.New(f);
                    }
                    controller.RenderController.UndrawFoldLine();
                    break;
                case SnapshotNodeKind.RotateKind:
                    foreach (Vertex v in node.MovedVertexList)
                    {
                        controller.VertexLayer.DeleteThisVersionToEnd(v);
                    }

                    foreach (Face f in CloverController.GetInstance().FaceLayer.Leaves)
                    {
                        CloverTreeHelper.UpdateFaceVerticesToLastedVersion(f);
                        controller.RenderController.Update(f);
                    }


                    break;
            }

            UndoFaceLayer();

            UndoGroup(node);

            // 修改操作层数
            operationLevel--;
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            // 已经没得Redo了
            if (operationLevel == snapshotList.Count - 1)
                return;
            // 修改操作层数
            operationLevel++;
            // 获取该层叶子
            CloverController controller = CloverController.GetInstance();
            controller.FaceLayer.CurrentLeaves = snapshotList[operationLevel].FaceLeaves;
            if (operationLevel == snapshotList.Count - 1)
                controller.FaceLayer.State = FacecellTreeState.Normal;
            // 更新渲染层
            controller.RenderController.DeleteAll();
            foreach (Face f in controller.FaceLayer.CurrentLeaves)
            {
                controller.RenderController.New(f);
            }
            //controller.RenderController.RedrawFoldLine();
        }
        #endregion

        #region 保存现场
        /// <summary>
        /// 保存一些顶点的历史到vertex layer
        /// </summary>
        /// <param name="vertices"></param>
        public void SaveVertices(List<Vertex> vertices)
        {
            CloverController controller = CloverController.GetInstance();
            foreach (Vertex v in vertices)
            {
                Vertex newVertex = v.Clone() as Vertex;
                controller.VertexLayer.UpdateVertex(newVertex, v.Index);
            }
        }
        #endregion

        #region 还原

        /// <summary>
        /// 重新将面的Layer设置到过去
        /// </summary>
        private void UndoFaceLayer()
        {
            foreach (KeyValuePair<Face, int> pair in snapshotList[operationLevel].FaceLayerMap)
            {
                pair.Key.Layer = pair.Value;
            }
        }

        /// <summary>
        /// 重新将组设置到过去
        /// </summary>
        private void UndoGroup(SnapshotNode node)
        {
            CloverController.GetInstance().FaceGroupLookupTable = node.FaceGroupLookupTable;
        }


        /// <summary>
        /// 检查Undo完了有没有新的snapshot，
        /// 有的话，删除所有的后续状态
        /// </summary>
        public void CheckUndoTree()
        {
            // 如果没有undo过，那么不需要删除redo状态
            if ((operationLevel == 0 && snapshotList.Count == 0) || operationLevel == snapshotList.Count - 1)
                return;

            CloverController controller = CloverController.GetInstance();

            SnapshotNode node = snapshotList[operationLevel];

            switch (node.Type)
            {
                case SnapshotNodeKind.CutKind:
                    RevertCutVertex();
                    break;
                case SnapshotNodeKind.RotateKind:
                    RevertRotateVertex();
                    break;
            }

            if (operationLevel + 1 < snapshotList.Count)
            {
                snapshotList.RemoveRange(operationLevel + 1, snapshotList.Count - operationLevel - 1);
            }

            // 刚刚Undo过了，需要删除所有的后续的记录
            if (controller.FaceLayer.State == FacecellTreeState.Undoing)
            {
                controller.FaceLayer.State = FacecellTreeState.Normal;
            }

            controller.FaceLayer.UpdateLeaves();

            foreach (Face f in controller.FaceLayer.Leaves)
            {
                CloverTreeHelper.UpdateFaceVerticesToLastedVersion(f);
            }
        }

        void RevertFaceAndEdge()
        {
            CloverController controller = CloverController.GetInstance();

            foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            {
                face.LeftChild = null;
                face.RightChild = null;
            }

            //// 保存进入折叠模式前的叶子节点的所有边
            //List<Edge> originEdgeList = new List<Edge>();
            //foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            //{
            //    foreach (Edge e in face.Edges)
            //    {
            //        originEdgeList.Add(e);
            //    }
            //}

            //List<Face> currentFaceList 
            //    = operationLevel < snapshotList.Count - 1 ? snapshotList[operationLevel + 1].FaceLeaves : controller.FaceLayer.Leaves;
            //// 在折叠模式中的面树叶子的所有的边
            //List<Edge> currentEdgeList = new List<Edge>();
            //foreach (Face face in currentFaceList)
            //{
            //    foreach (Edge e in face.Edges)
            //    {
            //        currentEdgeList.Add(e);
            //    }
            //}

            foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            {
                foreach (Edge e in face.Edges)
                {
                    e.LeftChild = null;
                    e.RightChild = null;
                }
            }

            //// 当前叶子的边集合减去原来的叶子的边集得到需要删除的边集
            //List<Edge> beDeletedEdges = currentEdgeList.Except(originEdgeList).ToList();

            //foreach (Edge edge in beDeletedEdges)
            //{
            //    if (edge.Parent.LeftChild == edge)
            //        edge.Parent.LeftChild = null;
            //    else if (edge.Parent.RightChild == edge)
            //        edge.Parent.RightChild = null;
            //}

            int originEdgeListCount = snapshotList[operationLevel].OriginEdgeListCount;
            controller.EdgeLayer.EdgeTreeList.RemoveRange(originEdgeListCount, controller.EdgeLayer.EdgeTreeList.Count - originEdgeListCount);

        }

        void RevertRotateVertex()
        {
            RevertFaceAndEdge();

            List<Vertex> beDeleteVertexVertexList = snapshotList[operationLevel].MovedVertexList;

            CloverController controller = CloverController.GetInstance();

            foreach (Vertex v in beDeleteVertexVertexList)
            {
                // 这里有个小问题，如果是level为最后一层时，我们应该调用DeleteThisVersionToEnd。
                // 不过可喜的是，在Undo里面做了。所以只能是处于undo链表内部。
                controller.VertexLayer.DeleteNextVersionToEnd(v);
            }

            RevertTailVertex();
        }

        void RevertCutVertex()
        {
            RevertFaceAndEdge();

            // 还原点表
            Dictionary<Vertex, int> beDeletedVertexVersionList = new Dictionary<Vertex, int>();

            foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            {
                foreach (Vertex v in face.Vertices)
                {
                    beDeletedVertexVersionList[v] = 0;
                }
            }

            CloverController controller = CloverController.GetInstance();

            foreach (KeyValuePair<Vertex, int> v in beDeletedVertexVersionList)
            {
                Vertex vertex = v.Key;
                controller.VertexLayer.DeleteNextVersionToEnd(vertex);
            }

            RevertTailVertex();
        }

        /// <summary>
        /// 删除尾部多余的顶点
        /// </summary>
        void RevertTailVertex()
        {
            CloverController controller = CloverController.GetInstance();
            int originCount = snapshotList[operationLevel].OriginVertexListCount;
            int currentCount = controller.VertexLayer.VertexCellTable.Count;
            CloverController.GetInstance().VertexLayer.VertexCellTable.RemoveRange(originCount, currentCount - originCount);
        }
 

        #endregion

        #region 半透明的原始面
        List<Face> transparentFaces = new List<Face>();

        /// <summary>
        /// 删除所有的半透明面
        /// </summary>
        public void ClearTransparentFaces()
        {
            RenderController render = CloverController.GetInstance().RenderController;
            foreach (Face face in transparentFaces)
            {
                render.Delete(face);
            }
            transparentFaces.Clear();
        }

        /// <summary>
        /// 创建半透明面
        /// </summary>
        /// <param name="face"></param>
        public void CreateTransparentFace(Face face)
        {
            transparentFaces.Add(face);

            RenderController render = CloverController.GetInstance().RenderController;

            render.New(face);
            render.ToGas(face);
        }
        #endregion
    }
}
