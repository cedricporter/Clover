using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        MaterialGroup frontMaterial;
	    public System.Windows.Media.Media3D.MaterialGroup FrontMaterial
	    {
            get { return frontMaterial; }
            set
            {
                frontMaterial = value;
                ImageSource img = ((value.Children[0] as DiffuseMaterial).Brush as ImageBrush).ImageSource;
                materialImageHeight = img.Height;
                materialImageWidth = img.Width;
                foreach (KeyValuePair<Face, GeometryModel3D> pair in faceMeshMap)
                {
                    pair.Value.Material = value;
                }
                UpdateEdgeLayer();
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

        #endregion

        #region 私有成员变量

        Double materialImageHeight = 2;
        Double materialImageWidth = 2;
        DiffuseMaterial edgeLayer = null;

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

        public void AddSolidStroke(Point p1, Point p2)
        {
            if (edgeLayer == null)
            {
                edgeLayer = new DiffuseMaterial();
                
            }
            ////test
            //Image myImage = new Image();
            //Rect rect = new Rect(new Size(100, 100));
            ////Rectangle rect = new Rectangle();
            ////rect.Height = rect.Width = 100;
            ////rect.Fill = new SolidColorBrush(Colors.Black);

            //DrawingVisual drawingVisual = new DrawingVisual();
            //DrawingContext drawingContext = drawingVisual.RenderOpen();
            //drawingContext.DrawRectangle(new SolidColorBrush(Colors.Black), (Pen)null, rect);
            //drawingContext.Close();

            //RenderTargetBitmap bmp = new RenderTargetBitmap(1200, 1200, 96, 96, PixelFormats.Pbgra32);
            //bmp.Render(drawingVisual);
            //myImage.Source = bmp;
            //ImageBrush imb2 = new ImageBrush(bmp);
            //mgf.Children.Add(new DiffuseMaterial(imb2));
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

            return mesh;
        }

        /// <summary>
        /// 当分辨率改变时改变纸张的边的分辨率
        /// </summary>
        void UpdateEdgeLayer()
        {
            Double thickness = (materialImageHeight > materialImageWidth ? materialImageHeight : materialImageWidth) / 100;
            Image img = new Image();
            Rect rect = new Rect(new Size(materialImageWidth, materialImageHeight));
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawRectangle((Brush)null, new Pen(new SolidColorBrush(Colors.Black), thickness), rect);
            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)materialImageWidth, (int)materialImageHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            img.Source = bmp;
            ImageBrush imgb = new ImageBrush(bmp);

            if (edgeLayer != null)
                frontMaterial.Children.Remove(edgeLayer);
            edgeLayer = new DiffuseMaterial(imgb);
            frontMaterial.Children.Add(edgeLayer);
        }

    }
}
