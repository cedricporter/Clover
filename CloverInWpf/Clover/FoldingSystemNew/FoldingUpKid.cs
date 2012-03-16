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
        List<Face> facesUnderBase = new List<Face>();
        Point3D originPoint;
        Point3D projectionPoint;

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
                    this.facesUnderBase.Add(face);
            }

            // 保存pickedVertex的原始位置
            originPoint = new Point3D(pickedVertex.X, pickedVertex.Y, pickedVertex.Z);
        }

        #endregion

        #region 在折叠模式中
        
        public Edge OnDrag(Point3D projectionPoint)
        {
            this.projectionPoint = projectionPoint;

            return null;
        }

        #endregion

        #region 退出折叠模式

        public void ExitFoldingMode()
        {
            // 释放资源
            facesAboveBase.Clear();
            facesUnderBase.Clear();
        }

        #endregion
    }
}
