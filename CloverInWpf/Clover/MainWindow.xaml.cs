using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls;

using Clover.Tool;
using Clover.Visual;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Diagnostics;
using CloverPython;


namespace Clover
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        CubeNavigator cubeNav;
        List<ToolFactory> tools = new List<ToolFactory>();
        Utility utility;
        VisualController visualController;
        CloverInterpreter cloverInterpreter = new CloverInterpreter();

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
            VisualController.Initialize(this);
            visualController = VisualController.GetSingleton();
            //TextVisualElement vi = new TextVisualElement("Fuck", new Point(200, 200), (SolidColorBrush)App.Current.FindResource("TextBlueBrush"));
            //visualController.AddVisual(vi);
            //vi.Start();

            // 导航立方
            CubeNavigator.InitializeInstance(this);
            cubeNav = CubeNavigator.GetInstance();    

            // 创建工具
            ToolFactory tool = new TestTool(this);
            tools.Add(tool);
            tool = new FoldTool(this);
            tools.Add(tool);
            ToolFactory.currentTool = tool;

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
            //toolBox = new ToolBox(this);
            //toolBox.Left = Left + toolBoxRelLeft;
            //toolBox.Top = Top + toolBoxRelTop;
            //toolBox.Show();

            // 更新矩阵
            utility.UpdateProjViewMat(foldingPaperViewport.ActualHeight, foldingPaperViewport.ActualWidth);
 
            // 初始化纸张
            CloverController.InitializeInstance(this);
            cloverController = CloverController.GetInstance();
            cloverController.Initialize(100, 100);
            cloverController.UpdatePaper();
            foldingPaperViewport.Children.Add(cloverController.Model);

            cloverInterpreter.InitialzeInterpreter();

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

            if (ToolFactory.currentTool != null)
                ToolFactory.currentTool.onIdle();
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

        #region 移动子窗口
        
        Point lastMousePos1 = new Point(-100, -100);
        Grid capturedGrid = null;
        private void Grid_Capture(Object sender, MouseButtonEventArgs e)
        {
            capturedGrid = (Grid)((Grid)sender).Parent;
        }
        private void Grid_Lost(Object sender, MouseButtonEventArgs e)
        {
            lastMousePos1.X = lastMousePos1.Y = -100;
            capturedGrid = null;
        }
        private void Grid_Move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || capturedGrid == null)
                return;
            Point currMousePos = e.GetPosition(this);
            if (lastMousePos1.X == -100 && lastMousePos1.Y == -100)
                lastMousePos1 = currMousePos;
            Double lastX = capturedGrid.RenderTransform.Value.OffsetX;
            Double lastY = capturedGrid.RenderTransform.Value.OffsetY;
            Vector offset = currMousePos - lastMousePos1;
            capturedGrid.RenderTransform = new TranslateTransform(offset.X + lastX, offset.Y + lastY);
            lastMousePos1 = currMousePos;
        }

        #endregion

        /// <summary>
        /// 当鼠标在折纸视口上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (ToolFactory.currentTool != null)
                ToolFactory.currentTool.onMove();

            Grid_Move(sender, e);
        }

        /// <summary>
        /// 当鼠标按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ToolFactory.currentTool != null)
                ToolFactory.currentTool.onPress();
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ToolFactory.currentTool != null)
                ToolFactory.currentTool.onDoubleClick();
        }

        /// <summary>
        /// 改变折纸的距离
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
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
                case Key.F2:
                    cloverController.Revert();
                    break;
                case Key.F1:
                    cloverController.StartFoldingModel(null);
                    break;
                case Key.F3:
                    cloverController.NeilTest();
                    break;
                case Key.F4:
                    cloverController.CutAFaceWithAddedTwoVertices(cloverController.FaceLayer.Leaves[0], new Edge(new Vertex(-50, 0, 0), new Vertex(50, 0, 0)));
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
        

        void FuckingKey(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                string output = cloverInterpreter.ExecuteOneLine(commandLineTextBox.Text);
                histroyTextBox.Text += commandLineTextBox.Text + "\n" + "[ " + output + " ]\n";
                //histroyTextBox.Text = commandLineTextBox.Text + "\n--> " + output + "\n" + histroyTextBox.Text;
                commandLineTextBox.Text = "";
                histroyTextBox.ScrollToEnd();
            }
            //e.Handled = true;
        }   
        

    }
}
