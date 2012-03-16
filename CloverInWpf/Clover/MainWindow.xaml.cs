using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading;

using Clover.Tool;
using Clover.Visual;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Diagnostics;
using CloverPython;
using System.Windows.Media.Animation;
using Clover.IO;


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
        IOController ioController;
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
            // IO控制器
            IOController.InitializeInstance(this);
            ioController = IOController.GetInstance();

            // 创建工具
            ToolFactory tool = new FoldTool(this);
            tools.Add(tool);
            ToolFactory.currentTool = tool;
            tool = new BlendTool(this);
            tools.Add(tool);

            // 杂项
            utility = Utility.GetInstance();
            utility.UpdateWorlCameMat();

            //BlendAngleVisual vi = new BlendAngleVisual(0, new Point(100, 100), new Point(100, 200));
            //vi.Start();
            //visualController.AddVisual(vi);

            // 注册回调函数
            CompositionTarget.Rendering += MainLoop;
            MouseMove += Window_TranslatePaper;

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
            // 更新矩阵
            utility.UpdateProjViewMat(foldingPaperViewport.ActualHeight, foldingPaperViewport.ActualWidth);

            // 初始化纸张
            CloverController.InitializeInstance(this);
            cloverController = CloverController.GetInstance();
            cloverController.Initialize(100, 100);
            foldingPaperViewport.Children.Add(cloverController.Model);
            foldingPaperViewport.Children.Add(cloverController.ShadowModel);
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

        Point lastMousePos2;

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

            return;
        }

        /// <summary>
        /// 右键平移纸张
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Window_TranslatePaper(Object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // 计算纸张的平移量
                Point currMousePos = e.GetPosition(this);
                if (currMousePos.Equals(lastMousePos2))
                    return;
                Vector offsetVec = currMousePos - lastMousePos2;
                offsetVec /= 3;
                RenderController.GetInstance().TranslateTransform.OffsetX += offsetVec.X;
                RenderController.GetInstance().TranslateTransform.OffsetY -= offsetVec.Y;
                // 点亮箭头
                if (offsetVec.X > 0)
                    (OffsetArrows.Children[3] as Image).BeginStoryboard((Storyboard)FindResource("LightUpArrow"));
                else if (offsetVec.X < 0)
                    (OffsetArrows.Children[2] as Image).BeginStoryboard((Storyboard)FindResource("LightUpArrow"));
                if (offsetVec.Y > 0)
                    (OffsetArrows.Children[1] as Image).BeginStoryboard((Storyboard)FindResource("LightUpArrow"));
                else if (offsetVec.Y < 0)
                    (OffsetArrows.Children[0] as Image).BeginStoryboard((Storyboard)FindResource("LightUpArrow"));

                lastMousePos2 = currMousePos;
            }
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

            lastMousePos2 = e.GetPosition(this);
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (ToolFactory.currentTool != null)
            //    ToolFactory.currentTool.onDoubleClick();

            //cloverController.Table.UpdateTableAfterFoldUp(true);
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
edge = Edge(Vertex(0, 50, 0), Vertex(-50, 30, 0))
faces = FindFacesByVertex(0)
CutFaces(faces, edge)

faces = FindFacesByVertex(0)
RotateFaces(faces, edge, 180)

clover.UpdateFaceGroupTable()
                    ";
                    string msg = cloverInterpreter.ExecuteOneLine(code);
                    histroyTextBox.Text += msg;
                    break;
                case Key.F3:
                    cloverController.ShadowSystem.Redo();
                    break;
                case Key.F4:
                    RenderController.GetInstance().testSuck(TestSuckImage, foldingPaperViewport);
                    break;
                //case Key.Up:
                //    cloverController.Update(0, 10, null, null);
                //    //cloverController.UpdateVertexPosition(null, 0, 10);
                //    break;
                //case Key.Down:
                //    cloverController.Update(0, -10, null, null);
                //    //cloverController.UpdateVertexPosition(null, 0, -10);
                //    break;
                case Key.Left:
                    //cloverController.UpdateVertexPosition(null, -10, 0);
                    break;
                case Key.F11:
                    //cloverController.UpdateVertexPosition(null, 10, 0);
                    //Face f = cloverController.FaceLayer.Leaves[0].Clone() as Face;
                    //f.UpdateVertices();
                    var a = cloverController.FaceLayer;
                    // cloverController.FaceGroupLookupTable.UpdateTableAfterFoldUp();
                    //cloverController.RenderController.UpdateAll();
                    cloverController.RenderController.AntiOverlap();
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
            if (CommandLineMenuItem.IsChecked == false)
                CommandLineMenuItem.IsChecked = true;
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
            if (OutputMenuItem.IsChecked == false)
                OutputMenuItem.IsChecked = true;
        }

        private void MenuItem_Unchecked_2(object sender, RoutedEventArgs e)
        {
            Output.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
            if (OutputMenuItem.IsChecked == true)
                OutputMenuItem.IsChecked = false;
        }

        private void ExportTexture_Show(object sender, RoutedEventArgs e)
        {
            ExportFrontPreviewImg.Source = cloverController.RenderController.GetFrontTexture();
            ExportBackPreviewImg.Source = cloverController.RenderController.GetBackTexture();
            ExportTexture.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeIn"));
        }

        private void ExportTexture_Hide(object sender, RoutedEventArgs e)
        {
            ExportTexture.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        private void ExportTexture_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowDialog();
            TextureExportPath.Text = folderDialog.SelectedPath;
        }

        private void ExportTexture_Export(object sender, RoutedEventArgs e)
        {
            //ExportTexture_Text.Visibility = Visibility.Visible;
            ioController.ExportTexture(TextureExportPath.Text);
            ExportTexture.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        private void ExportTexture_Cancle(object sender, RoutedEventArgs e)
        {
            ExportTexture.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        /// <summary>
        /// 打开Clover文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Clover File|*.clr";
            dialog.ShowDialog();
            if (dialog.FileName == "")
                return;
            cloverController.LoadFile(dialog.FileName);
        }

        /// <summary>
        /// 保存Clover文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Clover File|*.clr";
            dialog.ShowDialog();
            if (dialog.FileName == "")
                return;
            cloverController.SaveFile(dialog.FileName);
        }

        private void NewPaper_New(object sender, RoutedEventArgs e)
        {
            NewPaper.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        private void NewPaper_Canle(object sender, RoutedEventArgs e)
        {
            NewPaper.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeOut"));
        }

        private void NewPaper_Show(object sender, RoutedEventArgs e)
        {
            NewPaperTexturePreview.Source = paperSelector.InfoList[0].bmp;
            NewPaper.BeginStoryboard((Storyboard)App.Current.FindResource("WindowFadeIn"));
        }

        #endregion

        #region 工具栏按钮

        private void ToolFodeButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tools.Count >= 2)
                ToolFactory.currentTool = tools[0];
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
            if (tools.Count >= 2)
                ToolFactory.currentTool = tools[1];
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

        #region 脚本引擎
        class CloverInteruptThreadPackage
        {
            CloverInterpreter interpreter;
            string command;
            MainWindow window;

            public CloverInteruptThreadPackage(MainWindow window, CloverInterpreter interpreter, string command)
            {
                this.window = window;
                this.interpreter = interpreter;
                this.command = command;
            }

            public void Run()
            {
                string output = interpreter.ExecuteOneLine(command);
                //window.Dispatcher.Invoke(new SetOutputText(window.histroyTextBox
                window.SetOutputText(output);
            }
        }

        delegate void SetOutputTextHandle(string text);

        void SetOutputText(string text)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new SetOutputTextHandle(SetOutputText), text);
            }
            else
            {
                histroyTextBox.Text += text + "\n";
                commandLineTextBox.Text = "";
                histroyTextBox.ScrollToEnd();
            }
        }


        void commandKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                //RenderController.GetInstance().BeginRotatePaperY(true);
                CloverInteruptThreadPackage itr = new CloverInteruptThreadPackage(this, cloverInterpreter, commandLineTextBox.Text);
                Thread thread = new Thread(itr.Run);
                thread.Start();
            }
            else if (e.Key == Key.F6)
            {
                string output = cloverInterpreter.ExecuteOneLine(commandLineTextBox.Text);
                histroyTextBox.Text += commandLineTextBox.Text + "\n" + "[ " + output + " ]\n";
            }
        }
        #endregion

        #region Cube导航相关接口

        private void Cube_Click_Front(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("ft");
        }

        private void Cube_Click_Back(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("bk");
        }

        private void Cube_Click_Up(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("up");
        }

        private void Cube_Click_Down(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("dn");
        }

        private void Cube_Click_Left(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("lt");
        }

        private void Cube_Click_Right(object sender, MouseButtonEventArgs e)
        {
            cubeNav.RotateTo("rt");
        }

        #endregion

        #region Arrow导航相关接口

        private void Arrow_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Image).BeginStoryboard((Storyboard)FindResource("EnterArrow"));
        }

        private void Arrow_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Image).BeginStoryboard((Storyboard)FindResource("LeaveArrow"));
        }

        private void LeftArrow_Press(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().BeginTranslateSlerp("lt");
        }

        private void RightArrow_Press(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().BeginTranslateSlerp("rt");
        }

        private void UpArrow_Press(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().BeginTranslateSlerp("up");
        }

        private void DownArrow_Press(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().BeginTranslateSlerp("dn");
        }

        private void ResetArrow_Press(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().BeginTranslateSlerp("rs");
        }

        private void Arrow_MouseUp(Object sender, MouseButtonEventArgs e)
        {
            RenderController.GetInstance().EndTranslateSlerp();
        }

        #endregion

        #region 快捷键

        /// <summary>
        /// 打开关闭调试窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchDebugWindows_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CommandLineMenuItem.IsChecked == false || OutputMenuItem.IsChecked == false)
            {
                MenuItem_Checked_1(null, null);
                MenuItem_Checked_2(null, null);
            }
            else
            {
                MenuItem_Unchecked_1(null, null);
                MenuItem_Unchecked_2(null, null);
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCloverFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem_Click_1(null, null);
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCloverFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem_Click(null, null);
        }

        /// <summary>
        /// 新建文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCloverFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewPaper_Show(null, null);
        }

        /// <summary>
        /// 重置视角
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCamera_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RenderController.GetInstance().BeginRotationSlerp(new Quaternion());
            RenderController.GetInstance().BeginTranslateSlerp("rs");  
        }

        #endregion
        

        

    }
}
