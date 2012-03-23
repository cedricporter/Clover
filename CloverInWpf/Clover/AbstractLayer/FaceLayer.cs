using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;
using System.Windows.Media.Media3D;
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
        BendTpye bendtype;
        FaceGroup bendingParticipateGroup = null;
        bool IsBasicBendedFaceTheSameNormalWithItsGroup = false;

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
                // 判断组里面是否已经有了这样的组
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
        /// 在foldup后更lookuptable
        /// </summary>
        /// <param name="participatedGroup">参与折叠的面</param>
        /// <param name="movedFaceGroup">所有移动的面</param>
        /// <param name="fixedFaceGroup">所有不动的面</param>
        /// <param name="IsFacingUser">是否组的法线面向用户</param>
        /// <returns></returns>
        public bool UpdateTableAfterFoldUp( List<Face> participatedFaces, List<Face> movedFaces, List<Face> fixedFaces, bool IsFacingUser = true )
        {
            RemoveRedundantFaceGroup();
            FaceGroup participatedGroup = GetGroup( participatedFaces[ 0 ] );
            FaceGroup movedFaceGroup = new FaceGroup( movedFaces[ 0 ] );
            FaceGroup fixedFaceGroup = new FaceGroup( fixedFaces[ 0 ] );

            foreach ( Face f in movedFaces )
            {
                if ( !movedFaceGroup.HasFace( f ) )
                {
                    movedFaceGroup.AddFace( f );
                }
            }

            foreach ( Face f in fixedFaces )
            {
                if ( !fixedFaceGroup.HasFace( f ) )
                {
                    fixedFaceGroup.AddFace( f );
                }
            }

            foreach ( Face fp in participatedGroup.GetFaceList() )
            {
                if ( fp.LeftChild == null && fp.RightChild == null )
                {
                    if ( !movedFaceGroup.HasFace( fp ) && !fixedFaceGroup.HasFace( fp ) )
                    {
                        fixedFaceGroup.AddFace( fp );
                    }
                }
            }

            fixedFaceGroup.SortFace();
            fixedFaceGroup.Normal = participatedGroup.Normal;
            return participatedGroup.UpdateGroupAfterFoldUp( participatedGroup, movedFaceGroup, fixedFaceGroup, IsFacingUser );
        }


        /// <summary>
        /// 在bend前调用
        /// </summary>
        /// <param name="faces">要bend的所有面</param>
        /// <param name="angle">bend的角度,角度</param>
        public bool BeforeBending(List<Face> faces, double angle)
        {
            if (faces == null || Math.Abs(angle) > 180 )
            {
                return false;
            }
            if ( faces.Count == 0)
            {
                return false;
            }
            if (bendingParticipateGroup != null)
            {
                return false;
            }
            // 建立bending的临时组
            bendingParticipateGroup = new FaceGroup( faces[ 0 ] );
            for ( int i = 1; i < faces.Count; i++ )
            {
                bendingParticipateGroup.AddFace( faces[ i ] );
            }
            bendingParticipateGroup.SortFace();

            bendingParticipateGroup.Normal = bendingParticipateGroup.GetFaceList()[ 0 ].Normal;

            if ( CloverMath.IsTwoVectorTheSameDir( bendingParticipateGroup.Normal, GetGroup( bendingParticipateGroup.GetFaceList()[ 0 ] ).Normal, true ) )
            {
                IsBasicBendedFaceTheSameNormalWithItsGroup = true;
            }

            // 判断折叠样式
            if (Math.Abs(angle) < 0.00001) // 适应误差
            {
                bendtype = BendTpye.BlendZero;
            }
            else if ( Math.Abs( angle ) <= 180 && Math.Abs( angle ) >= 179.999999999999 )// 适应误差
            {
                bendtype = BendTpye.BlendSemiCycle;
            }
            else
            {
                bendtype = BendTpye.BlendNormally;
            }

            return true;
        }


        /// <summary>
        /// bendh后调用来更新lookuptable
        /// </summary>
        /// <returns></returns>
        public bool UpdateTableAfterBending(bool IsFacingUser = true)
        {
            
            if (bendtype == BendTpye.BlendZero)
            {
                return true;
            }

            FaceGroup participateGroup = null;
            // bend半周即180度
            if (bendtype == BendTpye.BlendSemiCycle)
            {
                bendtype = BendTpye.BlendZero;
                // 找到相关的组
                foreach (FaceGroup fg in faceGroupList)
                {
                    if ( CloverMath.IsTwoVectorTheSameDir(fg.Normal, bendingParticipateGroup.Normal))
                    {
                        if (participateGroup == null)
                        {
                            participateGroup = fg;
                        }
                        else if ( participateGroup != fg)
                        {
                            return false;// 一次只可能有一个组参与
                        }
                    }
                }
                faceGroupList.Remove( participateGroup );
                bendingParticipateGroup.RevertFaces();

                foreach (Face f in bendingParticipateGroup.GetFaceList())
                {
                    participateGroup.RemoveFace( f );
                }

                participateGroup.SortFace();
                // 判断用户的方向：
                if ( IsFacingUser ) // 组的正面面向用户
                {
                    // 逆序从上贴合
                    int layer = 0;
                    int lastlayer = 0;
                    layer = participateGroup.GetTopLayer() + 1;
                    // 根据是否覆盖来调整layer的值
                    for ( int i = participateGroup.GetFaceList().Count - 1; i >= 0; i-- )
                    {

                        if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( bendingParticipateGroup.GetFaceList()[ bendingParticipateGroup.GetFaceList().Count - 1 ], participateGroup.GetFaceList()[ i ] ) )
                        {
                            layer--;
                        }
                        else if ( i == participateGroup.GetFaceList().Count - 1 )
                        {
                            break;
                        }
                    }

                    lastlayer = bendingParticipateGroup.GetTopLayer();
                    for ( int i = bendingParticipateGroup.GetFaceList().Count - 1; i >= 0; i-- )
                    {
                        if ( bendingParticipateGroup.GetFaceList()[ i ].Layer == lastlayer)
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        else
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            layer++;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        participateGroup.AddFace( bendingParticipateGroup.GetFaceList()[ i ] );
                    }
                }
                else // 组背向用户
                {
                    int layer = 0;
                    int lastlayer = 0;
                    layer = participateGroup.GetBottomLayer() + 1;
                    for ( int i = 0; i < participateGroup.GetFaceList().Count; i++ )
                    {
                        if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( bendingParticipateGroup.GetFaceList()[ 0 ], participateGroup.GetFaceList()[ i ] ) )
                        {
                            layer++;
                        }
                        else if (i == 0)
                        {
                            break;
                        }

                    }

                    lastlayer = bendingParticipateGroup.GetBottomLayer();
                    for ( int i = 0; i < bendingParticipateGroup.GetFaceList().Count; i++ )
                    {
                        if ( bendingParticipateGroup.GetFaceList()[ i ].Layer == lastlayer )
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        else
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            layer--;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        participateGroup.AddFace( bendingParticipateGroup.GetFaceList()[ i ] );
                    }
                }

                faceGroupList.Add( participateGroup );
            }

            // 非180度的折叠
            if (bendtype == BendTpye.BlendNormally)
            {
                if ( !IsBasicBendedFaceTheSameNormalWithItsGroup ) // 基层新组与原来的组指向大致相同方向
                {
                    bendingParticipateGroup.RevertFaces();
                }
                
                // 寻找是否会和某个组重合
                foreach (FaceGroup fg in faceGroupList)
                {
                    if ( CloverMath.IsTwoVectorTheSameDir( fg.Normal, bendingParticipateGroup.Normal ) )
                    {
                        if ( participateGroup == null )
                        {
                            participateGroup = fg;
                        }
                        else if ( participateGroup != fg )
                        {
                            return false;
                        }
                    }
                }

                if (participateGroup == null)
                {
                    return true; // 很幸运，没有什么组跟你重合
                }

                // 判断自己组的排序方向和对方是不是一样的
                if ( !CloverMath.IsTwoVectorTheSameDir( participateGroup.Normal, bendingParticipateGroup.Normal, true ) )
                {
                    bendingParticipateGroup.RevertFaces();
                }

                if ( IsFacingUser )// 用户面向对方的组
                {
                    // 从上贴合
                    int layer = 0;
                    int lastlayer = 0;
                    // 根据是否覆盖来调整layer的值
                    for ( int i = participateGroup.GetFaceList().Count - 1; i >= 0; i-- )
                    {

                        if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( bendingParticipateGroup.GetFaceList()[ bendingParticipateGroup.GetFaceList().Count - 1 ], participateGroup.GetFaceList()[ i ] ) )
                        {
                            layer--;
                        }
                    }

                    layer = participateGroup.GetTopLayer() + 1;
                    lastlayer = bendingParticipateGroup.GetBottomLayer();
                    for ( int i = 0; i < bendingParticipateGroup.Count; i++ )
                    {
                        if ( bendingParticipateGroup.GetFaceList()[ i ].Layer == lastlayer)
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        else
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            layer++;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        participateGroup.AddFace( bendingParticipateGroup.GetFaceList()[ i ] );
                    }
                }
                else
                {
                    // 从下贴合
                    int layer = 0;
                    int lastlayer = 0;
                    layer = participateGroup.GetBottomLayer();
                    layer--;

                    for ( int i = 0; i < participateGroup.GetFaceList().Count; i++ )
                    {
                        if ( !CloverMath.IsIntersectionOfTwoFaceOnOnePlane( bendingParticipateGroup.GetFaceList()[ 0 ], participateGroup.GetFaceList()[ i ] ) )
                        {
                            layer++;
                        }
                        else if (i == 0)
                        {
                            break;
                        }
                    }
                    lastlayer = participateGroup.GetTopLayer();
                    for ( int i = bendingParticipateGroup.GetFaceList().Count - 1; i >= 0; i-- )
                    {
                        if (bendingParticipateGroup.GetFaceList()[ i ].Layer == lastlayer)
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        else
                        {
                            lastlayer = bendingParticipateGroup.GetFaceList()[ i ].Layer;
                            layer--;
                            bendingParticipateGroup.GetFaceList()[ i ].Layer = layer;
                        }
                        participateGroup.AddFace( bendingParticipateGroup.GetFaceList()[ i ] );
                    }

                }
                faceGroupList.Add( participateGroup );
            }
            bendtype = BendTpye.BlendZero;
            bendingParticipateGroup = null;
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

        public Face FindFaceByID(int id)
        {
            foreach (Face face in facecellTree.Leaves)
            {
                if (face.ID == id)
                    return face;
            }
            return null;
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
