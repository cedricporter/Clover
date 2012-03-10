using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover.IO
{
    class IOController
    {
        #region 单例

        static IOController instance = null;
        public static IOController GetInstance()
        {
            if (instance == null)
                instance = new IOController();
            return instance;
        }
        IOController()
        { }

        #endregion

        #region 只读属性
        Boolean isFileModified = true;
        public System.Boolean IsFileModified
        {
            get { return isFileModified; }
        }
        #endregion

        /// <summary>
        /// 从硬盘读入一个Clover工程
        /// </summary>
        public void ReadCloverFile(String Path)
        {

            //switch (state)
            //{
            //    case Ini:
            //        nextTrunk = ReadTrunkId();
            //        state = nextTrunk;
            //        break;
            //    case trunk1:
            //        while (!trunkEnd)
            //            ReadTrunkData();
            //        state = Ini;
            //        break;
            //        ...
            //        ...
            //}
        }

        /// <summary>
        /// 将一个Clover工程写入硬盘
        /// </summary>
        public void WriteCloverFile(String Path)
        {

        }

        /// <summary>
        /// 导出带折线提示的折纸图片
        /// </summary>
        public void ExportImage(String Path)
        {

        }

        /// <summary>
        /// 导出折叠脚本
        /// </summary>
        public void ExportScript(String Path)
        {

        }

    }
}
