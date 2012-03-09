﻿using System;
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
using System.Windows.Media.Animation;


namespace Clover
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        CubeNavigator cubeNav;
        PaperSelector paperSelector;
        List<ToolFactory> tools = new List<ToolFactory>();
        Utility utility;
        VisualController visualController;
        CloverInterpreter cloverInterpreter = new CloverInterpreter();
        
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
            // 纸张纹理选择器
            PaperSelector.InitializeInstance(this);
            paperSelector = PaperSelector.GetInstance();
            paperSelector.LoadPaperTextures("media/paper");

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
        }

        #region 处理子窗口的移动，缩放，关闭等
        
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
        private void Gird_Close(Object sender, RoutedEventArgs e)
        {
            capturedGrid = (Grid)((Grid)((Button)sender).Parent).Parent;
            capturedGrid.Visibility = Visibility.Hidden;
        }
        private void Grid_Move(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || capturedGrid == null)
                return;
            Point currMousePos = e.GetPosition(this);
            if (lastMousePos1.X == -100 && lastMousePos1.Y == -100)
                lastMousePos1 = currMousePos;
            Vector offset = currMousePos - lastMousePos1;
            Grid_SizeOrPositionChange(offset, new Vector(0, 0));
            lastMousePos1 = currMousePos;
        }
        private void Grid_SizeOrPositionChange(Vector translateOffset, Vector scaleOffset)
        {
            //TransformGroup tsg = new TransformGroup();
            //tsg.Children.Add(new TranslateTransform(translateOffset.X + lastX, translateOffset.Y + lastY));
            TranslateTransform ts = ((capturedGrid.RenderTransform as TransformGroup).Children[1] as TranslateTransform);
            ts.X += translateOffset.X;
            ts.Y += translateOffset.Y;
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

            cloverController.Table.UpdateTableAfterFoldUp( true );
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
                renCtrl.TranslateTransform.OffsetZ -= 20;
            }
            else
            {
                renCtrl.TranslateTransform.OffsetZ += 20;
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
                    cloverController.ShadowSystem.Undo();
                    break;
                case Key.F1:
                    string code = @"
vertex = GetVertex(0)
face = FindFacesByVertex(0)
edge = GetFoldingLine(face[0], vertex, Vertex(0, 0))

CutFaces(face, edge)

faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 90)
                    ";
                    string msg = cloverInterpreter.ExecuteOneLine(code);
                    histroyTextBox.Text += msg;
                    break;
                case Key.F3:
                    cloverController.ShadowSystem.Redo();
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

        #region 窗口开启关闭选项

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            ToolBox.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeIn"));
        }

        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            ToolBox.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        private void MenuItem_Checked_1(object sender, RoutedEventArgs e)
        {
            CommandLine.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeIn"));
        }

        private void MenuItem_Unchecked_1(object sender, RoutedEventArgs e)
        {
            CommandLine.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
            if (CommandLineMenuItem.IsChecked == true)
                CommandLineMenuItem.IsChecked = false;
        }

        private void MenuItem_Checked_2(object sender, RoutedEventArgs e)
        {
            Output.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeIn"));
        }

        private void MenuItem_Unchecked_2(object sender, RoutedEventArgs e)
        {
            Output.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
            if (OutputMenuItem.IsChecked == true)
                OutputMenuItem.IsChecked = false;
        }

        #endregion

        #region 工具栏按钮

        private void ToolFodeButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToolFodeButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ToolTuckButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToolTuckButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ToolBlendButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToolBlendButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void BeginMacroButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BeginMacroButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void MagnetismButton_Checked(object sender, RoutedEventArgs e)
        {
            AbstractLayer.Magnet.IsMagnetismEnable = true;
        }

        private void MagnetismButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AbstractLayer.Magnet.IsMagnetismEnable = false;
        }

        private void ChangePaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaperSelect.Visibility == Visibility.Hidden)
                PaperSelect.Visibility = Visibility.Visible;
            else
                PaperSelect.Visibility = Visibility.Hidden;
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
