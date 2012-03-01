using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Clover.Tool;
using Clover.Visual;
using System.Windows.Media;



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
        public ToolBox toolBox;

        #region 折纸部分

        // 抽象数据结构控制器
        CloverController cloverController;
        Paper paper;
        #endregion


        #region 构造和初始化

        public MainWindow()
        {
            InitializeComponent();
            // 各种窗口
            // 窗口
            toolBox = new ToolBox(this);
            //toolBox.Show();
            // 测试Visual
            //visualController = VisualController.GetSingleton(this);
            //TextVisualElement vi = new TextVisualElement("Fuck", new Point(200, 200), (SolidColorBrush)App.Current.FindResource("TextBlueBrush"));
            //visualController.AddVisual(vi);
            //vi.Start();


            // 导航立方
            cubeNav = new CubeNavigator(this);
            

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
            toolBox.Left = Left + 10;
            toolBox.Top = Top + 70;
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

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentTool != null)
                currentTool.onMove();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentTool != null)
                currentTool.onPress();
        }

        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion



    }
}
