using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Clover
{
    /// <summary>
    /// 快照节点的类型
    /// </summary>
    public enum ShapshotNodeKind
    {
        RotateKind,
        CutKind
    }
    
    /// <summary>
    /// 快照节点
    /// </summary>
    public class SnapshotNode
    {
        SnapshotNode type;
        List<Face> faceLeaves = new List<Face>();

        #region get/set
        public Clover.SnapshotNode Type
        {
            get { return type; }
            set { type = value; }
        }
        public List<Face> FaceLeaves
        {
            get { return faceLeaves; }
            set { faceLeaves = value; }
        }
        #endregion

        #region API
        public SnapshotNode(List<Face> leaves)
        {
            foreach (Face f in leaves)
            {
                faceLeaves.Add(f);
            }
        }

        public void Add(Face face)
        {
            faceLeaves.Add(face);
        }
        #endregion

    }

    /// <summary>
    /// 影子，用于恢复折纸数据结构
    /// </summary>
    public class ShadowSystem
    {
        #region 成员变量
        int originEdgeListCount = -1;
        int originVertexListCount = -1;

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
        }

        #endregion

        #region 还原
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
                edge.Parent = null;
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
            for (int i = originVertexListCount; i < controller.VertexLayer.Vertices.Count; i++)
            {
                controller.VertexLayer.DeleteVertex(i);
            }

            //renderController.UpdateAll();

            controller.FaceLayer.UpdateLeaves();

            //UpdatePaper();

            ClearTransparentFaces();
        }

        List<SnapshotNode> snapshotList = new List<SnapshotNode>();


        private void AddSnapshot(SnapshotNode snapshot)
        {
            snapshotList.Add(snapshot);
        }

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
                edge.Parent = null;
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
            for (int i = originVertexListCount; i < controller.VertexLayer.Vertices.Count; i++)
            {
                controller.VertexLayer.DeleteVertex(i);
            }

            snapshotList.RemoveRange(operationLevel, snapshotList.Count - operationLevel + 1);

        }


        /// <summary>
        /// 拍快照
        /// </summary>
        public void Snapshot(List<Face> leaves)
        {
            SnapshotNode snapshot = new SnapshotNode(leaves);

            AddSnapshot(snapshot);

            // 当前操作的层数
            operationLevel++;
        }
        public void Snapshot()
        {
            Snapshot(CloverController.GetInstance().FaceLeaves);
        }

        int operationLevel = -1;    /// 当前level

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            CloverController controller = CloverController.GetInstance();

            // 没有历史记录了
            if (operationLevel <= -1)
                return;

            controller.FaceLayer.CurrentLeaves = snapshotList[operationLevel].FaceLeaves;
            controller.FaceLayer.State = FacecellTreeState.Undoing;


            controller.RenderController.DeleteAll();
            foreach (Face f in controller.FaceLayer.CurrentLeaves)
            {
                controller.RenderController.New(f);
            }

            controller.RenderController.UpdateAll();

            // 修改操作层数
            operationLevel--;
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            //throw NotImplementedException;
            System.Windows.MessageBox.Show("Redo is nto implementedException");
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
