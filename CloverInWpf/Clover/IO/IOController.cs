using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Clover.IO
{
    class IOController
    {
        #region 单例

        static IOController instance = null;
        static MainWindow mainWindow = null;
        public static void InitializeInstance(MainWindow mainWindow)
        {
            IOController.mainWindow = mainWindow;
        }
        public static IOController GetInstance()
        {
            if (instance == null)
            {
                Debug.Assert(mainWindow != null);
                instance = new IOController();
            }
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

        #region get/set
        string projectName = "CloverProject1";
        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
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
        public void ExportTexture(String path)
        {
            // 正面
            FileStream stream = new FileStream(path + "/" + projectName + "_front.jpg", FileMode.Create);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)mainWindow.ExportFrontPreviewImg.Source));
            encoder.Save(stream);
            stream.Close();
            // 背面
            stream = new FileStream(path + "/" + projectName + "_back.jpg", FileMode.Create);
            encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)mainWindow.ExportBackPreviewImg.Source));
            encoder.Save(stream);
            stream.Close();

            //mainWindow.ExportTexture_Text.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 导出模型的xaml文件
        /// </summary>
        /// <param name="path"></param>
        public void ExportModelXaml(String path)
        {
            ModelExporter.Export(path);
        }

        /// <summary>
        /// 导出折叠脚本
        /// </summary>
        public void ExportScript(String Path)
        {

        }

    }
}
