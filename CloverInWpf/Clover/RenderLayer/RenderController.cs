using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Clover.Visual;
using Clover.AbstractLayer;

/**
@date		:	2012/03/01
@filename	: 	RenderController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	控制渲染层运作
**/

namespace Clover
{
    public delegate void OnRotationEndHandle();

    public class RenderController
    {

        #region Get/Set 方法

        Quaternion dstQuaternion = new Quaternion();
        public System.Windows.Media.Media3D.Quaternion DstQuaternion
        {
            get { return dstQuaternion; }
            //set { dstQuaternion = value; }
        }

        Quaternion srcQuaternion = new Quaternion();
        public System.Windows.Media.Media3D.Quaternion SrcQuaternion
        {
            get { return srcQuaternion; }
            set { srcQuaternion = value; }
        }

        public System.Windows.Media.Media3D.TranslateTransform3D TranslateTransform
        {
            get
            {
                Utility.GetInstance().UpdateWorlCameMat();
                // 让VertexInfoVisual更新
                foreach (Edge edge in CloverController.GetInstance().Edges)
                {
                    if (edge.Vertex1.Update != null && edge.Vertex2.Update != null)
                    {
                        edge.Vertex1.Update(edge.Vertex1, null);
                        edge.Vertex2.Update(edge.Vertex2, null);
                    }
                }
                return (TranslateTransform3D)transformGroup.Children[1];
            }
            set
            {
                transformGroup.Children[1] = value;
                Utility.GetInstance().UpdateWorlCameMat();
            }
        }

        public System.Windows.Media.Media3D.RotateTransform3D RotateTransform
        {
            get
            { return (RotateTransform3D)transformGroup.Children[0]; }
            set
            {
                transformGroup.Children[0] = value;
                Utility.GetInstance().UpdateWorlCameMat();
                // 让VertexInfoVisual更新
                foreach (Edge edge in CloverController.GetInstance().Edges)
                {
                    if (edge.Vertex1.Update != null)
                        edge.Vertex1.Update(edge.Vertex1, null);
                    if (edge.Vertex2.Update != null)
                        edge.Vertex2.Update(edge.Vertex2, null);
                }
            }
        }

        MaterialGroup frontMaterial;
        public System.Windows.Media.Media3D.DiffuseMaterial FrontMaterial
        {
            get { return (DiffuseMaterial)frontMaterial.Children[0]; }
            set
            {
                frontMaterial = materialController.UpdateFrontMaterial(value);
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.Material = frontMaterial;
                }
            }
        }

        MaterialGroup backMaterial;
        public System.Windows.Media.Media3D.DiffuseMaterial BackMaterial
        {
            get { return (DiffuseMaterial)backMaterial.Children[0]; }
            set
            {
                backMaterial = materialController.UpdateBackMaterial(value);
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.BackMaterial = backMaterial;
                }
            }
        }

        /// <summary>
        /// 纸张模型
        /// </summary>
        ModelVisual3D entity = new ModelVisual3D();
        public System.Windows.Media.Media3D.ModelVisual3D Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        /// <summary>
        /// 纸张虚像
        /// </summary>
        ModelVisual3D shadow = new ModelVisual3D();
        public System.Windows.Media.Media3D.ModelVisual3D Shadow
        {
            get { return shadow; }
            set { shadow = value; }
        }

        Model3DGroup modelGroup = new Model3DGroup();
        public System.Windows.Media.Media3D.Model3DGroup ModelGroup
        {
            get { return modelGroup; }
            //set { modelGroup = value; }
        }

        Dictionary<Face, GeometryModel3D> faceMeshMap = new Dictionary<Face, GeometryModel3D>();
        public Dictionary<Face, GeometryModel3D> FaceMeshMap
        {
            get { return faceMeshMap; }
            //set { faceMeshMap = value; }
        }
        #endregion

        #region 私有成员变量
        MaterialController materialController = new MaterialController();
        Transform3DGroup transformGroup = new Transform3DGroup();
        public System.Windows.Media.Media3D.Transform3DGroup TransformGroup
        {
            get { return transformGroup; }
            //set { transformGroup = value; }
        }
        #endregion

        #region 单例

        static RenderController instance = null;
        public static RenderController GetInstance()
        {
            if (instance == null)
            {
                instance = new RenderController();
            }
            return instance;
        }
        RenderController()
        {
            entity.Content = modelGroup;
            TranslateTransform3D translateTransform = new TranslateTransform3D(0, 0, -300);
            transformGroup.Children.Add(new RotateTransform3D());
            transformGroup.Children.Add(translateTransform);
            entity.Transform = transformGroup;
        }

        #endregion

        #region 改变材质特性

        public void Testfuck()
        {
            //frontMaterial = materialController.GetFrontShadow();
            foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
            {
                pair.Value.Material = materialController.GetFrontShadow();
                pair.Value.BackMaterial = materialController.GetBackShadow();
            }
        }

        /// <summary>
        /// 转变成半透明材质
        /// </summary>
        /// <param name="face"></param>
        public void ToGas(Face face)
        {
            faceMeshMap[face].Material = materialController.GetFrontShadow();
            faceMeshMap[face].BackMaterial = materialController.GetBackShadow();
        }

        /// <summary>
        /// 转变成不透明材质
        /// </summary>
        /// <param name="face"></param>
        public void ToSolid(Face face)
        {
            faceMeshMap[face].Material = frontMaterial;
            faceMeshMap[face].BackMaterial = backMaterial;
        }

        #endregion

        #region 对Mesh的操作

        /// <summary>
        /// 添加一个新面
        /// </summary>
        /// <param name="face"></param>
        public void New(Face face)
        {
            GeometryModel3D model = new GeometryModel3D();
            model.Material = frontMaterial;
            model.BackMaterial = backMaterial;
            model.Geometry = NewMesh(face);
            modelGroup.Children.Add(model);
            faceMeshMap[face] = model;
            // 让纸张散开
            //AntiOverlap();
        }

        /// <summary>
        /// 删除指定面
        /// </summary>
        /// <param name="face"></param>
        public void Delete(Face face)
        {
            //bool asdf = modelGroup.Children[0].IsFrozen;
            modelGroup.Children.Remove(faceMeshMap[face]);
            faceMeshMap.Remove(face);
            // 让纸张散开
            //AntiOverlap();
        }

        /// <summary>
        /// 删除所有的面
        /// </summary>
        public void DeleteAll()
        {
            modelGroup.Children.Clear();
            faceMeshMap.Clear();
        }

        /// <summary>
        /// 更新一个已存在的面
        /// </summary>
        /// <param name="face"></param>
        public void Update(Face face, Boolean autoUpdate = true)
        {
            faceMeshMap[face].Geometry = NewMesh(face, autoUpdate);
            // 让纸张散开
            //AntiOverlap();
        }

        /// <summary>
        /// 更新整个组的面
        /// </summary>
        /// <param name="group"></param>
        public void Update(FaceGroup group, Boolean autoUpdate = true)
        {
            foreach (Face face in group.GetFaceList())
            {
                faceMeshMap[face].Geometry = NewMesh(face, autoUpdate);
            }
        }

        /// <summary>
        /// 更新所有已存在的面
        /// </summary>
        public void UpdateAll()
        {
            foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
            {
                Face face = pair.Key;
                pair.Value.Geometry = NewMesh(face);
            }
            // 让纸张散开
            //AntiOverlap();
        }

        /// <summary>
        /// 根据传入的Face创建一个新MeshGeometry3D
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        MeshGeometry3D NewMesh(Face face, Boolean autoUpdate = true)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            // 更新顶点
            foreach (Vertex v in face.Vertices)
            {
                //Point3D rp;
                //rp = UpdateRenderPoint(v);
                // 更新3d坐标
                if (autoUpdate)
                    mesh.Positions.Add(UpdateRenderPoint(v));
                else
                    mesh.Positions.Add(v.RenderPoint);
                // 更新纹理坐标
                mesh.TextureCoordinates.Add(new Point(v.u, v.v));
            }
            // 更新索引
            for (int i = 1; i < face.Vertices.Count - 1; i++)
            {
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(i);
                mesh.TriangleIndices.Add(i + 1);
                //Debug.WriteLine(face.Vertices[i].point);
            }
            return mesh;
        }

        Point3D UpdateRenderPoint(Vertex v)
        {
            Point3D rp = new Point3D();
            rp.X = v.X;
            rp.Y = v.Y;
            rp.Z = v.Z;
            v.RenderPoint = rp;
            return rp;
        }

        /// <summary>
        /// 反重叠
        /// </summary>
        public void AntiOverlap()
        {
            FaceGroupLookupTable faceGroupLookupTable = CloverController.GetInstance().FaceGroupLookupTable;

            foreach (FaceGroup g in CloverController.GetInstance().FaceGroupLookupTable.FaceGroupList)
            {
                float baseval = 0;
                //float step = 6f;
                float step = 0.01f;
                foreach (Face f in g.GetFaceList())
                {
                    if (faceMeshMap.ContainsKey(f))
                    {
                        //Vector3D offset = g.Normal * baseval;
                        Vector3D offset = g.Normal * f.Layer * step;
                        faceMeshMap[f].Transform = new TranslateTransform3D(offset);
                        baseval += step;
                    }
                    else
                    {
                        //int a =1;
                        System.Windows.MessageBox.Show("你又2B了，faceMeshMap里面没有Group里面的face。");
                    }
                }
            }

        }

        /// <summary>
        /// 使纸张按组的法线方向散开
        /// </summary>
        /// <param name="group"></param>
        public void DisperseLayer(FaceGroup group)
        {
            if (group == null || group.GetFaceList().Count == 0)
                return;
            List<Face> faceList = group.GetFaceList();
            // 重置同组所有面的渲染点坐标
            foreach (Face face in faceList)
            {
                foreach (Vertex v in face.Vertices)
                {
                    v.RenderPoint = new Point3D(v.X, v.Y, v.Z);
                }
            }
            // 以当前总层数作为依据，指定offset值
            int step = 6;
            //int step = 1;
            int offset = faceList.Count / 2 * step;
            // 从faceList的两头向中间逼近，逐层计算偏移量
            int bottom = 0;
            int top = faceList.Count - 1;
            while (top >= bottom)
            {
                Vector3D offVec = group.Normal * -offset;
                foreach (Vertex v in faceList[bottom].Vertices)
                {
                    v.RenderPoint += offVec;
                }

                offVec *= -1;
                foreach (Vertex v in faceList[top].Vertices)
                {
                    v.RenderPoint += offVec;
                }

                offset -= step;
                top--;
                bottom++;
            }
        }

        #endregion

        #region 对材质的操作

        /// <summary>
        /// 增加新的折线
        /// </summary>
        /// <param name="u0"></param>
        /// <param name="v0"></param>
        /// <param name="u1"></param>
        /// <param name="v1"></param>
        /// <param name="isUpdate"></param>
        public void AddFoldingLine(Double u0, Double v0, Double u1, Double v1)
        {
            materialController.AddFoldingLine(u0, v0, u1, v1);
        }

        /// <summary>
        /// 撤销一步生成的折线
        /// </summary>
        public void UndrawFoldLine()
        {
            materialController.RebuildFoldLinesToPrev();
        }

        /// <summary>
        /// 根据ShadowSystem当前的堆栈重绘所有折线至下个版本
        /// </summary>
        public void RedrawFoldLine()
        {
            materialController.RebuildFoldLinesToNext();
        }

        /// <summary>
        /// 获取正面纹理
        /// </summary>
        /// <returns></returns>
        public ImageSource GetFrontTexture()
        {
            return materialController.MergeFrontTexture();
        }

        /// <summary>
        /// 获取背面纹理
        /// </summary>
        /// <returns></returns>
        public ImageSource GetBackTexture()
        {
            return materialController.MergeBackTexture();
        }

        #endregion

        /// <summary>
        /// 为一个顶点添加提示信息
        /// </summary>
        /// <param name="v"></param>
        public void AddVisualInfoToVertex(Vertex v)
        {
            //VertexInfoVisual vi = new VertexInfoVisual(v);
            //VisualController.GetSingleton().AddVisual(vi);
            //v.Update += vi.UpdateInfoCallBack;
            ////CubeNavigator.GetInstance().Update += vi.UpdateInfoCallBack;
            //vi.Start();
        }

        #region 动画



        /// <summary>
        /// 一些无法通过故事板实现，只能基于帧的动画
        /// </summary>
        public void RenderAnimations()
        {
            RotationSlerp();
            TranslateSlerp();
            materialController.PaperChange();
            RotatePaperY();
            //AntiOverlap();
        }

        #region 从3D视角变为2D视角的插值动画

        int RotationSlerpCount = -1;

        public OnRotationEndHandle OnRotationEndOnce;

        public void BeginRotationSlerp(Quaternion dst)
        {
            dstQuaternion = dst;
            RotationSlerpCount = 0;
        }
        void RotationSlerp()
        {
            if (RotationSlerpCount == -1)
                return;
            // 插值
            srcQuaternion = Quaternion.Slerp(srcQuaternion, dstQuaternion, 0.15);
            RotationSlerpCount++;
            // 判断动画中止的条件
            if (RotationSlerpCount > 40)
            {
                srcQuaternion = dstQuaternion;
                RotationSlerpCount = -1;
                if (OnRotationEndOnce != null)
                    OnRotationEndOnce();
            }
            // 动画
            RotateTransform = new RotateTransform3D(new QuaternionRotation3D(srcQuaternion));
            CubeNavigator cubeNav = CubeNavigator.GetInstance();
            //cubeNav.CubeNavModel.Transform = (RotateTransform3D)transformGroup.Children[0];
            cubeNav.CubeFront.Transform = cubeNav.CubeBack.Transform = cubeNav.CubeUp.Transform =
                     cubeNav.CubeDown.Transform = cubeNav.CubeLeft.Transform = cubeNav.CubeRight.Transform = (RotateTransform3D)transformGroup.Children[0];
            cubeNav.LastQuat = srcQuaternion;
        }

        #endregion

        #region 自动绕Y轴旋转视角的动画

        Boolean rotatePaperYFlag = false;
        Quaternion rotatePaperYQuat = new Quaternion(new Vector3D(0, 1, 0), 1);
        void RotatePaperY()
        {
            if (!rotatePaperYFlag)
                return;
            srcQuaternion = rotatePaperYQuat * srcQuaternion;
            RotateTransform = new RotateTransform3D(new QuaternionRotation3D(srcQuaternion));
        }

        public void BeginRotatePaperY(Boolean isOn)
        {
            rotatePaperYFlag = isOn;
        }

        #endregion

        #region 平移动画

        int translateSlerpCount = -1;
        String translateSlerpDirection = "";
        void TranslateSlerp()
        {
            if (translateSlerpDirection == "")
                return;

            switch (translateSlerpDirection)
            {
                case "up":
                    TranslateTransform.OffsetY += 1;
                    break;
                case "dn":
                    TranslateTransform.OffsetY -= 1;
                    break;
                case "lt":
                    TranslateTransform.OffsetX -= 1;
                    break;
                case "rt":
                    TranslateTransform.OffsetX += 1;
                    break;
                case "rs":
                    translateSlerpCount--;
                    TranslateTransform.OffsetX /= 1.2;
                    TranslateTransform.OffsetY /= 1.2;
                    if (translateSlerpCount == -1)
                    {
                        EndTranslateSlerp();
                        TranslateTransform.OffsetX = TranslateTransform.OffsetY = 0;
                    }
                    break;
            }
        }

        public void BeginTranslateSlerp(String direction)
        {
            translateSlerpDirection = direction;
            if (direction == "rs")
                translateSlerpCount = 40;
        }

        public void EndTranslateSlerp()
        {
            translateSlerpDirection = "";
        }

        #endregion

        #endregion

    }
}
