using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Mogre;
using MogreInWpf;



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

        // 抽象数据结构控制器
        CloverController cloverController = new CloverController();

        /// <summary>
        /// 场景创建
        /// </summary>
        private void CreateScene()
        {
            // 初始化抽象数据结构，暂时先放在这里，到时再说了
            cloverController.Initialize( 100, 100 );

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

            // 代码写这里
            cubeNav = new CubeNavigator(this);
            //this.AddChild(cubeNav);
        }

        /// <summary>
        /// 主循环，逻辑处理
        /// </summary>
        private void mogreImageSource_PreRender(object sender, EventArgs e)
        {

        }

        #region 构造和初始化

        public MainWindow()
        {
            InitializeComponent();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SDF");
        }


        
    }
}
