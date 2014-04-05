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
using System.Windows.Shapes;

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Snipping.xaml
    /// </summary>
    public partial class Snipping : Window
    {
        private bool Start = false;
        private Point Starting;
        ScreenCapture sc;
        public Snipping()
        {
            sc = new ScreenCapture();
            InitializeComponent();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            Start = true;
        
            rect.Width = 0;
            rect.Height = 0;
            Starting.X = System.Windows.Forms.Control.MousePosition.X;
            Starting.Y = System.Windows.Forms.Control.MousePosition.Y;
            rect.Visibility = Visibility.Visible;

        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
           
            Start = false;
            Brush backup = this.Background;
            this.Background = System.Windows.Media.Brushes.Transparent;
            BitmapSource bs = CreateBitmapSourceFromBitmap(sc.GetRectBitmapBytes((int)rect.Margin.Left, (int)rect.Margin.Top, (int)rect.Width, (int)rect.Height));
            MessageBoxResult result= MessageBox.Show("", "Confirm Area", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Clipboard.SetImage(bs);
                this.Close();
            }
            else
            {
                rect.Width = 0;
                rect.Height = 0;
                rect.Visibility = Visibility.Hidden;
                this.Background = backup;
            }
   

        }
        
    private static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException("bitmap");

        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Start)
            {
                SetRect(Starting,new Point(System.Windows.Forms.Control.MousePosition.X,System.Windows.Forms.Control.MousePosition.Y));
            }
        }

        private void SetRect(Point start, Point Current)
        {
            rect.Width = Math.Abs(start.X - Current.X);
            rect.Height = Math.Abs(start.Y - Current.Y);
            double top = Math.Min(start.Y, Current.Y);
            double left =Math.Min(start.X, Current.X);
            rect.Margin = new Thickness(left, top, 0, 0);
        }
    }

   
}
