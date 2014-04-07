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
        public Boolean isRemote=false;
        public System.Net.IPAddress myip;
        public UDP udp=MainWindow.udp;
        public System.Net.IPAddress hostIP;
        private int left = 20;
        DispatcherTimer timer;     
        public MainWindow window;
        public VideoPreview()
        {
            InitializeComponent();
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            InitializeComponent();
        
        }

        public enum Mode{
            Watting,
            Request,
            InCall
          
        }
        
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
 
        public string Nick
        {
            set
            {
                _Nick.Content = value;
            }
            get
            {
                return _Nick.Content.ToString();
            }
        }

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
        public VideoPreview(Mode mode, System.Net.IPAddress hostIP,Boolean isRemote)
        {
            this.isRemote = isRemote;
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
            {
                time_left.Visibility = Visibility.Visible;
            }
            else  if (mode == Mode.InCall)
                prev.Visibility = Visibility.Visible;
            else if (mode == Mode.Request)
            {
                accept.Visibility = Visibility.Visible;
                decline.Visibility = Visibility.Visible;
                prev.Visibility = Visibility.Visible; 
              
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
            if(left==0)
            {
                udp.SendMessageTo(UDP.ExitCall,myip);
                
                window.videoConf.Dispatcher.Invoke((Action)(() =>
                   {
                       window.videoConf._stack.Children.Remove(window.videoConf.vp[myip]);
                   }));
            }

        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
            if (!isRemote)
            {
                udp.SendMessageTo(UDP.RVideocall+hostIP, hostIP);
                VideoConf videoConf = new VideoConf(window, hostIP);
                window.videoConf = videoConf;
                if (videoConf.SetVideoSources())
                {
                    videoConf.Show();
                    videoConf.statusLabel.Content = "Connected to Host ...";
                    videoConf.AddUser(hostIP);
                    videoConf.MakeUserPreview(hostIP, VideoPreview.Mode.InCall);
                        
                }
                else
                {
                    _Mode = Mode.InCall;
                    udp.SendMessageTo(UDP.RemoveMember, hostIP);
                }
                window.Dispatcher.Invoke((Action)(() =>
                {

                    window.waiting.Close();
                }));
            }else
            {
                udp.SendMessageTo(UDP.RRemote, hostIP);
                window.Dispatcher.Invoke((Action)(() =>
                {
                    window.remote = new Remote(window, hostIP);
                    window.remote.StartSending();
                    window.remoteWin.Close();
                    window.Remote.Content = "Stop Remote";
                }));
            }

        }

        private void decline_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
            if (isRemote)
            {
                window.remoteWin.Close();
            }
            else
            {
                udp.SendMessageTo(UDP.RemoveMember, hostIP);
                window.waiting.Close();
            }
        }
    }
}
