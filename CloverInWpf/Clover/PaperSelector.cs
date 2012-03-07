using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Input;
//using System.Windows.Shapes;


/**
@date		:	2012/03/03
@filename	: 	MaterialController.cs
@author		:	屠文翔	
			 -     _     - 
			| | - (_) __| |
			| |/ /| |/ _  |
			|   < | |||_| |
			|_|\_\|_|\____|
@note		:	选择纸张纹理
**/


namespace Clover
{
    class PaperSelector
    {
        #region 单例
        
        static MainWindow mainWindow = null;
        static PaperSelector instance = null;
        public static void InitializeInstance(MainWindow mainWindow)
        {
            PaperSelector.mainWindow = mainWindow;
        }
        public static PaperSelector GetInstance()
        {
            if (instance == null)
            {
                Debug.Assert(mainWindow != null);
                instance = new PaperSelector();
            }
            return instance;
        }

        PaperSelector()
        { }

        #endregion

        /// <summary>
        /// 从指定路径扫描纹理文件
        /// </summary>
        /// <param name="path">扫描路径</param>
        public void LoadPaperTextures(String path)
        {
            String[] files;

            // 判断路径是否存在
            if (Directory.Exists(path) == false)
            {
                System.Windows.MessageBox.Show("你扫描插件的路径：" + path + "不存在，在该路径扫描插件失败。");
                return;
            }

            files = Directory.GetFiles(path);

            Rect rect;
            DrawingVisual dv;
            DrawingContext dc;
            RenderTargetBitmap bmp;
            Image img;
            foreach (String file in files)
            {
                if (Path.GetExtension(file) != ".png" && Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".bmp")
                    return;

                // 装载图片
                ImageSource imgSource = new BitmapImage(new Uri(@file, UriKind.Relative));
                if (imgSource == null)
                    continue;

                // 将图片处理成略缩图
                rect = new Rect(new Size(100, 100));
                dv = new DrawingVisual();
                dc = dv.RenderOpen();
                dc.DrawImage(imgSource, rect);
                dc.Close();
                bmp = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(dv);

                // 将略缩图加入菜单
                img = new Image();
                img.Source = bmp;
                System.Windows.Shapes.Rectangle hl = new System.Windows.Shapes.Rectangle();
                hl.Fill = new SolidColorBrush(Color.FromArgb(100, 0, 116, 255));
                hl.Height = hl.Width = 100;
                hl.Visibility = Visibility.Hidden;
                Canvas box = new Canvas();
                box.Height = box.Width = 100;
                box.Margin = new Thickness(5);
                box.Cursor = Cursors.Hand;
                box .Effect = (Effect)App.Current.FindResource("DropShadowEffect1px");
                box.Children.Add(img);
                box.Children.Add(hl);
                box.MouseLeftButtonDown += OnPreviewClick;
                box.MouseEnter += OnPreviewEnter;
                box.MouseLeave += OnPreviewLeave;
                mainWindow.PaperPreviewPanel.Children.Add(box);
            }
        }

        void OnPreviewEnter(Object sender, EventArgs e)
        {
            (sender as Canvas).Children[1].Visibility = Visibility.Visible;
        }

        void OnPreviewLeave(Object sender, EventArgs e)
        {
            (sender as Canvas).Children[1].Visibility = Visibility.Hidden;
        }

        void OnPreviewClick(Object sender, EventArgs e)
        {

        }

    }
}
