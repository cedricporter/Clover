using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;

namespace Clover
{
    /// <summary>
    /// 面树，保存着所有的面，包括历史上存在过的面
    /// </summary>
    public class FacecellTree
    {
        #region get/set
        public Clover.Face Root
        {
            get { return root; }
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
        public void UpdateLeaves(Face oldFace = null)
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
    public class LookupTable
    {
        List<FaceGroup> tables = new List<FaceGroup>();
        
        #region get/set
        public List<FaceGroup> Tables
        { get { return tables; } }
        #endregion

        /// <summary>
        /// 构造第一个面的时候初始化。
        /// </summary>
        /// <param name="f"></param>
        public LookupTable(Face f)
        {
            FaceGroup g = new FaceGroup( f );
            tables.Add( g );
        }


        /// <summary>
        /// 得到面所在的分组，如果错误返回null。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public FaceGroup GetGroup(Face f)
        {
            foreach (FaceGroup fg in tables)
            {
                foreach (Face face in fg.GetGroup())
                {
                    if (face == f)
                    {
                        return fg;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 将一个face增加到table中，table会自动生成或者插入到合适的分组中
        /// </summary>
        /// <param name="f"></param>
        public void AddFace(Face f)
        {
            foreach (FaceGroup fg in tables)
            {
                if ( fg.IsMatch(f))
                {
                    fg.AddFace( f );
                    return;
                }
            }
            FaceGroup newfg = new FaceGroup( f );
            tables.Add( newfg );
        }

        /// <summary>
        /// 刷新table中的列表，删除空的组
        /// </summary>
        void UpdateTable()
        {
            foreach (FaceGroup fg in tables)
            {
                if (fg.GetGroup().Count == 0)
                {
                    tables.Remove( fg );
                }
            }
        }

        /// <summary>
        /// 自动叠合的时候调用。
        /// </summary>
        /// <param name="fg1">合并后的目标</param>
        /// <param name="fg2">合并的源</param>
        /// <param name="IsOver">fg2 是否位于 fg1的上面</param>
        /// <param name="op">目前的折叠操作类型</param>
        public void Autofold(FaceGroup fg1, FaceGroup fg2, bool IsOver,FoldingOp op )
        {
            switch(op)
            {
                case FoldingOp.Blend:
                case FoldingOp.FoldUp:

                    if ( IsOver )
                    {
                        int layer = 0;
                        for ( int i = 0; i < fg1.GetGroup().Count; i++ )
                        {
                            fg1.GetGroup()[ i ].Layer = layer;
                            layer++;
                        }
                        for ( int i = fg2.GetGroup().Count - 1; i >= 0; i-- )
                        {
                            fg2.GetGroup()[ i ].Layer = layer;
                            layer++;
                            fg1.AddFace( fg2.GetGroup()[ i ] );
                        }
                        UpdateTable();
                        
                    }
                    else
                    {
                        int layer = 0;
                        for ( int i = 0; i < fg1.GetGroup().Count; i++ )
                        {
                            fg1.GetGroup()[ i ].Layer = layer;
                            layer++;
                        }
                        layer = fg1.GetBottomLayer();
                        layer--;
                        for ( int i = 0; i < fg2.GetGroup().Count; i++ )
                        {
                            fg2.GetGroup()[ i ].Layer = layer;
                            fg1.AddFace( fg2.GetGroup()[ i ] );
                            layer--;
                        }
                        UpdateTable();
                    }
                    break;

                case FoldingOp.TuckIn:
                    break;
            }
        }
    }



    /// <summary>
    /// 面层的抽象
    /// </summary>
    public class FaceLayer
    {
        #region get/set
        public Clover.FacecellTree FacecellTree
        {
            get { return facecellTree; }
        }
        public List<Face> Leaves
        { 
            get { return facecellTree.Leaves; }
        }
        #endregion

        FacecellTree facecellTree;
        LookupTable lookupTable;
        CloverController controller;

        public FaceLayer(CloverController controller)
        {
            this.controller = controller;
        }

        public void Initliaze(Face root)
        {
            root.UpdateVertices();
            
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


        public void UpdateLeaves(Face oldFace = null)
        {
            facecellTree.UpdateLeaves(oldFace);
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
