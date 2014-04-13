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
    public partial class AudioPreview : UserControl
    {
        public Boolean isRemote=false;
        
        public System.Net.IPAddress myip;
        public UDP udp=MainWindow.udp;
        public System.Net.IPAddress hostIP;
        private int left = 20;
        DispatcherTimer timer;     
        public MainWindow window;
        public bool canRecord=false;
        public AudioPreview()
        {
            InitializeComponent();
            
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
           
        
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

        public AudioPreview(Mode mode,System.Net.IPAddress hostIP)
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
        public AudioPreview(Mode mode, System.Net.IPAddress hostIP, Boolean isRemote)
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
            else if (mode == Mode.InCall)
            {
                prev.Visibility = Visibility.Visible;
                Record.Visibility = Visibility.Visible;
            }
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
            Record.Visibility = Visibility.Hidden;
        }
        void timer_Tick(object sender, EventArgs e)
        {
            
            time_left.Content = --left;
            if(left==0)
            {
                udp.SendMessageTo(UDP.ExitCallA,myip);
                if (window.audioConf != null) 
                window.audioConf.Dispatcher.Invoke((Action)(() =>
                   {
                       window.audioConf._stack.Children.Remove(window.audioConf.vp[myip]);
                   }));
            }

        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
          
                udp.SendMessageTo(UDP.RAudiocall+ hostIP, hostIP);
                AudioConf audioConf = new AudioConf(window, hostIP);
                window.audioConf = audioConf;
                if (AudioConf.audio.sources.Count>0)
                {
                    audioConf.Show();
                    audioConf.statusLabel.Content = "Connected to Host ...";
                    audioConf.AddUser(hostIP);
                    audioConf.MakeUserPreview(hostIP, AudioPreview.Mode.InCall);
                        
                }
                else
                {
                    _Mode = Mode.InCall;
                    udp.SendMessageTo(UDP.RemoveMemberA, hostIP);
                }
                window.Dispatcher.Invoke((Action)(() =>
                {

                    window.waiting.Close();
                }));
            

        }

        private void decline_Click(object sender, RoutedEventArgs e)
        {
            _Mode = Mode.InCall;
         
            
                udp.SendMessageTo(UDP.RemoveMemberA, hostIP);
                window.waiting.Close();
            
        }

        public void Record_Click(object sender, RoutedEventArgs e)
        {
            this.canRecord = !canRecord;
            if(this.canRecord==false)
            {
                Record.Content = "Record";
                
            }
            else if(this.canRecord==true)
            {
                Record.Content = "Stop";
                
            }
            
        }
    }
}
