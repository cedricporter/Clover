using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Mogre;
using OgreLib;



namespace Clover
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public SceneManager sceneManager;
        public SceneNode rootSceneNode;
        public SceneNode cloverRoot;

        public MainWindow()
        {
            InitializeComponent();
            App.Current.Exit += Current_Exit;
        }

        /// <summary>
        /// 窗口初始化
        /// 在这里初始化Ogre引擎
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            // 初始化ogre
            OgreRenderTarget.InitOgreAsync();
        }

        /// <summary>
        /// 释放Ogre引擎
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_Exit(object sender, ExitEventArgs e)
        {
            RenderTargetControl.Source = null;
            OgreRenderTarget.Dispose();
        }

        /// <summary>
        /// 当窗口大小发生变化时改变Ogre渲染窗口大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderTargetControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (OgreRenderTarget == null)
                return;
            OgreRenderTarget.ViewportSize = e.NewSize;
        }

        /// <summary>
        /// 当Ogre加载资源时，可以异步地显示加载进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ogre_ResourceLoadItemProgress(Object sender, ResourceLoadEventArgs e)
        {
            // todo
            //_progressName.Text = e.Name;
            //_progressScale.SclaeX = e.Progress;
        }

        /// <summary>
        /// Ogre场景创建
        /// 在这里向Ogre场景中添加物体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ogre_InitScene(Object sender, RoutedEventArgs e)
        {
            // 初始化一些全局变量
            sceneManager = OgreRenderTarget.SceneManager;
            rootSceneNode = sceneManager.RootSceneNode;
            cloverRoot = rootSceneNode.CreateChildSceneNode("cloverRoot");
        }

        /// <summary>
        /// Ogre渲染事件，在每一帧被绘制之前调用
        /// 可以将逻辑写在这里
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ogre_PreRender(Object sender, System.EventArgs e)
        {
            // todo
        }

        /// <summary>
        /// 处理鼠标事件，需重写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderTargetControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        
    }
}
