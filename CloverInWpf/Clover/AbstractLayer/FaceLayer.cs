using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;

namespace Clover
{
    public enum FacecellTreeState
    {
        Normal,     /// 正常模式，Leaves返回真正的叶节点
        Undoing     /// 撤销模式中，Leaves返回currentLeaves
    }

    /// <summary>
    /// 面树，保存着所有的面，包括历史上存在过的面
    /// </summary>
    public class FacecellTree
    {
        #region get/set
        /// <summary>
        /// 返回所有叶节点，每次自动更新，如果要更高的运行效率，可以拆成另外两个函数
        /// </summary>
        public List<Face> Leaves
        {
            get
            {
                switch ( currentState )
                {
                    case FacecellTreeState.Normal:
                        return leaves;
                    case FacecellTreeState.Undoing:
                        return currentLeaves;
                    default:
                        return leaves;
                }
            }
        }

        /// <summary>
        /// 真正的叶节点
        /// </summary>
        public List<Face> RealLeaves
        {
            get { /*Update();*/ return leaves; }
        }
        public Clover.Face Root
        {
            get { return root; }
        }
        public List<Face> CurrentLeaves
        {
            get { return currentLeaves; }
            set { currentLeaves = value; }
        }
        public Clover.FacecellTreeState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
        #endregion

        Face root;
        List<Face> leaves = new List<Face>();
        List<Face> currentLeaves = new List<Face>();

        /// 当前状态，
        FacecellTreeState currentState;

        public FacecellTree( Face r )
        {
            root = r;
        }

        /// <summary>
        /// 更新叶节点
        /// </summary>
        /// <param name="oldFace">改变了的节点</param>
        /// <remarks>如果输入一个面，则将这个面移除并将他的两个孩子加到叶节点表。否则重建整棵树。</remarks>
        public void UpdateLeaves( Face oldFace = null )
        {
            if ( oldFace == null )
            {
                leaves.Clear();
                Travel( root );
                return;
            }

            Debug.Assert( oldFace.LeftChild != null && oldFace.RightChild != null );
            leaves.Remove( oldFace );
            leaves.Add( oldFace.LeftChild );
            leaves.Add( oldFace.RightChild );
        }

        bool IsLeave( Face face )
        {
            return face.LeftChild == null && face.RightChild == null;
        }

        /// <summary>
        /// 后续遍历
        /// </summary>
        /// <param name="r">根节点</param>
        /// <remarks>请在调用Travel前清空leaves！</remarks>
        void Travel( Face r )
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
    /// group查询表，可以得到所有的group。
    /// </summary>
    public class FaceGroupLookupTable : ICloneable
    {
        List<FaceGroup> faceGroupList = new List<FaceGroup>();

        #region get/set
        public List<FaceGroup> FaceGroupList
        { get { return faceGroupList; } }
        #endregion

        /// <summary>
        /// 构造第一个面的时候初始化。
        /// </summary>
        /// <param name="f"></param>
        public FaceGroupLookupTable( Face f )
        {
            FaceGroup g = new FaceGroup( f );
            faceGroupList.Add( g );
        }


        /// <summary>
        /// 得到面所在的分组,如果获取失败返回null,当获取失败时检查一下是否忘记add了。
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public FaceGroup GetGroup( Face f )
        {
            foreach ( FaceGroup fg in faceGroupList )
            {
                if ( fg.GetFaceList().Contains( f ) )
                    return fg;
            }
            return null;
        }



        /// <summary>
        /// 往lookuptable中加入face，会自动匹配加入group中或新增group。
        /// 否则强制新建一个group
        /// </summary>
        /// <param name="f"></param>
        void AddFace( Face f )
        {
            foreach ( FaceGroup fg in faceGroupList )
            {
                if ( fg.HasFace( f ) )
                {
                    return;
                }
                if ( fg.IsMatch( f ) )
                {
                    fg.AddFace( f );
                    return;
                }
            }
            FaceGroup newfgt = new FaceGroup( f );
            faceGroupList.Add( newfgt );
        }



        /// <summary>
        /// 删除looktable的某个face，不存在则会失败返回false
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        bool RemoveFace( Face f )
        {
            foreach ( FaceGroup group in faceGroupList )
            {
                if ( group.HasFace( f ) )
                {
                    group.RemoveFace( f );
                    if ( group.Count == 0 )
                        faceGroupList.Remove( group );
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 重新给group里面的东西排序
        /// </summary>
        public void SortGroup()
        {
            foreach ( FaceGroup group in faceGroupList )
            {
                group.SortFace();
            }
        }

        /// <summary>
        /// 当有可能一个group的face不再属于同一个group时候调用，强制更新。
        /// </summary>
        public void UpdateGroup()
        {
            for ( int i = 0; i < faceGroupList.Count; i++ )
            {
                for ( int j = 0; j < faceGroupList[ i ].GetFaceList().Count; j++ )
                {
                    if ( !faceGroupList[ i ].IsMatch( faceGroupList[ i ].GetFaceList()[ j ] ) )
                    {
                        Face f = faceGroupList[ i ].GetFaceList()[ j ];
                        faceGroupList[ i ].RemoveFace( f );
                        AddFace( f );
                    }
                }
            }

        }

        public object Clone()
        {
            FaceGroupLookupTable newtables = new FaceGroupLookupTable( null );
            newtables.faceGroupList.Clear();
            foreach ( FaceGroup fg in faceGroupList )
            {
                foreach ( Face f in fg.GetFaceList() )
                {
                    newtables.AddFace( f );
                }
            }
            return newtables;
        }



        /// <summary>
        /// 刷新table中的列表，删除空的组
        /// </summary>
        private void RemoveRedundantFaceGroup()
        {
            for ( int i = 0; i < faceGroupList.Count; i++ )
            {
                if ( faceGroupList[ i ].GetFaceList().Count == 0 )
                {
                    faceGroupList.RemoveAt( i );
                }
            }

        }

        ///// <summary>
        ///// 更新lookuptable
        ///// </summary>
        //void UpdateLookupTable()
        //{
        //    UpdateGroup();
        //    //RemoveRedundantFaceGroup();

        //}


        ///// <summary>
        ///// 检索group中的组有没有非常靠近的面
        ///// </summary>
        ///// <param name="atuofoldinfolist">保存着可以可能会自动贴合的group对</param>
        ///// <param name="threshold">用弧度表示的角度</param>
        ///// <returns></returns>
        //public bool CheckForAutoFoldUp( ref List<FoldUpInfo> atuofoldinfolist, double threshold = 0.174 )
        //{

        //    foreach ( FaceGroup fgfix in tables )
        //    {
        //        foreach ( FaceGroup fgmove in tables )
        //        {
        //            if ( fgfix != fgmove )
        //            {
        //                double ang = CloverMath.CalculatePlaneAngle( fgfix.GetFaceList()[ 0 ], fgmove.GetFaceList()[ 0 ] );
        //                if ( ang  < threshold )
        //                {
        //                    bool IsContain = false;
        //                    foreach ( FoldUpInfo info in atuofoldinfolist )
        //                    {
        //                        if (info.fgFix == fgfix && info.fgMov == fgmove ||
        //                            info.fgMov == fgfix && info.fgFix == fgmove 
        //                            )
        //                        {
        //                            IsContain = true;
        //                            break;
        //                        }
        //                    }

        //                    if ( IsContain )
        //                    {
        //                        continue;
        //                    }
        //                    FoldUpInfo foldupinfo = new FoldUpInfo();
        //                    foldupinfo.fgFix = fgfix;
        //                    foldupinfo.fgMov = fgmove;
        //                    foldupinfo.angle = ang;

        //                    foreach ( Vertex ver in fgfix.GetFaceList()[ 0 ].Vertices )
        //                    {
        //                        if ((ver.X * fgfix.A + ver.Y * fgfix.B + ver.Z * fgfix.C + fgfix.D) > 0)
        //                        {
        //                            foldupinfo.IsOver = true;
        //                            break;
        //                        }

        //                        if ( ( ver.X * fgfix.A + ver.Y * fgfix.B + ver.Z * fgfix.C + fgfix.D ) < 0 )
        //                        {
        //                            foldupinfo.IsOver = false;
        //                            break;
        //                        }

        //                    }
        //                    atuofoldinfolist.Add( foldupinfo );
        //                }
        //            }
        //        }
        //    }

        //    if ( atuofoldinfolist.Count > 0 )
        //    {
        //        return true;
        //    }
        //    return false;
        //}



        ///// <summary>
        ///// 当foldup时调用，以便group排序;注意，一定要先调用后才可以updatelookuptable;
        ///// </summary>
        ///// <param name="foldupinfo"></param>
        //public void FoldUp(FoldUpInfo foldupinfo )
        //{

        //    if ( foldupinfo.IsOver )
        //    {
        //        int layer = 0;
        //        for ( int i = 0; i < foldupinfo.fgFix.GetFaceList().Count; i++ )
        //        {
        //            foldupinfo.fgFix.GetFaceList()[ i ].Layer = layer;
        //            layer++;
        //        }
        //        // 根据是否覆盖来调整layer的值
        //        for ( int i = foldupinfo.fgFix.GetFaceList().Count - 1; i >= 0; i-- )
        //        {

        //            if ( !CloverMath.IsIntersectionOfTwoFace( foldupinfo.fgMov.GetFaceList()[ foldupinfo.fgMov.GetFaceList().Count - 1 ], foldupinfo.fgFix.GetFaceList()[ i ] ) )
        //            {
        //                layer--;
        //            }
        //        }

        //        for ( int i = foldupinfo.fgMov.GetFaceList().Count - 1; i >= 0; i-- )
        //        {

        //            foldupinfo.fgMov.GetFaceList()[ i ].Layer = layer;
        //            layer++;
        //            foldupinfo.fgFix.AddFace( foldupinfo.fgMov.GetFaceList()[ i ] );
        //        }
        //        RemoveRedundantFaceGroup();

        //    }
        //    else
        //    {
        //        int layer = 0;
        //        for ( int i = 0; i < foldupinfo.fgFix.GetFaceList().Count; i++ )
        //        {
        //            foldupinfo.fgFix.GetFaceList()[ i ].Layer = layer;
        //            layer++;
        //        }
        //        layer = foldupinfo.fgFix.GetBottomLayer();
        //        layer--;
        //        for ( int i = 0; i < foldupinfo.fgFix.GetFaceList().Count; i++ )
        //        {
        //            if ( !CloverMath.IsIntersectionOfTwoFace( foldupinfo.fgMov.GetFaceList()[ 0 ], foldupinfo.fgFix.GetFaceList()[ i ] ) )
        //            {
        //                layer++;
        //            }
        //        }

        //        for ( int i = 0; i < foldupinfo.fgMov.GetFaceList().Count; i++ )
        //        {
        //            foldupinfo.fgMov.GetFaceList()[ i ].Layer = layer;
        //            foldupinfo.fgFix.AddFace( foldupinfo.fgMov.GetFaceList()[ i ] );
        //            layer--;
        //        }
        //        RemoveRedundantFaceGroup();
        //    }
        //}




        bool AddGroup( FaceGroup fg )
        {
            foreach ( FaceGroup fgin in faceGroupList )
            {
                if ( CloverMath.IsTwoVectorTheSameDir( fg.Normal, fgin.Normal ) )
                {
                    return false;
                }
            }
            faceGroupList.Add( fg );
            return true;
        }


        public List<Face> GetFaceExcludeGroupFoundByFace(Face f)
        {
            List<Face> result = new List<Face>();
            FaceGroup excludefg = GetGroup( f );
            foreach (FaceGroup fg in faceGroupList)
            {
                if(fg != excludefg)
                {
                    foreach (Face face in fg.GetFaceList())
                    {
                        result.Add( f );
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// foldup后对lookuptable进行更新和排序
        /// </summary>
        /// <param name="IsDefaultDir">是否按照默认方向折叠，默认方向是面向用户，即摄像机的方向</param>
        /// <returns></returns>
        public bool UpdateTableAfterFoldUp( bool IsDefaultDir = true )
        {

            // 找cutface
            List<Face> cutedFace = new List<Face>(); // 记录被cut的faces
            FaceGroup fixedFaceGroup = null; // 用于记录旋转前后位置不变的faces
            FaceGroup movedFaceGroup = null;// 用于记录旋转前后位置变化的faces
            List<Face> listNewFacesList = new List<Face>(); // 用于记录cut后新生的新faces

            FaceGroup participatedGroup = null;// 记录参与折叠的group

            // 检测操作了哪个group
            foreach ( FaceGroup fg in faceGroupList )
            {
                foreach ( Face f in fg.GetFaceList() )
                {
                    if ( f.LeftChild != null && f.RightChild != null )
                    {
                        cutedFace.Add( f );

                        if ( participatedGroup == null )
                        {
                            participatedGroup = fg;
                        }

                        if ( participatedGroup != fg )
                        {
                            // 一次foldup只能对一个group中的面进行操作
                            return false;

                        }

                    }
                }
            }


            //  没有发现折叠的face, 一定是只移动了面
            if ( cutedFace.Count == 0 )
            {
                foreach ( FaceGroup fg in faceGroupList )
                {
                    foreach ( Face f in fg.GetFaceList() )
                    {
                        if ( f.LeftChild != null && f.RightChild != null )
                        {
                            fg.RemoveFace( f );
                            fg.AddFace( f.LeftChild );
                            fg.AddFace( f.RightChild );
                        }
                    }
                }
            }



            // 查找fixed face和moved face：

            // 抽取直接有关联的fixed和moved的faces
            for ( int i = 0; i < cutedFace.Count; i++ )
            {
                Face fleftchild = cutedFace[ i ].LeftChild;
                Face frightchild = cutedFace[ i ].RightChild;

                if ( CloverMath.IsTwoVectorTheSameDir( fleftchild.Normal, cutedFace[ i ].Normal, true ) )
                {
                    if ( fixedFaceGroup == null )
                    {
                        fixedFaceGroup = new FaceGroup( fleftchild );
                    }
                    else
                        fixedFaceGroup.AddFace( fleftchild );
                }
                else
                {
                    if ( movedFaceGroup == null )
                    {
                        movedFaceGroup = new FaceGroup( fleftchild );
                    }
                    else
                        movedFaceGroup.AddFace( fleftchild );
                }

                if ( CloverMath.IsTwoVectorTheSameDir( frightchild.Normal, cutedFace[ i ].Normal, true ) )
                {
                    if ( fixedFaceGroup == null )
                    {
                        fixedFaceGroup = new FaceGroup( frightchild );
                    }
                    else
                        fixedFaceGroup.AddFace( frightchild );
                }
                else
                {
                    if ( movedFaceGroup == null )
                    {
                        movedFaceGroup = new FaceGroup( frightchild );
                    }
                    else
                        movedFaceGroup.AddFace( frightchild );
                }


                listNewFacesList.Add( fleftchild );
                listNewFacesList.Add( frightchild );
            }

            foreach ( Face f in participatedGroup.GetFaceList() )
            {
                if ( f.LeftChild == null && f.RightChild == null )
                {
                    listNewFacesList.Add( f );
                }

            }

            // fold至少cut一个face，那么一定会有moveface
            if ( movedFaceGroup == null )
            {
                foreach ( Face f in cutedFace )
                {
                    RemoveFace( f );
                    if ( f.LeftChild != null )
                        AddFace( f.LeftChild );
                    if ( f.RightChild != null )
                        AddFace( f.RightChild );

                }
                return false;
            }


            // 发现不是foldup操作，直接返回，并且对group进行更新
            if ( !CloverMath.IsTwoVectorTheSameDir( movedFaceGroup.Normal, fixedFaceGroup.Normal ) )
            {

                foreach ( Face f in cutedFace )
                {
                    RemoveFace( f );
                    if ( f.LeftChild != null )
                        AddFace( f.LeftChild );
                    if ( f.RightChild != null )
                        AddFace( f.RightChild );
                }
                return false;
            }

            if ( IsDefaultDir )
            {
                int layer = 0;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    layer++;
                }
                // 根据是否覆盖来调整layer的值
                for ( int i = fixedFaceGroup.GetFaceList().Count - 1; i >= 0; i-- )
                {

                    if ( !CloverMath.IsIntersectionOfTwoFace( movedFaceGroup.GetFaceList()[ movedFaceGroup.GetFaceList().Count - 1 ], fixedFaceGroup.GetFaceList()[ i ] ) )
                    {
                        layer--;
                    }
                }

                for ( int i = movedFaceGroup.GetFaceList().Count - 1; i >= 0; i-- )
                {

                    movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    layer++;
                    fixedFaceGroup.AddFace( movedFaceGroup.GetFaceList()[ i ] );
                }
                RemoveRedundantFaceGroup();

            }
            else
            {
                int layer = 0;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    fixedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    layer++;
                }
                layer = fixedFaceGroup.GetBottomLayer();
                layer--;
                for ( int i = 0; i < fixedFaceGroup.GetFaceList().Count; i++ )
                {
                    if ( !CloverMath.IsIntersectionOfTwoFace( movedFaceGroup.GetFaceList()[ 0 ], fixedFaceGroup.GetFaceList()[ i ] ) )
                    {
                        layer++;
                    }
                }

                for ( int i = 0; i < movedFaceGroup.GetFaceList().Count; i++ )
                {
                    movedFaceGroup.GetFaceList()[ i ].Layer = layer;
                    fixedFaceGroup.AddFace( movedFaceGroup.GetFaceList()[ i ] );
                    layer--;
                }
                RemoveRedundantFaceGroup();
            }

            AddGroup( fixedFaceGroup );
            UpdateGroup();
            return true;
        }

    }



    /// <summary>
    /// 面层的抽象
    /// </summary>
    public class FaceLayer
    {
        #region get/set
        public Clover.Face Root
        {
            get { return root; }
            set { root = value; }
        }
        public Clover.FacecellTree FacecellTree
        {
            get { return facecellTree; }
        }
        public List<Face> Leaves
        {
            get { return facecellTree.Leaves; }
        }
        public List<Face> CurrentLeaves
        {
            get { return facecellTree.CurrentLeaves; }
            set
            {
                facecellTree.CurrentLeaves.Clear();
                foreach ( Face f in value )
                {
                    facecellTree.CurrentLeaves.Add( f );
                }
            }
        }
        public FacecellTreeState State
        {
            get { return facecellTree.CurrentState; }
            set { facecellTree.CurrentState = value; }
        }
        #endregion

        FacecellTree facecellTree;
        FaceGroupLookupTable lookupTable;
        CloverController controller;
        Face root;

        public FaceLayer()
        {
        }

        public void Initliaze( Face root )
        {
            this.root = root;
            root.UpdateVertices();

            facecellTree = new FacecellTree( root );
            lookupTable = new FaceGroupLookupTable( root );
            this.controller = CloverController.GetInstance();
        }

        /// <summary>
        /// 更新面的查询表，查询表里面将同一个平面的面组合在一个组里面。
        /// </summary>
        void UpdateLoopupTable()
        {
            // bla bla... 更新索引，到时再说

        }


        public void UpdateLeaves( Face oldFace = null )
        {
            facecellTree.UpdateLeaves( oldFace );
        }

        /// <summary>
        /// 这里有问题，要修改
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="rotDegree"></param>
        public void Update( List<Edge> edges, double rotDegree )
        {
            // bla bla


            UpdateLoopupTable();
        }

    }

}
