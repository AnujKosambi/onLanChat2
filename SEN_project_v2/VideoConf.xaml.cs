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
using System.Threading;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Runtime.InteropServices;
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
        public System.Windows.Forms.Timer timer;
        private ScreenCapture sc = new ScreenCapture();
        public RTPClient rtpClient;
        /// <summary>
        /// Video Devices
        /// </summary>
        private VideoCaptureDevice videoDevice;
        private FilterInfoCollection infos;

        BitmapImage bi = new BitmapImage();
        public VideoConf(MainWindow parent, IPAddress host)
        {
            this.host = host;
            this.udp = MainWindow.udp;
            mParent = parent;
            Users = new List<IPAddress>();
            vp = new Dictionary<IPAddress, VideoPreview>();

            requestedUsers = UserList.Selected.Where(x => MainWindow.hostIPS.Contains(x) == false).ToList();
            InitializeComponent();
            if (mParent.rtpClient != null)
                mParent.rtpClient.Dispose();
            rtpClient = new RTPClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("224.0.0.2"), 
                (int)MainWindow.Ports.RTP),vp, MainWindow.hostIP.ToString(), "224.0.0.2");
            mParent.rtpClient = this.rtpClient;
            rtpClient.window = this;
            videoDevice = new VideoCaptureDevice();

        }
     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 500;
           // timer.Start();
        
        }
        public bool SetVideoSources()
        {
            infos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo info in infos)
            {
                VideoSources.Items.Add(info.Name);

            }
            if (VideoSources.Items.Count > 0)
            {
                VideoSources.SelectedIndex = 0;
                return true;
            }
            else
            {
                System.Windows.MessageBox.Show("You can not initiate Video Call/Conference due to :" + "-- No Resoure Available");
                return false;
            }

        }
        private void timer_Tick(object sender, EventArgs e)
        {
          
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {    
           System.Windows.Controls.Button b_start=((System.Windows.Controls.Button)sender);
            if (videoDevice.IsRunning == true)
            {
                b_start.Content = "Start";
                videoDevice.Stop();
              
            }
            else
            {

                videoDevice = new VideoCaptureDevice(infos[VideoSources.SelectedIndex].MonikerString) { DesiredFrameRate = 20 };
                

                 videoDevice.NewFrame += capture_NewFrame;
                
                videoDevice.Start();
                b_start.Content = "Stop";
            }
        }



        void capture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            System.IO.MemoryStream ms=new System.IO.MemoryStream();
            
            eventArgs.Frame.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            Byte[] b = ms.GetBuffer();
            rtpClient.rtpSender.Send(b);

            System.IO.MemoryStream ms2 = new System.IO.MemoryStream();
            eventArgs.Frame.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms2.Seek(0, System.IO.SeekOrigin.Begin);
       
            bi.Dispatcher.BeginInvoke((Action)(() =>
            {
                bi = new BitmapImage();
                bi.BeginInit();   
                bi.StreamSource = ms2;
                bi.EndInit();
                server_img.Dispatcher.BeginInvoke((Action)(() =>
           {
               server_img.Source = bi;
           }));
           }));
            bi = null;
            
            
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            videoDevice.Stop();
            rtpClient.Dispose();
        }


    }

}
