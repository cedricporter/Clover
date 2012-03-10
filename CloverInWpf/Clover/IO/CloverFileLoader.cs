using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.IO
{
    /// <summary>
    /// Clover文件读取器
    /// </summary>
    class CloverFileLoader
    {
        FaceLayer faceLayer = new FaceLayer();
        EdgeLayer edgeLayer = new EdgeLayer();
        VertexLayer vertexLayer = new VertexLayer();


        FileParser parser = new FileParser();

        public void LoadFromFile(string filename)
        {
            parser.LoadFile(filename);

        }
    }
}
