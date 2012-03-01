using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Mogre;
using MogreInWpf;
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
        MogreImage mogreImageSource;

        #region 编译选项

        public readonly bool InitMogreAsync = true;
        public static readonly Size StartupViewportSize = new Size(800, 600);
        public readonly bool AutoUpdateViewportSize = true;

        #endregion

        public SceneManager sceneManager;
        public SceneNode rootSceneNode;
        public SceneNode cloverRoot;
        public List<Camera> cameras = new List<Camera>();

        public CubeNavigator cubeNav;
        public List<ToolFactory> tools = new List<ToolFactory>();
        public ToolFactory currentTool = null;
        public ToolBox toolBox;

        #region 折纸部分

        // 抽象数据结构控制器
        CloverController cloverController;
        Paper paper;
        #endregion

        VisualController visualController;

        /// <summary>
        /// 场景创建
        /// </summary>
        private void CreateScene()
        {
            // 初始化全局变量
            Root root = MogreRootManager.GetSharedRoot();
            sceneManager = mogreImageSource.SceneManager = root.CreateSceneManager(SceneType.ST_GENERIC, "mainSceneManager");
            sceneManager.AmbientLight = new ColourValue(0.5f, 0.5f, 0.5f);
            rootSceneNode = sceneManager.RootSceneNode;
            cloverRoot = rootSceneNode.CreateChildSceneNode("cloverRoot");

            //  创建相机，将来或许会用到多相机
            cameras.Clear();
            Camera camera = sceneManager.CreateCamera("mainCamera");
            camera.AutoAspectRatio = true;
            camera.NearClipDistance = 5;
            camera.Position = new Vector3(0, 0, 200);
            camera.LookAt(new Vector3(0));
            cameras.Add(camera);

            // 创建视口，将来也可能会用到很多视口吗……
            // 嗯，有可能的。。。 —— Cedric Porter
            List<ViewportDefinition> vds = new List<ViewportDefinition>();
            vds.Add(new ViewportDefinition
            {
                Camera = camera,
                Left = 0,
                Top = 0,
                Width = 1,
                Height = 1,
                BackgroundColour = new ColourValue(0,0,0,0),
            });
            mogreImageSource.ViewportDefinitions = vds.ToArray();

            // 创建工具集
            tools.Clear();
            TestTool tl = new TestTool(this);
            tools.Add(tl);
            currentTool = tl;

            // 代码写这里
            

            // 初始化抽象数据结构，暂时先放在这里，到时再说了
            cloverController = new CloverController();
            cloverController.Initialize( 100, 100 );
            //cloverRoot.AttachObject(cloverController.Paper);
            cloverController.UpdatePaper();
        }

        /// <summary>
        /// 主循环，逻辑处理
        /// </summary>
        private void mogreImageSource_PreRender(object sender, EventArgs e)
        {
            if (visualController != null)
                visualController.Update();
        }

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
        /// 窗口初始化
        /// 在这里初始化Ogre引擎
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化 mogre image
            try
            {
                InitializeImage();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                this.Close();
            }
            // 初始化ogre
            if (InitMogreAsync)
            {
                mogreImageSource.InitOgreAsync(System.Threading.ThreadPriority.Normal, new RoutedEventHandler(OnOgreInitComplete));
            }
            else
            {
                mogreImageSource.InitOgreImage();
                OnOgreInitComplete(null, null);
            }

            toolBox.Left = Left + 10;
            toolBox.Top = Top + 70;
            toolBox.Show();
        }

        /// <summary>
        /// 初始化 mogre image
        /// </summary>
        private void InitializeImage()
        {
            mogreImageSource = new MogreImage();
            mogreImageSource.ViewportSize = PreferredMogreViewportSize;
            // 将下面的值改为true，并注释掉CreateScene，可以显示出一个ogre头……
            mogreImageSource.CreateDefaultScene = false;
            CreateScene();
            MogreImage.Source = mogreImageSource;
        }

        /// <summary>
        /// 当初始化完毕后希望做的工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnOgreInitComplete(Object sender, RoutedEventArgs args)
        {
            // 比如显示装载已完成……

            // 注册PreRender事件回调函数
            mogreImageSource.PreRender += new EventHandler(mogreImageSource_PreRender);
        }

        #endregion



        #region 变更ogre视窗大小

        public Size PreferredMogreViewportSize
        {
            get 
            {
                if (MogreImage.ActualHeight == 0 || MogreImage.ActualWidth == 0)
                    return StartupViewportSize;
                return new Size(MogreImage.ActualWidth, MogreImage.ActualHeight);
            }
        }

        /// <summary>
        /// 当MogreImage大小发生改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MogreImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mogreImageSource.ViewportSize = PreferredMogreViewportSize;
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
            try
            {

                MogreImage img = MogreImage.Source as MogreImage;
                if (img != null)
                {
                    img.Dispose();
                }
                MogreImage = null;

                MogreRootManager.DisposeSharedRoot();

                cloverRoot.Dispose();
                cloverRoot = null;
                sceneManager = null;
                rootSceneNode.Dispose();
                rootSceneNode = null;
                cameras.Clear();

                if (toolBox != null)
                    toolBox.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
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

        #endregion


        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        



    }
}
