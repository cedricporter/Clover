using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Clover;

namespace Clover.IO
{

    /// <summary>
    /// Clover文件分析器，分析方式和编译器中的语法分析过程类似
    /// </summary>
    class FileParser
    {
        BinaryReader reader;
        FileStream fs;
        VertexLayer vertexLayer;
        EdgeLayer edgeLayer;
        FaceLayer faceLayer;

        public void LoadFile(string filename)
        {
            fs = new FileStream(filename, FileMode.Open);

            reader = new BinaryReader(fs);
        }

        void Clover()
        {
            VertexLayer();

            EdgeLayer();

            FaceLayer();
        }

        void VertexLayer()
        {
            BinaryFormatter bf = new BinaryFormatter();

            vertexLayer = bf.Deserialize(fs) as VertexLayer;

            Debug.Assert(vertexLayer != null);
        }

        void EdgeLayer()
        {

        }

        void EdgeTree()
        {

        }

        void Edge()
        {

        }

        void FaceLayer()
        {

        }

        void FaceTree()
        {

        }

        void Face()
        {

        }


    }
}
