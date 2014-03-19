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
using System.Windows.Threading;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for VideoPreview.xaml
    /// </summary>
    public partial class VideoPreview : UserControl
    {
        DispatcherTimer timer;
        public enum Mode{
            Watting,
            Request,
            InCall
          
        }
        private int left = 20;
        public Mode _Mode
        {
            set
            {
                HideVisibility();
                SetVisibility(value);
                if (value == Mode.Watting)
                    StartWaiting();
                else
                    timer.Stop();
            }
        }
        public MainWindow window;
        public string Nick
        {
            set
            {
                _Nick.Content = value;
            }
        }
        public System.Net.IPAddress ip;
        public UDP udp;
        public System.Net.IPAddress hostIP;
        public VideoPreview(Mode mode,System.Net.IPAddress hostIP)
        {
            InitializeComponent();
            this.hostIP = hostIP;
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            InitializeComponent();
            SetVisibility(mode);
            _Mode = mode;
        }
        public void SetVisibility(Mode mode)
        {
            HideVisibility();
            if (mode == Mode.Watting)
                time_left.Visibility = Visibility.Visible;
            if (mode == Mode.InCall)
                prev.Visibility = Visibility.Visible;
            if (mode == Mode.Request)
            {
                accept.Visibility = Visibility.Visible;
                decline.Visibility = Visibility.Visible;
            }
        }
        public void StartWaiting()
        {
            timer.Start();
            
            SetVisibility(Mode.Watting);
        }
        private void HideVisibility()
        {
            prev.Visibility = Visibility.Hidden;
            accept.Visibility = Visibility.Hidden;
            decline.Visibility = Visibility.Hidden;
            time_left.Visibility = Visibility.Hidden;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            
            time_left.Content = --left;
            

        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
            udp.SendMessageTo(UDP.RVideocall, hostIP);
            window.videoConf = new VideoConf(udp,hostIP);
            window.videoConf.Show();
            window.videoConf.statusLabel.Content = "Connected to Host ...";
            window.videoConf.AddUser(hostIP);
            window.videoConf.MakeUserPreview(hostIP, VideoPreview.Mode.InCall);
            window.waiting.Close();
        }

        private void decline_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
            udp.SendMessageTo(UDP.RemoveMember, hostIP);
               
        }
    }
}
