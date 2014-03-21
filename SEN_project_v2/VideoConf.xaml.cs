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
//using System.Net.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Windows.Forms;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for VideoConf.xaml
    /// </summary>
    public partial class VideoConf : Window
    {
        public List<IPAddress> Users;
        private UDP udp;
        public List<IPAddress> requestedUsers;
        public Dictionary<IPAddress,VideoPreview> vp;
        public IPAddress host;
        public MainWindow mParent;
        public Timer timer;
        private ScreenCapture sc = new ScreenCapture();
        public RTPClient rtpClient;
        public VideoConf(MainWindow parent,IPAddress host)
        {
            this.host = host;
            this.udp = MainWindow.udp;
            mParent = parent;
            Users = new List<IPAddress>();
            vp = new Dictionary<IPAddress, VideoPreview>();
          
             mParent.rtpClient = this.rtpClient;
            requestedUsers = UserList.Selected.Where(x => MainWindow.hostIPS.Contains(x) == false).ToList();
            InitializeComponent();
            rtpClient = new RTPClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("224.0.0.2"), (int)MainWindow.Ports.RTP), vp, MainWindow.hostIP.ToString(), "224.0.0.2");
            rtpClient.vcWind = this;
            timer = new Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 500;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Byte[] b=sc.GetDesktopBitmapBytes();
            rtpClient.rtpSender.Send(b);
            server_img.Source=rtpClient.GetImage(b).Source;
        }

        public void AddUser(IPAddress ip)
        {
            Users.Add(ip);
            //this.mParent.rtpClient.rBuffer.Add(ip, new System.IO.MemoryStream());
            
        }
        public void MakeUserPreview(IPAddress ip,VideoPreview.Mode mode)
        {
            vp.Add(ip, new VideoPreview(mode, ip) { Nick = UserList.Get(ip).nick });
            vp[ip].udp = udp;
            vp[ip].HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            vp[ip].VerticalAlignment = VerticalAlignment.Stretch;
            vp[ip].hostIP = host;
            vp[ip].SizeChanged += VideoConf_SizeChanged;
            _stack.Children.Add(vp[ip]);
            _stack.SizeChanged += _stack_SizeChanged;
        }

        void _stack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((StackPanel)_stack).InvalidateVisual();
            foreach (VideoPreview s in _stack.Children)
            {
                s.Height = _stack.Height;
                s.Width = s.RenderSize.Height * 1.5;
                
            }
        }

        void VideoConf_SizeChanged(object sender, SizeChangedEventArgs e)
        {
    
                ((VideoPreview)sender).Width = ((VideoPreview)sender).RenderSize.Height *1.5 ;
                VideoPreview v = ((VideoPreview)sender);
              
     
        }
/// <summary>
/// only for host
/// </summary>

        public void Start() //IF Host
        {
            foreach (IPAddress ip in UserList.Selected)
            {
                if (!MainWindow.hostIPS.Contains(ip))
                    udp.SendMessageTo(UDP.Videocall, ip);

            }
               

        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((Grid)sender).Width = ((Grid)sender).RenderSize.Height * 1.5;
           
        }

   
        


    }
}
