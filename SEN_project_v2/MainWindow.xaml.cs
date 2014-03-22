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
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
//using WPF.Themes;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static UDP udp;
        Threads threads;
        Dictionary<IPAddress, int> indexer;
        public RTPClient rtpClient;
        public VideoConf videoConf;
        public static List<IPAddress> hostIPS;
        public static IPAddress hostIP;
        public MainWindow()
        {
            InitializeComponent();
            
        
            udp = new UDP((int)Ports.UDP);
            udp.SetWindow(this);
            
            threads = new Threads(this);
            indexer = new Dictionary<IPAddress, int>();
            #region hostIP init
            hostIPS = new List<IPAddress>();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
               
                foreach (var x in ni.GetIPProperties().UnicastAddresses)
                {

                    System.Diagnostics.Debug.WriteLine(ni.NetworkInterfaceType + "" + x.Address);
                    if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        hostIPS.Add(x.Address);

                }
            }
            #endregion
        }
        public void AddToUserList(UserView uv)
        {
            indexer.Add(uv.u_ip, _listView.Items.Count);
            _listView.Items.Insert(indexer[uv.u_ip], uv);

        }

        public void RemoverFromUserList(IPAddress ip)
        {
            try
            {
                _listView.Items.RemoveAt(indexer[ip]);
                indexer.Remove(ip);
            }
            catch (Exception e)
            {
            };


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            threads.StartAll();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class Threads
        {
            public static Thread broadcast;
            public static Thread udpReceving;
            public static Thread tcpReceving;
            public static Thread fileSending;
            public static Thread fileReceving;
            public static Thread rtpReceving;
            public static MainWindow w;
            public Threads(MainWindow window)
            {
                w = window;
                broadcast = new Thread(new ThreadStart(broadcast_proc));
                udpReceving = new Thread(new ThreadStart(udp.recevingThread));
                udpReceving.SetApartmentState(ApartmentState.STA);
    //            rtpReceving = window.rtpClient.listen_thread;
                udpReceving.SetApartmentState(ApartmentState.STA);
            }

            private void broadcast_proc()
            {
                while (true)
                {
                    udp.SendMessageTo(UDP.Connect + Environment.MachineName, BroadCasting.SEND.Address);
                    Thread.Sleep(5000);
                  
                }
            }
            public void StartAll()
            {
                udpReceving.Start();
                broadcast.Start();

            }
            public void StopAll()
            {
                StopThread(broadcast);
                udp.SendMessageTo(UDP.Disconnect, BroadCasting.SEND.Address);
                if(w.rtpClient!=null)
                w.rtpClient.Dispose();
                StopThread(udpReceving);
                StopThread(tcpReceving);
                StopThread(fileSending);
                StopThread(fileReceving);
                StopThread(rtpReceving);
            }

            public void StopThread(Thread thread)
            {
                if (thread != null && thread.IsAlive)
                    thread.Abort();
            }
        }
        public struct BroadCasting
        {
            public static IPEndPoint SEND = new IPEndPoint(IPAddress.Parse("255.255.255.255"), (int)Ports.UDP);
            public static IPEndPoint RECEIVE = new IPEndPoint(IPAddress.Any, (int)Ports.UDP);

        }
        public enum Ports : int
        {
            UDP = 1716,
            TCP = 12316,
            RTP = 56789,

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            threads.StopAll();

            udp.recevingClient.Close();
        }

        private void VideoConfB_Click(object sender, RoutedEventArgs e)
        {

            videoConf = new VideoConf(this,hostIP);
            if (videoConf.SetVideoSources())
            {
                videoConf.Show();//  CreateVideoConf(null);
                videoConf.statusLabel.Content = "Waiting For Users's Responses...";
                foreach (IPAddress ip in videoConf.requestedUsers)
                {
                    //    videoConf.vp.Add(ip, new VideoPreview(VideoPreview.Mode.Watting, null) { Nick = UserList.Get(ip).nick });
                    //  videoConf._stack.Children.Add(videoConf.vp[ip]);
                    videoConf.MakeUserPreview(ip, VideoPreview.Mode.Watting);

                }
                videoConf.Start();
            }
        }
        public void CreateVideoConf(IPAddress host)
        {
        //    videoConf = new VideoConf(udp, host);

//            videoConf.Show();

            waiting = new Window();
            waiting.BorderThickness = new Thickness(0, 0, 0, 0);
            waiting.AllowsTransparency = true;
            waiting.Topmost = true;
            waiting.HorizontalAlignment = HorizontalAlignment.Center;
            waiting.VerticalAlignment = VerticalAlignment.Center;
  
            VideoPreview vp = new VideoPreview(VideoPreview.Mode.Request,host);
            vp.Nick = UserList.Get(host).nick;
            vp.window = this;
            waiting.Content = vp;
            waiting.SizeToContent =SizeToContent.WidthAndHeight;
            waiting.WindowStyle = WindowStyle.None;
            vp.udp = udp;
            waiting.Show();

        }
        public Window waiting;



    }
}