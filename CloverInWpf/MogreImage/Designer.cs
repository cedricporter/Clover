using System.ComponentModel;
using System.Windows;

namespace MogreInWpf
{
    // Retrieved from (Alex Sarafian) http://sarafianalex.wordpress.com/2008/06/14/in-design-mode/
    
    public static class Designer
    {
        private static readonly DependencyObject _dummy = new DependencyObject();

        public static bool InDesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(_dummy); }
        }
    }
}
