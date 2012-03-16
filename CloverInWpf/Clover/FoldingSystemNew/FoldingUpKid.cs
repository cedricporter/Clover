using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.AbstractLayer;
using System.Windows.Media.Media3D;


namespace Clover
{
    class FoldingUpKid
    {

        #region 成员变量

        CloverController cloverController;
        FaceGroup group;
        Vertex pickedVertex;
        Face baseFace;
        List<Face> facesAboveBase = new List<Face>();
        List<Face> facesBelowBase = new List<Face>();
        List<Face> facesWithFoldLine = new List<Face>();
        Point3D originPoint;
        Point3D projectionPoint;
        Edge currFoldLine = null;
        Edge lastFoldLine = null;

        #endregion

        #region 进入折叠模式

        public void EnterFoldingMode(Vertex pickedVertex, Face nearestFace)
        {
            this.cloverController = CloverController.GetInstance();

            // 寻找同group面中拥有pickedVertex的面中最下面的那个面作为baseFace
            this.group = cloverController.FaceGroupLookupTable.GetGroup(nearestFace);
            this.pickedVertex = pickedVertex;
            this.baseFace = nearestFace;
            foreach (Face face in group.GetFaceList())
            {
                if (CloverTreeHelper.IsVertexInFace(pickedVertex, face) && face.Layer < baseFace.Layer)
                    baseFace = face;
            }

            // 将同Group的面分为在base面之上（含baseFace）和base面之下的两组
            foreach (Face face in group.GetFaceList())
            {
                if (face.Layer >= baseFace.Layer)
                    this.facesAboveBase.Add(face);
                else
                    this.facesBelowBase.Add(face);
            }

            // 保存pickedVertex的原始位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);

            // todo
            // 为facesAboveBase和faceUnderBase生成平面纹理
        }

        #endregion

        #region 在折叠模式中
        
        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;

            // todo 
            // 这里还应该有个判断条件，当facesAboveBase中的面有不同group的相邻面时，不可进行FoldingUp

            // 第一步，作折线
            if (CloverMath.IsTwoPointsEqual(originPoint, projectionPoint))
                return null;
            Point3D p1 = new Point3D(originPoint.X, originPoint.Y, originPoint.Z);
            Point3D p2 = new Point3D(projectionPoint.X, projectionPoint.Y, projectionPoint.Z);
            CloverMath.GetPerpendicularBisector(ref p1, ref p2, group.Normal);
            currFoldLine = new Edge(new Vertex(p1), new Vertex(p2));
            if (lastFoldLine == null)
                lastFoldLine = currFoldLine;

            // 第二步，在facesAboveBase中找出所有被折线切过的面。如果没有任何面被切过，则判定失败。
            foreach (Face face in facesAboveBase)
            {
                if (CloverTreeHelper.IsEdgeCrossedFace(face, currFoldLine))
                    facesWithFoldLine.Add(face);
            }
            if (facesWithFoldLine.Count == 0)
                return null;

            // 第三步，对facesWithFoldLine中的面进行逐个判定，只要有一个面判定失败，则判定失败。
            // todo

            // 第四步，求currFoldLine与baseFace的交点
            Edge returnEdge = CloverTreeHelper.GetEdgeCrossedFace(baseFace, currFoldLine);

            // 第五步，移动两个虚像，造成面已经被切割折叠的假象


            // 第六步，进入下一个轮回
            lastFoldLine = currFoldLine;
            facesWithFoldLine.Clear();

            // 第七步，返回值
            return returnEdge;
        }

        #endregion

        #region 退出折叠模式

        public void ExitFoldingMode()
        {
            // 释放资源
            facesAboveBase.Clear();
            facesBelowBase.Clear();
            facesWithFoldLine.Clear();
            lastFoldLine = null;
        }

        #endregion
    }
}
