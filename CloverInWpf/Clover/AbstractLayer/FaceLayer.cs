using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    /// <summary>
    /// 面树，保存着所有的面，包括历史上存在过的面
    /// </summary>
    class FacecellTree
    {
        #region get/set
        public Clover.Face Root
        {
            get { return root; }
            set { root = value; }
        }
        #endregion

        Face root;
        List<Face> leaves = new List<Face>();

        public FacecellTree(Face r)
        {
            root = r;
        }

        /// <summary>
        /// 返回所有叶节点，每次自动更新，如果要更高的运行效率，可以拆成另外两个函数
        /// </summary>
        public List<Face> Leaves
        {
            get { /*Update();*/ return leaves; }
        }

        /// <summary>
        /// 更新叶节点
        /// </summary>
        /// <param name="oldFace">改变了的节点</param>
        /// <remarks>如果输入一个面，则将这个面移除并将他的两个孩子加到叶节点表。否则重建整棵树。</remarks>
        public void Update(Face oldFace = null)
        {
            if (oldFace == null)
            {
                leaves.Clear();
                Travel(root);
                return;
            }

            Debug.Assert(oldFace.LeftChild != null && oldFace.RightChild != null);
            leaves.Remove(oldFace);
            leaves.Add(oldFace.LeftChild);
            leaves.Add(oldFace.RightChild);
        }

        bool IsLeave(Face face)
        {
            return face.LeftChild == null && face.RightChild == null;
        }

        /// <summary>
        /// 后续遍历
        /// </summary>
        /// <param name="r">根节点</param>
        /// <remarks>请在调用Travel前清空leaves！</remarks>
        void Travel(Face r)
        {
            if (r == null)
                return;

            Travel(r.LeftChild);
            Travel(r.RightChild);

            if (IsLeave(r))
                leaves.Add(r);
        }
    }

    /// <summary>
    /// 面查询表，将一个平面上的面放在一个group里面，方便多面折叠
    /// </summary>
    class LookupTable
    {
        List<List<Face>> tables = new List<List<Face>>();
        Face root;

        #region get/set
        public List<List<Face>> Tables
        { get { return tables; } }
        #endregion

        public LookupTable(Face root)
        {
            this.root = root;
        }

        public int AddGroup(Face face)
        {
            List<Face> fl = new List<Face>();
            fl.Add(face);
            tables.Add(fl);
            return tables.Count - 1;
        }

    }



    /// <summary>
    /// 面层的抽象
    /// </summary>
    class FaceLayer
    {
        FacecellTree facecellTree;
        LookupTable lookupTable;
        CloverController controller;

        public FaceLayer(CloverController controller)
        {
            this.controller = controller;
        }

        public void Initliaze(Face root)
        {
            facecellTree = new FacecellTree(root);
            lookupTable = new LookupTable(root);
        }

        /// <summary>
        /// 更新面的查询表，查询表里面将同一个平面的面组合在一个组里面。
        /// </summary>
        void UpdateLoopupTable()
        {
            // bla bla... 更新索引，到时再说

        }

        /// <summary>
        /// 这里有问题，要修改
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="rotDegree"></param>
        public void Update(List<Edge> edges, double rotDegree)
        {
            // bla bla


            UpdateLoopupTable();
        }

    }

}
