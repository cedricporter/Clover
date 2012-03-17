using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Clover.AbstractLayer;

namespace Clover.FoldingSystemNew
{
    class Tucking
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
        Point3D projectionPoint;
        Point3D originPoint;
        Edge currTuckLine = null;
        Edge lastTuckLine = null;

        #endregion

        #region 进入Tucking

        public void EnterTuckingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.cloverController = CloverController.GetInstance();

            // 寻找同group面中拥有pickedVertex的面中最下面的那个面作为floorFace,最上面的那个面作为ceilingFace
            this.group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace);
            if (this.group == null)
            {
                System.Windows.MessageBox.Show("找不到Groups");
                return;
            }
            this.pickedVertex = pickedVertex;
            this.floorFace = this.ceilingFace = nearestFace;
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

            // 保存pickedVertex的原始位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
        }

        #endregion

        #region 执行Tucking

        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;
            facesWithTuckLine.Clear();

            // 第零步，如果floorFace和ceilingFace是同一个，那就说明不可能进行tucking
            if (floorFace == ceilingFace)
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

            // 第四步，求currTuckLine与ceilingFace的交点
            Edge returnEdge = CloverTreeHelper.GetEdgeCrossedFace(ceilingFace, currTuckLine);

            // 第五步，移动两个虚像，造成面已经被切割折叠的假象
            // 该部分已经移至UI层处理

            // 第六步，进入下一个轮回
            lastTuckLine = currTuckLine;


            // 第七步，返回值
            return returnEdge;
        }

        #endregion

        #region 退出Tucking

        public void ExitTuckingMode()
        {
            facesInHouse.Clear();
            facesNotInHouse.Clear();
            facesWithTuckLine.Clear();
            facesWithoutTuckLine.Clear();
        }

        #endregion

    }
}
