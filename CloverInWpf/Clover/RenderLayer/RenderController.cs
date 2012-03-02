﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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

namespace Clover.RenderLayer
{
    public class RenderController
    {

        #region Get/Set 方法

        Double distance = 300;
        public System.Double Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        Material frontMaterial;
	    public System.Windows.Media.Media3D.Material FrontMaterial
	    {
            get { return frontMaterial; }
            set
            {
                frontMaterial = value;
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.Material = value;
                }
            }
        }

        Material backMaterial;
        public System.Windows.Media.Media3D.Material BackMaterial
        {
            get { return backMaterial; }
            set 
            { 
                backMaterial = value;
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
        public Dictionary<Face, GeometryModel3D> FaceMeshMap
        {
            get { return faceMeshMap; }
            //set { faceMeshMap = value; }
        }

        //Dictionary<Edge, ModelVisual3D> edgeLineMap = new Dictionary<Edge, ModelVisual3D>();
        //public Dictionary<Edge, ModelVisual3D> EdgeLineMap
        //{
        //    get { return edgeLineMap; }
        //    //set { edgeLineMap = value; }
        //}
        #endregion

        MainWindow mainWindow;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RenderController(MainWindow mainWindow)
        {
            entity.Content = modelGroup;
            this.mainWindow = mainWindow;
            entity.Transform = new TranslateTransform3D(0, 0, -distance);
        }

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
        }

        /// <summary>
        /// 删除指定面
        /// </summary>
        /// <param name="face"></param>
        public void Delete(Face face)
        {
            modelGroup.Children.Remove(faceMeshMap[face]);
            faceMeshMap.Remove(face);
        }

        /// <summary>
        /// 更新一个已存在的面
        /// </summary>
        /// <param name="face"></param>
        public void Update(Face face)
        {
            faceMeshMap[face].Geometry = NewMesh(face);     
        }

        /// <summary>
        /// 更新所有已存在的面
        /// </summary>
        public void UpdateAll()
        {
            foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
            {
                Face face = pair.Key;
                GeometryModel3D model = pair.Value;
                model.Geometry = NewMesh(face);
            }
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
                mesh.TriangleIndices.Add(face.Vertices[i + 1].Index);
                mesh.TriangleIndices.Add(face.Vertices[i].Index);
                mesh.TriangleIndices.Add(face.Vertices[0].Index);
                //Debug.WriteLine(face.Vertices[i].point);
            }
            //// 描绘边缘
            //foreach (Edge edge in face.Edges)
            //{
            //    _3DTools.ScreenSpaceLines3D line = new _3DTools.ScreenSpaceLines3D();
            //    line.Points.Add(edge.Vertex1.GetPoint3D());
            //    line.Points.Add(edge.Vertex2.GetPoint3D());
            //    line.Color = Colors.Black;
            //    line.Thickness = 2;
            //    mainWindow.foldingPaperViewport.Children.Add(line);
            //}

            return mesh;
        }
    }
}