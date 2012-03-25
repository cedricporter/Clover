using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace BallAndStickTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 成员变量

        Ball[,] balls1, balls2, currBalls, lastBalls;
        Ellipse[,] drawingBalls;
        Stick[] sticks;

        Int32 width = 10;
        Int32 height = 1;
        Int32 numOfSticks = 0;

        #endregion

        #region 初始化

        public MainWindow()
        {
            InitializeComponent();
            InitializeBalls();
            InitializeSticks();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += MainLoop;
            this.MouseLeftButtonUp += onRelease;
            this.MouseMove += onMove;
        }

        void InitializeBalls()
        {
            // 初始化数组
            balls1 = new Ball[width, height];
            balls2 = new Ball[width, height];
            drawingBalls = new Ellipse[width, height];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    // 新建小球
                    balls1[i, j] = new Ball();
                    Ball b = balls1[i, j];
                    b.position = new Point(i * 30, j * 30);
                    if (i == 0 && j == 0)
                        b.isAnchor = true;

                    // 复制一份
                    balls2[i, j] = balls1[i, j];

                    // 当前的和上一帧的
                    currBalls = balls1;
                    lastBalls = balls2;

                    // 画出小球
                    Ellipse ball = new Ellipse();
                    ball.Height = ball.Width = b.diameter ;
                    if (b.isAnchor)
                        ball.Fill = new SolidColorBrush(Colors.Gray);
                    else
                        ball.Fill = new SolidColorBrush(Colors.White);
                    ball.Stroke = new SolidColorBrush(Colors.Black);
                    ball.StrokeThickness = 2;
                    ball.RenderTransform = new TranslateTransform(b.position.X, b.position.Y);
                    Box.Children.Add(ball);

                    // 为小球添加响应函数
                    ball.MouseLeftButtonDown += onPress;
                    //ball.MouseMove += onMove;

                    // 加入数组
                    drawingBalls[i, j] = ball;
                }
            }
        }

        void InitializeSticks()
        {
            // 根据球数量新建数组
            numOfSticks = height * (width - 1);
            numOfSticks += width * (height - 1);
            sticks = new Stick[numOfSticks];

            // 连接sitck和ball，对于每个ball，连接它们的右边和下边邻居
            int k = 0;
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    // 连接右边
                    if (i < width - 1)
                    {
                        sticks[k] = new Stick(lastBalls[i, j], lastBalls[i+1, j]);
                        lastBalls[i, j].sticks.Add(sticks[k]);
                        lastBalls[i+1, j].sticks.Add(sticks[k]);
                        k++;
                    }
                    // 连接下边
                    if (j < height - 1)
                    {
                        sticks[k] = new Stick(lastBalls[i, j], lastBalls[i, j + 1]);
                        lastBalls[i, j].sticks.Add(sticks[k]);
                        lastBalls[i, j+1].sticks.Add(sticks[k]);
                        k++;
                    }
                }
            }
        }

        #endregion

        #region 循环

        void MainLoop(Object sender, EventArgs e)
        {
            UpdateDrawing();
            UpdatePhysics();
        }

        void UpdateDrawing()
        {
            // 永远使用上一帧的物理计算结果来绘制
            for (int j=0; j<height; j++)
                for (int i = 0; i < width; i++)
                {
                    TranslateTransform tt = drawingBalls[i, j].RenderTransform as TranslateTransform;
                    tt.X = lastBalls[i, j].position.X;
                    tt.Y = lastBalls[i, j].position.Y;
                }
        }

        void UpdatePhysics()
        {
            UpdateSticks();
            UpdateBalls();
        }

        void UpdateBalls()
        {
            foreach (Ball b in currBalls)
            {
                b.UpdatePosition();
            }
            Ball[,] temp = currBalls;
            currBalls = lastBalls;
            lastBalls = temp;
        }

        void UpdateSticks()
        {
            foreach (Stick s in sticks)
            {
                s.UpdateTension();
            }
        }

        #endregion


        #region 小球鼠标响应函数

        Ball dragingBall = null;
        Ellipse dragingElli = null;
        Point lastMousePos;
        void onMove(Object sender, MouseEventArgs e)
        {
            if (dragingBall != null && dragingElli != null)
            {
                Point currMousePos = e.GetPosition(this);
                dragingBall.position += currMousePos - lastMousePos;
                TranslateTransform tt = dragingElli.RenderTransform as TranslateTransform;
                tt.X = dragingBall.position.X;
                tt.Y = dragingBall.position.Y;
                lastMousePos = currMousePos; 
            }
        }

        void onPress(Object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(this);
            Ellipse elli = sender as Ellipse;
            // 寻找被选中的球
            for (int j=0; j<height; j++)
                for (int i = 0; i < width; i++)
                {
                    if (drawingBalls[i, j] == elli && !lastBalls[i, j].isAnchor)
                    {
                        dragingElli = elli;
                        dragingBall = lastBalls[i, j];
                        dragingBall.isDraging = true;
                        return;
                    }
                }
        }

        void onRelease(Object sender, MouseButtonEventArgs e)
        {
            if (dragingBall != null)
                dragingBall.isDraging = false;
            dragingBall = null;
            dragingElli = null;
        }

        #endregion
    }
}
