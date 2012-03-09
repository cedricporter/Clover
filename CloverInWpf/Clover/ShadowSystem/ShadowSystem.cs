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

        #region 只读属性

        int operationLevel = -1;                                            /// 当前level
        public int OperationLevel
        {
            get { return operationLevel; }
        }

        List<SnapshotNode> snapshotList = new List<SnapshotNode>();         /// 用以保存Snapshot
        public List<SnapshotNode> SnapshotList
        {
            get { return snapshotList; }
        }

        #endregion

        /// <summary>
        /// 拍快照
        /// </summary>
        public void Snapshot(SnapshotNode node)
        {
            CheckUndoTree();

            snapshotList.Add(node);
            operationLevel++;
        }
        //public void Snapshot(List<Face> leaves, List<Edge> newEdges)
        //{
        //    SnapshotNode snapshot = new SnapshotNode(leaves);
        //    if (newEdges != null)
        //        snapshot.NewEdges = newEdges;
        //    snapshotList.Add(snapshot);
        //    // 当前操作的层数
        //    operationLevel++;
        //}
        //public void Snapshot(List<Face> leaves)
        //{
        //    Snapshot(leaves, null);
        //}
        //public void Snapshot(List<Edge> newEdges)
        //{
        //    Snapshot(CloverController.GetInstance().FaceLeaves, newEdges);
        //}
        //public void Snapshot()
        //{
        //    Snapshot(CloverController.GetInstance().FaceLeaves);
        //}

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            // 没有历史记录了
            if (operationLevel == -1)
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
            controller.RenderController.RedrawFoldLine();
        }



        #region 成员变量
        int originEdgeListCount = -1;
        int originVertexListCount = -1;
        LookupTable originGroup;

        /// <summary>
        /// 进入折叠模式前的叶子节点表，用于恢复
        /// </summary>
        List<Face> originFaceList = new List<Face>();
        #endregion

        #region get/set
        public List<Face> OriginFaceList
        {
            get { return originFaceList; }
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

        #region 历史遗留问题

        /// <summary>
        /// 为所有顶点保存历史记录
        /// </summary>
        public void SaveOriginVertices()
        {
            VertexLayer vertexLayer = CloverController.GetInstance().VertexLayer;
            for (int i = 0; i < vertexLayer.VertexCellTable.Count; i++)
            {
                Vertex v = vertexLayer.Vertices[i].Clone() as Vertex;

                vertexLayer.UpdateVertex(v, v.Index);
            }
        }

        /// <summary>
        /// 保存到原始叶节点表
        /// </summary>
        /// <param name="leaves">当前的叶子</param>
        /// <remarks>
        /// 当撤销的时候，只需将originFaceList里面的face的孩子都清空就可以还原面树了。
        /// </remarks>
        public void SaveOriginState()
        {
            //SaveOriginVertices();
            CloverController controller = CloverController.GetInstance();

            originFaceList.Clear();
            foreach (Face f in controller.FaceLayer.Leaves)
            {
                originFaceList.Add(f);
            }

            originEdgeListCount = controller.EdgeLayer.Count;
            originVertexListCount = controller.VertexLayer.Vertices.Count;
            originGroup = controller.Table.Clone() as LookupTable;
        }

        /// <summary>
        /// 还原到originLeaves
        /// </summary>
        public void Revert()
        {
            CloverController controller = CloverController.GetInstance();

            // 保存进入折叠模式前的叶子节点的所有边
            List<Edge> originEdgeList = new List<Edge>();
            foreach (Face face in originFaceList)
            {
                foreach (Edge e in face.Edges)
                {
                    originEdgeList.Add(e);
                }
            }

            // 在折叠模式中的面树叶子的所有的边
            List<Edge> currentEdgeList = new List<Edge>();
            foreach (Face face in controller.FaceLayer.Leaves)
            {
                foreach (Edge e in face.Edges)
                {
                    currentEdgeList.Add(e);
                }
            }

            // 当前叶子的边集合减去原来的叶子的边集得到需要删除的边集
            List<Edge> beDeletedEdges = currentEdgeList.Except(originEdgeList).ToList();

            foreach (Edge edge in beDeletedEdges)
            {
                if (edge.Parent == null)
                    continue;

                if (edge.Parent.LeftChild == edge)
                    edge.Parent.LeftChild = null;
                if (edge.Parent.RightChild == edge)
                    edge.Parent.RightChild = null;
            }

            controller.EdgeLayer.EdgeTreeList.RemoveRange(originEdgeListCount, controller.EdgeLayer.EdgeTreeList.Count - originEdgeListCount);

            List<Vertex> beDeletedVertexVersionList = new List<Vertex>();

            foreach (Face face in originFaceList)
            {
                // 内节点一定有两个孩子，否则就出错了。
                if (face.LeftChild != null && face.RightChild != null)
                {
                    beDeletedVertexVersionList.AddRange(face.LeftChild.Vertices.Except(beDeletedVertexVersionList));
                    controller.RenderController.Delete(face.LeftChild);
                    beDeletedVertexVersionList.AddRange(face.RightChild.Vertices.Except(beDeletedVertexVersionList));
                    controller.RenderController.Delete(face.RightChild);
                    controller.RenderController.New(face);
                }
                face.LeftChild = face.RightChild = null;
            }

          
            // 还原点表
            foreach (Vertex v in beDeletedVertexVersionList)
            {
                controller.VertexLayer.DeleteLastVersion(v.Index);
            }
            for (int i = originVertexListCount; i < controller.VertexLayer.VertexCellTable.Count + 1; i++)
            {
                controller.VertexLayer.DeleteVertex(originVertexListCount);
            }

            // 还原组
            controller.Table = originGroup;
            //renderController.UpdateAll();

            controller.FaceLayer.UpdateLeaves();

            //UpdatePaper();

            ClearTransparentFaces();
        }

        #endregion

        #endregion

        #region 还原

        /// <summary>
        /// 检查Undo完了有没有新的snapshot，
        /// 有的话，删除所有的后续状态
        /// </summary>
        void CheckUndoTree()
        {
            // 如果没有undo过，那么不需要删除redo状态
            if (operationLevel == snapshotList.Count - 1)
                return;

            CloverController controller = CloverController.GetInstance();

            // 刚刚Undo过了，需要删除所有的后续的记录
            if (controller.FaceLayer.State == FacecellTreeState.Undoing)
            {
                controller.FaceLayer.State = FacecellTreeState.Normal;
            }

            SnapshotNode node = snapshotList[operationLevel];

            switch (node.Type)
            {
                case SnapshotNodeKind.CutKind:
                    break;
                case SnapshotNodeKind.RotateKind:
                    break;
            }

            snapshotList.RemoveRange(operationLevel, snapshotList.Count - operationLevel + 1);
        }
         
        void RevertCut()
        {
            CloverController controller = CloverController.GetInstance();

            foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            {
                face.LeftChild = null;
                face.RightChild = null;
            }

            // 保存进入折叠模式前的叶子节点的所有边
            List<Edge> originEdgeList = new List<Edge>();
            foreach (Face face in snapshotList[operationLevel - 1].FaceLeaves)
            {
                foreach (Edge e in face.Edges)
                {
                    originEdgeList.Add(e);
                }
            }

            // 在折叠模式中的面树叶子的所有的边
            List<Edge> currentEdgeList = new List<Edge>();
            foreach (Face face in snapshotList[operationLevel].FaceLeaves)
            {
                foreach (Edge e in face.Edges)
                {
                    currentEdgeList.Add(e);
                }
            }

            // 当前叶子的边集合减去原来的叶子的边集得到需要删除的边集
            List<Edge> beDeletedEdges = currentEdgeList.Except(originEdgeList).ToList();

            foreach (Edge edge in beDeletedEdges)
            {
                if (edge.Parent.LeftChild == edge)
                    edge.Parent.LeftChild = null;
                if (edge.Parent.RightChild == edge)
                    edge.Parent.RightChild = null;
            }

            controller.EdgeLayer.EdgeTreeList.RemoveRange(originEdgeListCount, controller.EdgeLayer.EdgeTreeList.Count - originEdgeListCount);

            List<Vertex> beDeletedVertexVersionList = new List<Vertex>();

            foreach (Face face in originFaceList)
            {
                // 内节点一定有两个孩子，否则就出错了。
                if (face.LeftChild != null && face.RightChild != null)
                {
                    beDeletedVertexVersionList.AddRange(face.LeftChild.Vertices.Except(beDeletedVertexVersionList));
                    controller.RenderController.Delete(face.LeftChild);
                    beDeletedVertexVersionList.AddRange(face.RightChild.Vertices.Except(beDeletedVertexVersionList));
                    controller.RenderController.Delete(face.RightChild);
                    controller.RenderController.New(face);
                }
                face.LeftChild = face.RightChild = null;
            }

            // 还原点表
            foreach (Vertex v in beDeletedVertexVersionList)
            {
                controller.VertexLayer.DeleteThisVersionToEnd(v);
            }
            for (int i = originVertexListCount; i < controller.VertexLayer.VertexCellTable.Count; i++)
            {
                controller.VertexLayer.DeleteVertex(i);
            }

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
