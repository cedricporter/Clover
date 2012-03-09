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
        SnapshotNodeKind type;
        List<Face> faceLeaves = new List<Face>();
        List<Edge> newEdges = null;
        List<Vertex> movedVertexList = new List<Vertex>();
        
        #region get/set
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
