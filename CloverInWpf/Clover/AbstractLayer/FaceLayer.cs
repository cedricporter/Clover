using System;
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
        public void Update()
        {
            leaves.Clear();
            Travel(root);
        }

        bool IsLeave(Face face)
        {
            return face.LeftChild == null && face.RightChild == null;
        }

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

        public List<List<Face>> Tables
        { get { return tables; } }

        public int AddGroup(Face face)
        {
            List<Face> fl = new List<Face>();
            fl.Add(face);
            tables.Add(fl);
            return tables.Count - 1;
        }

    }



    /// <summary>
    /// 面层的抽象，相当于控制器。
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
            lookupTable = new LookupTable();
        }

        /// <summary>
        /// 更新面的查询表，查询表里面将同一个平面的面组合在一个组里面。
        /// </summary>
        void UpdateLoopupTable()
        {
            // bla bla... 更新索引，到时再说

        }

        public void Update(List<Edge> edges, double rotDegree)
        {
            // bla bla


            UpdateLoopupTable();
        }

    }

}
