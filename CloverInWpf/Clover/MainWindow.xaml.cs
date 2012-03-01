using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Clover.Tool;
using Clover.Visual;
using Clover.RenderLayer;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Diagnostics;



namespace Clover
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public CubeNavigator cubeNav;
        public List<ToolFactory> tools = new List<ToolFactory>();
        public ToolFactory currentTool = null;
        public Utility utility;

        #region 工具窗

        public ToolBox toolBox;
        public Double toolBoxRelLeft = 10;
        public Double toolBoxRelTop = 70;

        #endregion
        
        #region 折纸部分

        // 抽象数据结构控制器
        CloverController cloverController;
        public Clover.CloverController CloverController
        {
            get { return cloverController; }
        }
        //Paper paper;

        #endregion


        #region 构造和初始化

        public MainWindow()
        {
            InitializeComponent();
            
            //toolBox.Show();
            // 测试Visual
            //visualController = VisualController.GetSingleton(this);
            //TextVisualElement vi = new TextVisualElement("Fuck", new Point(200, 200), (SolidColorBrush)App.Current.FindResource("TextBlueBrush"));
            //visualController.AddVisual(vi);
            //vi.Start();

            // 导航立方
            cubeNav = new CubeNavigator(this);

            // 初始化纸张
            cloverController = new CloverController(this);
            cloverController.Initialize(100, 100);
            cloverController.UpdatePaper();
            foldingPaperViewport.Children.Add(cloverController.Model);

            // 杂项
            utility = new Utility(this);
            utility.UpdateWorlCameMat(-cloverController.RenderController.Entity.Transform.Value.OffsetZ);

            stopwatch.Start();
            statsTimer = new System.Windows.Threading.DispatcherTimer(TimeSpan.FromSeconds(1), System.Windows.Threading.DispatcherPriority.Normal,
                new EventHandler(FrameRateDisplay), this.Dispatcher);
            CompositionTarget.Rendering += FrameCountPlusPlus;
        }

        ~MainWindow()
        {
            //System.Windows.MessageBox.Show("Fuck");
        }

        /// <summary>
        /// 窗口初始化完毕后的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 各种窗口
            toolBox = new ToolBox(this);
            toolBox.Left = Left + toolBoxRelLeft;
            toolBox.Top = Top + toolBoxRelTop;
            toolBox.Show();
            
        }

        #endregion

        #region 监视帧率

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        int frameCount = 0;
        float Fps = 0;
        System.Windows.Threading.DispatcherTimer statsTimer;
        void FrameRateDisplay(Object sender, EventArgs e)
        {
            long elapsed = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            if (elapsed != 0)
                Fps = frameCount * 1000.0f / elapsed;
            frameCount = 0;
            this.Title = "Clover Current Fps:";
            this.Title += Fps.ToString("#.00");
        }
        void FrameCountPlusPlus(Object sender, EventArgs e)
        {
            frameCount++;
        }

        #endregion

        #region 释放资源

        /// <summary>
        /// 窗口关闭时关闭所有子窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (toolBox != null)
                toolBox.Close();
        }

        #endregion

        #region 鼠标事件响应函数

        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            // 使工具窗位置相对主窗口静止
            if (toolBox != null)
            {
                toolBox.Left = Left + toolBoxRelLeft;
                toolBox.Top = Top + toolBoxRelTop;
            }
        }

        /// <summary>
        /// 当鼠标在折纸视口上。。。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foldingPaperViewport_MouseMove(object sender, MouseEventArgs e)
        {
            //Viewport3DVisual viewport;
            //Boolean success;
            //Matrix3D visualToScreen = MathUtils.TryTransformTo2DAncestor(foldingPaperViewport.Children[0], out viewport, out success);
            //if (success)
            //{
            //    Point3D p1 = cloverController.Edges[0].Vertex1.GetPoint3D() * visualToScreen;
            //    Point3D p2 = cloverController.Edges[0].Vertex2.GetPoint3D() * visualToScreen;
            //    Debug.WriteLine("======================");
            //    Debug.Write(p1);
            //    Debug.Write("      ");
            //    Debug.WriteLine(p2);
            //    Debug.WriteLine(Mouse.GetPosition(foldingPaperViewport));
            //}
        }

        #endregion

        #region 主窗体大小改变

        /// <summary>
        /// 当窗口大小发生改变时……
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 使工具窗位置相对主窗口静止
            if (toolBox != null)
            {
                toolBox.Left = Left + toolBoxRelLeft;
                toolBox.Top = Top + toolBoxRelTop;
            }
            // 更新矩阵
            utility.UpdateProjViewMat(foldingPaperViewport.ActualHeight, foldingPaperViewport.ActualWidth);
        }

        #endregion

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //Viewport3DVisual viewport;
            //Boolean success;
            //Matrix3D visualToScreen = MathUtils.TryTransformTo2DAncestor(foldingPaperViewport.Children[0], out viewport, out success);
            ////if (success)
            ////{

            //Point3D p1 = cloverController.Edges[0].Vertex1.GetPoint3D() * utility.To2DMat;
            //Point3D p2 = cloverController.Edges[0].Vertex2.GetPoint3D() * utility.To2DMat;
            //    Debug.WriteLine("======================");
            //    Debug.Write(p1);
            //    Debug.Write("      ");
            //    Debug.WriteLine(p2);
            //    //Debug.WriteLine(Mouse.GetPosition(foldingPaperViewport));
            //    Debug.WriteLine(Mouse.GetPosition(foldingPaperViewport));
            ////}
        }

        

    }
}
