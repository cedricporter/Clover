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
        #region get/set
        public Clover.FaceLayer FaceLayer
        {
            get { return parser.FaceLayer; }
        }
        public Clover.EdgeLayer EdgeLayer
        {
            get { return parser.EdgeLayer; }
        }
        public Clover.VertexLayer VertexLayer
        {
            get { return parser.VertexLayer; }
        }
        #endregion

        FileParser parser = new FileParser();

        public void LoadFromFile(string filename)
        {
            parser.LoadFile(filename);

        }
    }
}
