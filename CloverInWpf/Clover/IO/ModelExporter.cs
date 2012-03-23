using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;

/**
@date		:	2012/02/27@filename	: 	CubeNavigator.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:   输出当前模型的xmal文件	
*/

namespace Clover.IO
{
    class ModelExporter
    {
        static int indent = 0;

        static void WriteLineWithIndent(StreamWriter sw, String s)
        {
            String s1 = s.Substring(0, 1);
            String s2 = s.Substring(0, 2);
            if (s.Length > 2 && s2 == "</" || s1 == "/")
                indent -= 4;
            for (int i = 0; i < indent; i++)
                s = " " + s;
            sw.WriteLine(s);
            if (s.Length > 2 && s1 == "<" && s2 != "</")
                indent += 4;
        }

        public static void Export(String file)
        {
            StreamWriter sw = File.CreateText(file);
            Model3DGroup mg = RenderController.GetInstance().ModelGroup;
            String str = "";
            // 头
            WriteLineWithIndent(sw, "<ResourceDictionary");
            WriteLineWithIndent(sw, "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'");
            WriteLineWithIndent(sw, "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>");
            WriteLineWithIndent(sw, "<ModelVisual3D x:Key='fuckmodel'>");
            WriteLineWithIndent(sw, "<ModelVisual3D.Content>");
            WriteLineWithIndent(sw, "<Model3DGroup>");

            foreach (GeometryModel3D gm in mg.Children)
            {
                WriteLineWithIndent(sw, "<GeometryModel3D>");
                // mesh
                MeshGeometry3D mesh = (gm.Geometry as MeshGeometry3D);
                WriteLineWithIndent(sw, "<GeometryModel3D.Geometry>");
                WriteLineWithIndent(sw, "<MeshGeometry3D");
                // positions
                str = " Positions='";
                foreach (Point3D p in mesh.Positions)
                    str += p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString() + " ";
                str += "'";
                WriteLineWithIndent(sw, str);
                // texture coordinates
                str = " TextureCoordinates='";
                foreach (Point p in mesh.TextureCoordinates)
                    str += p.X.ToString() + "," + p.Y.ToString() + " ";
                str += "'";
                WriteLineWithIndent(sw, str);
                // triangle indices
                str = " TriangleIndices='";
                foreach (int i in mesh.TriangleIndices)
                    str += i.ToString() + " ";
                str += "'";
                WriteLineWithIndent(sw, str);
                WriteLineWithIndent(sw, "/>");
                WriteLineWithIndent(sw, "</GeometryModel3D.Geometry>");
                WriteLineWithIndent(sw, "</GeometryModel3D>");
            }

            // 尾
            WriteLineWithIndent(sw, "</Model3DGroup>");
            WriteLineWithIndent(sw, "</ModelVisual3D.Content>");
            WriteLineWithIndent(sw, "</ModelVisual3D>");
            WriteLineWithIndent(sw, "</ResourceDictionary>");
            sw.Close();
        }
    }
}
