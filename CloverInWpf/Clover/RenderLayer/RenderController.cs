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

        ModelVisual3D entity = new ModelVisual3D();
        public System.Windows.Media.Media3D.ModelVisual3D Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        Model3DGroup modelGroup = new Model3DGroup();

        Dictionary<Face, GeometryModel3D> faceMeshMap = new Dictionary<Face, GeometryModel3D>();

        #endregion

        #region 私有成员变量
        MaterialController materialController = new MaterialController();
        Transform3DGroup transformGroup = new Transform3DGroup();
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
        public void Update(Face face)
        {
            faceMeshMap[face].Geometry = NewMesh(face);
            // 让纸张散开
            AntiOverlap();
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
            AntiOverlap();
        }

        /// <summary>
        /// 根据传入的Face创建一个新MeshGeometry3D
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        MeshGeometry3D NewMesh(Face face)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            // 更新顶点
            foreach (Vertex v in face.Vertices)
            {
                Point3D rp;
                rp = UpdateRenderPoint(v);
                // 更新3d坐标
                mesh.Positions.Add(new Point3D(rp.X, rp.Y, rp.Z));
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
        /// 反重叠，让纸张散开
        /// </summary>
        public void AntiOverlap()
        {
            //LookupTable lt = CloverController.GetInstance().Table;
            //if (lt == null || lt.Tables.Count == 0)
            //    return;
            FaceGroupLookupTable faceGroupLookupTable = CloverController.GetInstance().FaceGroupLookupTable;
            //faceGroupLookupTable.UpdateTableAfterFoldUp();

            foreach (FaceGroup g in CloverController.GetInstance().FaceGroupLookupTable.FaceGroupList)
            {
                float baseval = 0;
                float step = 0.01f;
                foreach (Face f in g.GetFaceList())
                {
                    if (faceMeshMap.ContainsKey(f))
                    {
                        Vector3D offset = g.Normal * baseval;

                        faceMeshMap[f].Transform = new TranslateTransform3D(offset);
                        baseval += step;
                    }
                    else
                    {
                        int a =1;
                    }
                }
            }
        }

        #endregion

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

        /// <summary>
        /// 为一个顶点添加提示信息
        /// </summary>
        /// <param name="v"></param>
        public void AddVisualInfoToVertex(Vertex v)
        {
            VertexInfoVisual vi = new VertexInfoVisual(v);
            VisualController.GetSingleton().AddVisual(vi);
            v.Update += vi.UpdateInfoCallBack;
            //CubeNavigator.GetInstance().Update += vi.UpdateInfoCallBack;
            vi.Start();
        }

        #region 动画



        /// <summary>
        /// 一些无法通过故事板实现，只能基于帧的动画
        /// </summary>
        public void RenderAnimations()
        {
            RotationSlerp();
            materialController.PaperChange();
            //AntiOverlap();
        }

        #region 从3D视角变为2D视角的插值动画

        int RotationSlerpCount = -1;
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
            if (RotationSlerpCount > 60)
            {
                srcQuaternion = dstQuaternion;
                RotationSlerpCount = -1;
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

        #endregion

    }
}
