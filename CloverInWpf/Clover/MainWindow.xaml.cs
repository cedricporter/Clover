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

        CubeNavigator cubeNav;
        List<ToolFactory> tools = new List<ToolFactory>();
        public ToolFactory currentTool = null;
        Utility utility;
        VisualController visualController;

        #region 工具窗

        public ToolBox toolBox;
        public Double toolBoxRelLeft = 10;
        public Double toolBoxRelTop = 70;

        #endregion
        
        #region 折纸部分

        // 抽象数据结构控制器
        public CloverController cloverController;

        #endregion

        #region 构造和初始化

        public MainWindow()
        {
            InitializeComponent();
            
            // 测试Visual
            visualController = VisualController.GetSingleton(this);
            //TextVisualElement vi = new TextVisualElement("Fuck", new Point(200, 200), (SolidColorBrush)App.Current.FindResource("TextBlueBrush"));
            //visualController.AddVisual(vi);
            //vi.Start();

            // 导航立方
            CubeNavigator.InitializeInstance(this);
            cubeNav = CubeNavigator.GetInstance();    

            // 创建工具
            ToolFactory tool = new TestTool(this);
            tools.Add(tool);
            currentTool = tool;

            // 初始化纸张
            CloverController.InitializeInstance(this);
            cloverController = CloverController.GetInstance();
            cloverController.Initialize(100, 100);
            cloverController.UpdatePaper();
            foldingPaperViewport.Children.Add(cloverController.Model);

            // 杂项
            utility = Utility.GetInstance();
            utility.UpdateWorlCameMat();

            // 注册回调函数
            CompositionTarget.Rendering += MainLoop;

            

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
            //toolBox.Show();

            // 更新矩阵
            utility.UpdateProjViewMat(foldingPaperViewport.ActualHeight, foldingPaperViewport.ActualWidth);

            VertexInfoVisual vi = new VertexInfoVisual(cloverController.Edges[0].Vertex1);
            visualController.AddVisual(vi);
            vi.Start();

            this.Focus();
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 主循环
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainLoop(Object sender, EventArgs e)
        {
            //RenderingEventArgs re = (RenderingEventArgs)e;
            //Debug.WriteLine(re.RenderingTime);
            visualController.Update();

            RenderController.GetInstance().RenderAnimations();
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
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentTool != null)
                currentTool.onMove();

            //Matrix3D projViewMatIn = Utility.GetInstance().ProjViewMat;
            //if (!projViewMatIn.HasInverse)
            //    return;
            //projViewMatIn.Invert();
            //Point3D start = new Point3D(e.GetPosition(this.foldingPaperViewport).X, e.GetPosition(this.foldingPaperViewport).Y, -1);
            //start.X -= foldingPaperViewport.ActualWidth / 2;
            //start.Y -= foldingPaperViewport.ActualHeight / 2;            
            //Point3D mid = new Point3D(start.X, start.Y, 0);
            //start *= projViewMatIn;
            //mid *= projViewMatIn;
            //Vector3D dir = mid - start;
            //dir.Normalize();
            //Debug.WriteLine(start);
            //Debug.WriteLine(dir);
            //start.Z += 300;
            //mid.Z += 300;
            //Point3D intersection = CloverMath.IntersectionOfLineAndPlane(start, mid, cloverController.FaceLeaves[0]);
            //Debug.WriteLine(intersection);

            //Debug.WriteLine("======================");
            //Debug.WriteLine(Mouse.GetPosition(this));
            //Point3D p = cloverController.Edges[0].Vertex1.GetPoint3D();
            //Debug.WriteLine(p);
            //Matrix3D mat = Utility.GetInstance().To2DMat;
            //p *= mat;
            //Debug.WriteLine(p);
            //mat.Invert();
            //p *= mat;
            //Debug.WriteLine(p);   
        }

        /// <summary>
        /// 当鼠标左键按下，，
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentTool != null)
                currentTool.onPress();
        }

        /// <summary>
        /// 改变折纸的距离
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            RenderController renCtrl = RenderController.GetInstance();
            if (e.Delta > 0)
            {
                renCtrl.Distance += 20;
                //renCtrl.UpdatePosition();
            }
            else
            {
                renCtrl.Distance -= 20;
                //renCtrl.UpdatePosition();
            }
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

        #region 键盘响应函数

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.R:
                    cloverController.Revert();
                    break;
                case Key.G:
                    cloverController.StartFoldingModel(null);
                    break;
                case Key.X:
                    cloverController.NeilTest();
                    break;
                case Key.Up:
                    cloverController.Update(0, 10, null, null);
                    //cloverController.UpdateVertexPosition(null, 0, 10);
                    break;
                case Key.Down:
                    cloverController.Update(0, -10, null, null);
                    //cloverController.UpdateVertexPosition(null, 0, -10);
                    break;
                case Key.Left:
                    //cloverController.UpdateVertexPosition(null, -10, 0);
                    break;
                case Key.Right:
                    //cloverController.UpdateVertexPosition(null, 10, 0);
                    break;
            }

            //cloverController.RenderController.UpdatePosition();
        }

        #endregion
        

        

    }
}
