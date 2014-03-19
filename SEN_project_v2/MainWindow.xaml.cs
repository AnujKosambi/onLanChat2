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
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static UDP udp;
        public UDP rtpUDP;
        Threads threads;
        Dictionary<IPAddress, int> indexer;
        private RTPClient rtpClient;
        public VideoConf videoConf;
        public static List<IPAddress> hostIPS;
        public MainWindow()
        {
            InitializeComponent();
            udp = new UDP((int)Ports.UDP);
            udp.SetWindow(this);
            threads = new Threads();
            indexer = new Dictionary<IPAddress, int>();
            #region hostIP init
            hostIPS = new List<IPAddress>();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var x in ni.GetIPProperties().UnicastAddresses)
                {
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

        class Threads
        {
            public static Thread broadcast;
            public static Thread udpReceving;
            public static Thread tcpReceving;
            public static Thread fileSending;
            public static Thread fileReceving;
            public static Thread rtpReceving;
            public Threads()
            {
                broadcast = new Thread(new ThreadStart(broadcast_proc));
                udpReceving = new Thread(new ThreadStart(udp.recevingThread));
                udpReceving.SetApartmentState(ApartmentState.STA);
                rtpReceving=new Thread(new ThreadStart(udp.RTPPacket_thread));
                udpReceving.SetApartmentState(ApartmentState.STA);
            }

            private void broadcast_proc()
            {
                while (true)
                {
                    udp.SendMessageTo(UDP.Connect + Environment.MachineName, BroadCasting.SEND.Address);
                    Thread.Sleep(5000);
                    String list = string.Join(" ", UserList.Selected.Select(x => x.ToString()).ToArray());
                    System.Diagnostics.Debug.WriteLine(list);
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

                StopThread(udpReceving);
                StopThread(tcpReceving);
                StopThread(fileSending);
                StopThread(fileReceving);
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
            RTP = 5555,

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            threads.StopAll();

            udp.recevingClient.Close();
        }

        private void VideoConfB_Click(object sender, RoutedEventArgs e)
        {

            videoConf = new VideoConf(udp, null);

            videoConf.Show();//  CreateVideoConf(null);
            videoConf.statusLabel.Content = "Waiting For Users's Responses...";
            foreach (IPAddress ip in videoConf.requestedUsers)
            {
                videoConf.vp.Add(ip, new VideoPreview(VideoPreview.Mode.Watting, null) { Nick = UserList.Get(ip).nick });
                videoConf._stack.Children.Add(videoConf.vp[ip]);
            }
            videoConf.Start();
        }
        public void CreateVideoConf(IPAddress host)
        {
        //    videoConf = new VideoConf(udp, host);

//            videoConf.Show();

            waiting = new Window();
            VideoPreview vp = new VideoPreview(VideoPreview.Mode.Request,host);
            vp.Nick = UserList.Get(host).nick;
            vp.window = this;
            waiting.Content = vp;
            waiting.SizeToContent =SizeToContent.WidthAndHeight;
            waiting.WindowStyle = WindowStyle.ToolWindow;
            vp.udp = udp;
            waiting.Show();

        }
        public Window waiting;



    }
}