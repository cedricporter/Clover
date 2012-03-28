using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover.AbstractLayer;
using System.Diagnostics;

namespace Clover
{
    public class Tucking
    {

        #region 成员变量

        CloverController cloverController;
        Vertex pickedVertex;
        FaceGroup group;
        Face ceilingFace, floorFace;
        List<Face> facesInHouse = new List<Face>();
        List<Face> facesNotInHouse = new List<Face>();
        List<Face> facesWithTuckLine = new List<Face>();
        List<Face> facesWithoutTuckLine = new List<Face>();
        List<Face> fixedFaces = new List<Face>();
        Point3D projectionPoint;
        Point3D originPoint;
        bool isPositive;
        Edge currTuckLine = null;
        Edge lastTuckLine = null;
        List<Edge> newEdges = new List<Edge>();

        #endregion

        #region 进入Tucking

        public List<Face> EnterTuckingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.cloverController = CloverController.GetInstance();
            //cloverController.ShadowSystem.CheckUndoTree();

            // 寻找同group面中拥有pickedVertex的面中最下面的那个面作为floorFace,最上面的那个面作为ceilingFace
            this.group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace);
            this.pickedVertex = pickedVertex;
            this.floorFace = this.ceilingFace = nearestFace;

            // 是正向还是反向
            Vector3D currNormal = group.Normal * cloverController.RenderController.Entity.Transform.Value;
            Double judge = Vector3D.DotProduct(currNormal, new Vector3D(0, 0, 1));
            isPositive = judge < 0 ? false : true;

            if (isPositive)
            {
                foreach (Face face in group.GetFaceList())
                {
                    if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer < floorFace.Layer)
                        floorFace = face;
                    if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer > ceilingFace.Layer)
                        ceilingFace = face;
                }
                // 将同Group的面分为在floor和ceiling之间和在floor和ceiling之外两组
                foreach (Face face in group.GetFaceList())
                {
                    if (face.Layer >= floorFace.Layer && face.Layer <= ceilingFace.Layer)
                        this.facesInHouse.Add(face);
                    else
                        this.facesNotInHouse.Add(face);
                }
            }
            else
            {
                foreach (Face face in group.GetFaceList())
                {
                    if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer > floorFace.Layer)
                        floorFace = face;
                    if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer < ceilingFace.Layer)
                        ceilingFace = face;
                }
                // 将同Group的面分为在floor和ceiling之间和在floor和ceiling之外两组
                foreach (Face face in group.GetFaceList())
                {
                    if (face.Layer <= floorFace.Layer && face.Layer >= ceilingFace.Layer)
                        this.facesInHouse.Add(face);
                    else
                        this.facesNotInHouse.Add(face);
                }
            }


            // 保存pickedVertex的原始位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);

            return facesInHouse;
        }

        #endregion

        #region 执行Tucking

        public Edge OnDrag(Point3D projectionPoint)
        {
            // 预定义
            FaceGroupLookupTable faceLookupTable = cloverController.FaceGroupLookupTable;
            this.projectionPoint = projectionPoint;
            facesWithTuckLine.Clear();

            // 第零步，如果floorFace和ceilingFace是同一个，那就说明不可能进行tucking
            // 如果面所在组的层数为非偶数，也不能够进行tucking in
            if (floorFace == ceilingFace)
                return null;

            if (faceLookupTable.GetGroup(ceilingFace).GetFaceList().Count % 2 != 0)
                return null;

            // 第一步，作折线
            if (CloverMath.IsTwoPointsEqual(originPoint, projectionPoint))
                return null;
            Point3D p1 = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            Point3D p2 = new Point3D(projectionPoint.X, projectionPoint.Y, projectionPoint.Z);
            CloverMath.GetPerpendicularBisector(ref p1, ref p2, group.Normal);
            currTuckLine = new Edge(new Vertex(p1), new Vertex(p2));
            if (lastTuckLine == null)
                lastTuckLine = currTuckLine;

            // 第二步，在facesInHouse中找出所有被折线切过的面。如果没有任何面被切过，则判定失败。
            foreach (Face face in facesInHouse)
            {
                if (CloverTreeHelper.IsEdgeCrossedFace(face, currTuckLine))
                    facesWithTuckLine.Add(face);
            }
            if (facesWithTuckLine.Count == 0)
                return null;

            // 第三步，对facesWithTuckLine中的面进行逐个判定，只要有一个面判定失败，则判定失败。
            // 第一小步，首先折线应该至少穿过两个面 
            if (facesWithTuckLine.Count < 2)
                return null;
            // 第二小步，暂时还没有想到

            // 第四步，求currTuckLine与ceilingFace的交点
            Edge returnEdge = CloverTreeHelper.GetEdgeCrossedFace(ceilingFace, currTuckLine);

            // 第五步，移动两个虚像，造成面已经被切割折叠的假象
            // 该部分已经移至UI层处理

            // 第六步，进入下一个轮回
            lastTuckLine = currTuckLine;


            // 第七步，返回值
            return returnEdge;
        }

        /// <summary>
        /// 判定移动的面
        /// </summary>
        /// <param name="pickedFace"></param>
        /// <param name="foldingLine"></param>
        public bool FindFaceWithoutTuckLine()
        {
            // 查找cut完成后所有要移动的面
            List<Face> tempFaces = new List<Face>();
            foreach (Face face in facesWithTuckLine)
            {
                if (face.LeftChild != null)
                    tempFaces.Add(face.LeftChild);
                if (face.RightChild != null)
                    tempFaces.Add(face.RightChild);
            }
            if (tempFaces.Count == 0)
                return false;

            // 从tempFaces中剔除拥有PickedVertex的那些Face
            // 同时tempFaces里面应该有与该面同组的并在其上层且没有折线经过的面
            List<Face> facesInSameGroup = cloverController.FaceGroupLookupTable.GetGroup(floorFace).GetFaceList();
            if (isPositive)
            {
                foreach (Face face in facesInSameGroup)
                {
                    if (face.Layer > floorFace.Layer && !facesWithTuckLine.Contains(face))
                        tempFaces.Add(face);
                }
            }
            else
            {
                foreach (Face face in facesInSameGroup)
                {
                    if (face.Layer < floorFace.Layer && !facesWithTuckLine.Contains(face))
                        tempFaces.Add(face);
                }
            }

            List<Face> facesWithPickedVertex = CloverTreeHelper.FindFacesFromVertex(tempFaces, pickedVertex);
            tempFaces = tempFaces.Except(facesWithPickedVertex).ToList();

            foreach (Face face in tempFaces)
            {
                foreach (Face faceWPV in facesWithPickedVertex)
                {
                    if (CloverMath.IsIntersectionOfTwoFaceOnOnePlane(face, faceWPV))
                    {
                        bool isClosed = false;
                        // 要除去折线的所有边
                        foreach (Edge e in faceWPV.Edges)
                        {
                            //if (e.Face1 == face || e.Face2 == face)
                            if ((e.Face1 == face || e.Face2 == face) && !newEdges.Contains(e))
                            {
                                isClosed = true;
                                break;
                            }
                        }
                        if (isClosed)
                        {
                            facesWithoutTuckLine.Add(face);
                            break;
                        }
                    }
                }
            }

            foreach (Face face in facesWithPickedVertex)
            {
                if (!facesWithoutTuckLine.Contains(face))
                    facesWithoutTuckLine.Add(face);
            }

            // 找到所有不动的面
            fixedFaces = tempFaces.Except(facesWithoutTuckLine).ToList();

            return true;
        }

        /// <summary>
        /// 两边对称的进行tuck in层的调整 
        /// </summary>
        public bool UpdateLayerInfoAfterTuckIn(Face _ceilingFace, Face _floorFace, Edge _currTuckLine, bool _isPositive)
        {
            cloverController = CloverController.GetInstance();
            Face cf = _ceilingFace.LeftChild;
            FaceGroup currGroup = cloverController.FaceGroupLookupTable.GetGroup(_ceilingFace.LeftChild);
            List<Face> facesContainsCeiling = currGroup.GetFaceList();
            List<Face> facesAboveCeiling = new List<Face>();
            
            // 找到当前正确的_ceilingFace和_floorFace.
            foreach (Face face in facesContainsCeiling)
            {
                if (face.Layer == _ceilingFace.Layer && CloverTreeHelper.IsEdgeCrossedFace(face, _currTuckLine))
                    _ceilingFace = face;
                if (face.Layer == _floorFace.Layer && CloverTreeHelper.IsEdgeCrossedFace(face, _currTuckLine))
                    _floorFace = face; 
            }

            if (_isPositive)
            {
                foreach (Face face in facesContainsCeiling)
                {
                    if (face.Layer > _ceilingFace.Layer)
                        facesAboveCeiling.Add(face);
                }
            }
            else
            {
                foreach (Face face in facesContainsCeiling)
                {
                    if (face.Layer < _ceilingFace.Layer)
                        facesAboveCeiling.Add(face);
                }
            }

            // 修订_ceilingFace的层数到最高层, 且_ceilingFace的层数一定为奇数
            // 若_ceilingFace和_floorFace的层数相差超过一层，则不修改
            if (Math.Abs((_ceilingFace.Layer - _floorFace.Layer)) <= 1)
            {
                if (_isPositive)
                {
                    _ceilingFace.Layer += facesAboveCeiling.Count();
                    if (_ceilingFace.Layer % 2 == 0)
                        return false;
                }
                else
                {
                    _ceilingFace.Layer -= facesAboveCeiling.Count();
                    if (_ceilingFace.Layer % 2 != 0)
                        return false;
                }
            }

            // 先将所有高于_ceilingFace的面按照层进行排序
            facesAboveCeiling.Sort(new layerComparer());
            
            // 将TuckingIn的面进行层排列
            if (_isPositive)
            {
                for (int i = (facesAboveCeiling.Count() - 1), j = 1; i >= 0; i--, j++)
                    facesAboveCeiling[i].Layer = _floorFace.Layer + j;
            }
            else
            {
                for (int i = 0, j = 1; i < facesAboveCeiling.Count(); i++, j++)
                    facesAboveCeiling[i].Layer = _floorFace.Layer - j;
            }

            currGroup.SortFace();

            return true; 
        }
        #endregion

        #region 退出Tucking

        public void ExitTuckingMode()
        {
            // 应用折叠
            //Edge tuckLine = CloverTreeHelper.GetEdgeCrossedFace(floorFace, currTuckLine);
            if (currTuckLine == null)
                return;
            newEdges = cloverController.FoldingSystem.CutFaces(facesWithTuckLine, currTuckLine);
            FindFaceWithoutTuckLine();
            cloverController.FoldingSystem.RotateFaces(facesWithoutTuckLine, currTuckLine, 180);

            // 添加折线
            if (newEdges.Count != 0)
            {
                foreach (Edge edge in newEdges)
                {
                    cloverController.RenderController.AddFoldingLine(edge.Vertex1.u, edge.Vertex1.v, edge.Vertex2.u, edge.Vertex2.v);
                }
            }

            // 更新组
            cloverController.FaceGroupLookupTable.UpdateTableAfterFoldUp(facesWithTuckLine, facesWithoutTuckLine, fixedFaces, isPositive);

            // Generate Script
            ScriptGenerator.GetInstance().AddTuckingAction(
                facesWithTuckLine, facesWithoutTuckLine, fixedFaces, 
                currTuckLine, ceilingFace, floorFace, isPositive);

            // 更新层信息
            UpdateLayerInfoAfterTuckIn(ceilingFace, floorFace, currTuckLine, isPositive);

            // 反重叠
            RenderController.GetInstance().AntiOverlap();

            // 测试散开
            //group = cloverController.FaceGroupLookupTable.GetGroup(newEdges[0].Face1);
            //RenderController.GetInstance().DisperseLayer(group);
            //RenderController.GetInstance().Update(group, false);

            // 释放资源
            facesInHouse.Clear();
            facesNotInHouse.Clear();
            facesWithTuckLine.Clear();
            facesWithoutTuckLine.Clear();
            lastTuckLine = currTuckLine = null;
        }

        #endregion

        #region 2b截折线

        int howMany = 0;
        Point3D cutFoldLineP1, cutFoldLineP2;

        public void CutFoldLine(Point3D point)
        {
            if (CloverMath.IsPointInTwoPoints(point, currTuckLine.Vertex2.GetPoint3D(), currTuckLine.Vertex1.GetPoint3D(), 2))
            {
                Debug.WriteLine("howMany " + howMany.ToString());
                if (howMany == 0)
                {
                    cutFoldLineP1 = point;
                    howMany++;
                }
                else if (howMany == 1)
                {
                    cutFoldLineP2 = point;
                    howMany = 0;
                    currTuckLine = new Edge(new Vertex(cutFoldLineP1), new Vertex(cutFoldLineP2));

                    facesWithTuckLine.Clear();
                    // 第二步，在facesAboveBase中找出所有被折线切过的面。如果没有任何面被切过，则判定失败。
                    foreach (Face face in facesInHouse)
                    {
                        if (CloverTreeHelper.IsEdgeCrossedFace(face, currTuckLine))
                            facesWithTuckLine.Add(face);
                    }
                    if (facesWithTuckLine.Count == 0)
                        return;

                }
            }
        }

        #endregion
    }
}
