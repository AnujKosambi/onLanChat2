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
using System.Net;
using System.Runtime.InteropServices;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Remote.xaml
    /// </summary>
    public partial class Remote : Window
    {
        public RTPClient rtpClient;
        public System.Windows.Forms.Timer timer;
        public System.Windows.Forms.Timer mouseTimer;
        private ScreenCapture sc;
        public  User32.POINT mousePos;
        private UDP udp = MainWindow.udp;
        private IPAddress  remoteIP;
        public Remote(Window parent,IPAddress ip)
        {
            InitializeComponent();
            Dictionary<IPAddress,VideoPreview> vplist=new Dictionary<IPAddress,VideoPreview>();
                      
            remoteIP = ip;
          
            rtpClient = new RTPClient(new IPEndPoint(ip, (int)MainWindow.Ports.RTP), Screen, MainWindow.hostIP.ToString(), ip.ToString());
            rtpClient.window = this;
            sc = new ScreenCapture();
            timer = new System.Windows.Forms.Timer();
            mouseTimer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            mouseTimer.Interval = 10;
            mouseTimer.Tick += mouseTimer_Tick;

        }

        void mouseTimer_Tick(object sender, EventArgs e)
        {
            User32.SetCursorPos(mousePos.X, mousePos.Y);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            rtpClient.rtpSender.Send(sc.GetDesktopBitmapBytes());

            
        }
        public void Start()
        {
            udp.SendMessageTo(UDP.Remote, remoteIP);
        }
        public void StartSending()
        {
            timer.Start();
            mouseTimer.Start();
        }
        public void StopSending()
        {
            timer.Stop();
            if (mouseTimer != null)
                mouseTimer.Stop();
            rtpClient.Dispose();
        }
        public class User32
        {

            [DllImport("user32.dll")]   
            public static extern bool GetCursorPos(out POINT lpPoint);
            [StructLayout(LayoutKind.Sequential)]
            public  struct POINT
            {
                public  int X;
                public  int Y;

                public static implicit operator Point(POINT point)
                {
                    return new Point(point.X, point.Y);
                }
            }
            [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int X, int Y);
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            User32.GetCursorPos(out mousePos);
            udp.SendMessageTo(UDP.Mouse + mousePos.X + UDP.Breaker + mousePos.Y,remoteIP);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mouseTimer != null)
                mouseTimer.Stop();
            if (timer != null)
                timer.Stop();
            if (rtpClient != null)
                rtpClient.Dispose();
      
            
        }

    

    }
}
