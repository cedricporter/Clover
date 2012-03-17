using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    /// <summary>
    /// 快照节点的类型
    /// </summary>
    public enum SnapshotNodeKind
    {
        Invalid,
        RotateKind,
        CutKind
    }

    /// <summary>
    /// 快照节点
    /// </summary>
    public class SnapshotNode
    {
        SnapshotNodeKind type;                                  /// 节点类型
        List<Face> faceLeaves = new List<Face>();               /// 当前快照时候的叶子节点
        List<Edge> newEdges = new List<Edge>();                 /// 新增加的边，仅在CutFaces时候设置
        List<Vertex> movedVertexList = new List<Vertex>();      /// 被移动的顶点，仅在RotateFaces的时候设置
        int originVertexListCount = -1;                         /// 当前快照的顶点的表的长度
        int originEdgeListCount = -1;                           /// 当前快照的边的表的长度
        FaceGroupLookupTable faceGroupLookupTable;              /// group快照，先整个备份下来，以后再改成增量备份                                                                
        Dictionary<int, int> faceIDMap = new Dictionary<int, int>();/// 用于还原面的layer                                                                
        
        #region get/set
        public Dictionary<int, int> FaceIDMap
        {
            get { return faceIDMap; }
            set { faceIDMap = value; }
        }
        public Clover.FaceGroupLookupTable FaceGroupLookupTable
        {
            get { return faceGroupLookupTable; }
            set { faceGroupLookupTable = value; }
        }
        public int OriginEdgeListCount
        {
            get { return originEdgeListCount; }
            set { originEdgeListCount = value; }
        }
        public int OriginVertexListCount
        {
            get { return originVertexListCount; }
            set { originVertexListCount = value; }
        }
        public List<Vertex> MovedVertexList
        {
            get { return movedVertexList; }
            set { movedVertexList = value; }
        }
        public Clover.SnapshotNodeKind Type
        {
            get { return type; }
            set { type = value; }
        }
        public List<Face> FaceLeaves
        {
            get { return faceLeaves; }
            set { faceLeaves = value; }
        }
        public List<Edge> NewEdges
        {
            get { return newEdges; }
            set { newEdges = value; }
        }
        #endregion

        #region API
        public SnapshotNode()
        {
            type = SnapshotNodeKind.Invalid;
        }
        public SnapshotNode(List<Face> leaves)
        {
            foreach (Face f in leaves)
            {
                faceLeaves.Add(f);
            }
            type = SnapshotNodeKind.Invalid;
        }

        public void Add(Face face)
        {
            faceLeaves.Add(face);
        }
        #endregion

    }
}
