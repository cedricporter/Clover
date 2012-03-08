using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        List<Edge> newEdges = null;
        
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
        public List<Edge> NewEdges
        {
            get { return newEdges; }
            set { newEdges = value; }
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
}
