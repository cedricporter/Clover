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
using Clover;

namespace _3D
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        CloverController controller = new CloverController();


        public MainWindow()
        {
            InitializeComponent();

            controller.Initialize(100, 100);

            try
            {
                foldingPaperViewport.Children.Add(controller.UpdatePaper());
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());	
            }
        }
    }
}
