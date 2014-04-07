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

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        Boolean start;
        Window window;
        Point StartPosition;
        Point StartLocation;
        public TitleBar()
        {
            InitializeComponent();
        }
        public void SetWindow(Window window){
            this.window = window;

        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            start = true;
            StartLocation.X = window.Left;
            StartLocation.Y = window.Top;
            StartPosition.X= System.Windows.Forms.Control.MousePosition.X;
            StartPosition.Y = System.Windows.Forms.Control.MousePosition.Y;
            this.MouseMove += Label_MouseMove;

        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            start = false;
            StartPosition.X = 0;
            StartPosition.Y = 0;
            this.MouseMove -= Label_MouseMove;
        }

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (start)
            {
                window.Left = StartLocation.X + (System.Windows.Forms.Control.MousePosition.X - StartPosition.X);
                window.Top = StartLocation.Y + (System.Windows.Forms.Control.MousePosition.Y - StartPosition.Y);
            }
        
        }
    }
}
