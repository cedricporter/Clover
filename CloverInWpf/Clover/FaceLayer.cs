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
        Face root;
        public Clover.Face Root
        {
            get { return root; }
            set { root = value; }
        }
        List<Face> leaves;

        /// <summary>
        /// 返回所有叶节点，每次自动更新，如果要更高的运行效率，可以拆成另外两个函数
        /// </summary>
        public List<Face> Leaves
        {
            get
            {
                Update();
                return leaves;
            }
        }

        /// <summary>
        /// 更新叶节点
        /// </summary>
        void Update()
        {
            leaves.Clear();
            Travel( root );
        }

        bool IsLeave(Face face)
        {
            if ( face.LeftChild == null && face.RightChild == null )
                return true;
            return false;
        }

        void Travel(Face r)
        {
            if ( r == null )
                return;

            Travel( r.LeftChild );
            Travel( r.RightChild );

            if ( IsLeave( r ) )
                leaves.Add( r );
        }
    }

    /// <summary>
    /// 面查询表，将一个平面上的面放在一个group里面，方便多面折叠
    /// </summary>
    class LookupTable
    {
        List<List<Face>> tables;

        public List<List<Face>> Tables
        {
            get
            {
                return tables;
            }
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
