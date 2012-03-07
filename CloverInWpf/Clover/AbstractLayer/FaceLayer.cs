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
        /// 得到面所在的分组,如果获取失败返回null,当获取失败时检查一下是否忘记add了。
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
        /// 往lookuptable中加入face，会自动匹配加入group中或新增group。
        /// 否则强制新建一个group
        /// </summary>
        /// <param name="f"></param>
        public void AddFace(Face f)
        {

            foreach (FaceGroup fg in tables)
            {
                if ( fg.IsMatch(f) )
                {
                    fg.AddFace( f );
                    return;
                }
            }
            FaceGroup newfgt = new FaceGroup( f );
            tables.Add( newfgt );
        }
        
        

        /// <summary>
        /// 删除looktable的某个face，不存在则会失败返回false
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool DeleteFace(Face f)
        {

            for ( int i = 0; i < tables.Count; i++ )
            {
                if (tables[i].DeleteFace(f))
                {
                    UpdateTable();
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// 当有可能一个group的face不再属于同一个group时候调用，强制更新。
        /// </summary>
        private void UpdateGroup()
        {

            for ( int i = 0; i < tables.Count; i++ )
            {
                for ( int j = 0; j < tables[ i ].GetGroup().Count; j++ )
                {
                    if ( !tables[ i ].IsMatch( tables[ i ].GetGroup()[j] ) )
                    {
                        Face f = tables[ i ].GetGroup()[ j ];
                        tables[ i ].DeleteFace( f );
                        AddFace( f );
                    }
                }
            }
           
        }


        /// <summary>
        /// 刷新table中的列表，删除空的组
        /// </summary>
        private void UpdateTable()
        {
            for ( int i = 0; i < tables.Count; i++ )
            {
                if ( tables[i].GetGroup().Count == 0 )
                {
                    tables.Remove( tables[ i ] );
                }
            }
            
        }

        /// <summary>
        /// 更新lookuptable
        /// </summary>
        public void UpdateLookupTable()
        {
            UpdateGroup();
            UpdateTable();

        }


        /// <summary>
        /// 检索group中的组有没有非常靠近的面
        /// </summary>
        /// <param name="atuofoldinfolist">保存着可以可能会自动贴合的group对</param>
        /// <param name="threshold">用弧度表示的角度</param>
        /// <returns></returns>
        public bool CheckForAutoFoldUp( ref List<AutoFoldInfo> atuofoldinfolist, double threshold = 0.174 )
        {
           
            foreach ( FaceGroup fgfix in tables )
            {
                foreach ( FaceGroup fgmove in tables )
                {
                    if ( fgfix != fgmove )
                    {
                        double ang = CloverMath.CalculatePlaneAngle( fgfix.GetGroup()[0], fgmove.GetGroup()[0] );
                        if ( ang  < threshold )
                        {
                            AutoFoldInfo autofoldinfo = new AutoFoldInfo();
                            autofoldinfo.fgFix = fgfix;
                            autofoldinfo.fgMov = fgmove;
                            autofoldinfo.angle = ang;
                            atuofoldinfolist.Add( autofoldinfo );
                        }
                    }
                }
            }

            if ( atuofoldinfolist.Count > 0 )
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 自动叠合的时候调用。
        /// </summary>
        /// <param name="fg1">合并后的目标</param>
        /// <param name="fg2">合并的源</param>
        /// <param name="IsOver">fg2 是否位于 fg1的上面</param>
        public void AutofoldUp(FaceGroup fg1, FaceGroup fg2, bool IsOver )
        {

            if ( IsOver )
            {
                int layer = 0;
                for ( int i = 0; i < fg1.GetGroup().Count; i++ )
                {
                    fg1.GetGroup()[ i ].Layer = layer;
                    layer++;
                }
                // 根据是否覆盖来调整layer的值
                for ( int i = fg1.GetGroup().Count - 1; i >= 0; i-- )
                {

                    if ( !CloverMath.IsIntersectionOfTwoFace( fg2.GetGroup()[ fg2.GetGroup().Count - 1 ], fg1.GetGroup()[ i ] ) )
                    {
                        layer--;
                    }
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
                for ( int i = 0; i < fg1.GetGroup().Count; i++ )
                {
                    if ( !CloverMath.IsIntersectionOfTwoFace( fg2.GetGroup()[ 0 ], fg1.GetGroup()[ i ] ) )
                    {
                        layer++;
                    }
                }

                for ( int i = 0; i < fg2.GetGroup().Count; i++ )
                {
                    fg2.GetGroup()[ i ].Layer = layer;
                    fg1.AddFace( fg2.GetGroup()[ i ] );
                    layer--;
                }
                UpdateTable();
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
