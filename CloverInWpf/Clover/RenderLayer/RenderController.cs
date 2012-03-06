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

        TranslateTransform3D translateTransform = new TranslateTransform3D();
        public System.Windows.Media.Media3D.TranslateTransform3D TranslateTransform
        {
            get { return translateTransform; }
            set 
            {
                if (value.OffsetZ < 40 || value.OffsetZ > 1000)
                    return;
                translateTransform = value; 
            }
        }

        RotateTransform3D rotateTransform = new RotateTransform3D();
        public System.Windows.Media.Media3D.RotateTransform3D RotateTransform
        {
            get { return rotateTransform; }
            set 
            { 
                rotateTransform = value;
                UpdatePosition();
                // 让VertexInfoVisual更新
                foreach (Edge edge in CloverController.GetInstance().Edges)
                {
                    edge.Vertex1.Update(edge.Vertex1, null);
                    edge.Vertex2.Update(edge.Vertex2, null);
                }
            }
        }
        
        Double distance = 300;
        public System.Double Distance
        {
            get { return distance; }
            set 
            { 
                if (value > 40 && value < 1000)
                    distance = value;
                UpdatePosition();
                // 让VertexInfoVisual更新
                foreach (Edge edge in CloverController.GetInstance().Edges)
                {
                    edge.Vertex1.Update(edge.Vertex1, null);
                    edge.Vertex2.Update(edge.Vertex2, null);
                }
            }
        }

        MaterialGroup frontMaterial;
	    public System.Windows.Media.Media3D.MaterialGroup FrontMaterial
	    {
            get { return frontMaterial; }
            set
            {
                frontMaterial = materialController.UpdateFrontMaterial(value);
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.Material = value;
                }
            }
        }

        MaterialGroup backMaterial;
        public System.Windows.Media.Media3D.MaterialGroup BackMaterial
        {
            get { return backMaterial; }
            set 
            { 
                backMaterial = materialController.UpdateBackMaterial(value);
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.BackMaterial = value;
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
        //public Dictionary<Face, GeometryModel3D> FaceMeshMap
        //{
        //    get { return faceMeshMap; }
        //    //set { faceMeshMap = value; }
        //}

        #endregion
        
        #region 私有成员变量
        MaterialController materialController = new MaterialController();
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
            UpdatePosition();
        }

        #endregion

        /// <summary>
        /// 更新折纸位置
        /// </summary>
        void UpdatePosition()
        {
            Transform3DGroup tg = new Transform3DGroup();
            TranslateTransform3D ts = new TranslateTransform3D(0, 0, -distance);
            tg.Children.Add(rotateTransform);
            tg.Children.Add(ts);
            entity.Transform = tg;

            if (instance != null) // 这个判断避免了Utility类和RenderController类无限递归调用
                Utility.GetInstance().UpdateWorlCameMat();
            
        }

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
            AntiOverlap();
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
                // 更新3d坐标
                mesh.Positions.Add(new Point3D(v.X, v.Y, v.Z));
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

        /// <summary>
        /// 反重叠，让纸张散开
        /// </summary>
        void AntiOverlap()
        {
            LookupTable lt = CloverController.GetInstance().Table;
            if (lt == null || lt.Tables.Count == 0)
                return;
            foreach (FaceGroup g in lt.Tables)
            {
                float baseval = 0;
                float step = 0.1f;
                foreach (Face f in g.GetGroup())
                {
                    Vector3D offset = g.Normal * baseval;
                    faceMeshMap[f].Transform = new TranslateTransform3D(offset);
                    baseval += step;
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
            cubeNav.CubeNavModel.Transform = rotateTransform;
            cubeNav.LastQuat = srcQuaternion;
        }

        #endregion

        #endregion

    }
}
