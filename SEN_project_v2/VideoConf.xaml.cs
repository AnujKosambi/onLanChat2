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
using NAudio.Wave;
using System.Runtime.InteropServices;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for VideoConf.xaml
    /// </summary>
    public partial class VideoConf : Window
    {
        public List<IPAddress> Users;
        public Boolean IsHost = false;
        private UDP udp;
        public List<IPAddress> requestedUsers;
        public Dictionary<IPAddress,VideoPreview> vp;
        public IPAddress host;
        public MainWindow mParent;
        public System.Windows.Forms.Timer timer;
        private ScreenCapture sc = new ScreenCapture();
        public RTPClient rtpClient;
        private Audio audio;
        public static float vol=0;
        /// <summary>
        /// Video Devices
        /// </summary>
        private VideoCaptureDevice videoDevice;
        private FilterInfoCollection infos;
        private WaveFileWriter waveWriter;
        BitmapImage bi = new BitmapImage();
        public VideoConf(MainWindow parent, IPAddress host)
        {
            parent.VideoConfB.IsEnabled = false;
            InitializeComponent();
            this.Background = MainWindow.brushColor;
            this.host = host;
            this.udp = MainWindow.udp;
            mParent = parent;
            Users = new List<IPAddress>();
            vp = new Dictionary<IPAddress, VideoPreview>();
            requestedUsers = UserList.Selected.Where(x => MainWindow.hostIPS.Contains(x) == false).ToList();
      
            if (mParent.rtpClient != null)
                mParent.rtpClient.Dispose();
            
            rtpClient = new RTPClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("224.0.0.2"), 
                (int)MainWindow.Ports.RTP),vp, MainWindow.hostIP.ToString(), "224.0.0.2");
            mParent.rtpClient = this.rtpClient;
            rtpClient.window = this;
            videoDevice = new VideoCaptureDevice();
            audio = new Audio();


            
        }
     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         /*   for (int i = 0; i < audio.sources.Count;i++ )
                AudioSources.Items.Add(audio.sources.ElementAt(i).ProductName);
            if (audio.sources.Count > 0)
            {
                AudioSources.SelectedIndex = 0;
            }*/
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 500;
           // timer.Start();
        
        }
        public bool SetVideoSources()
        {
            infos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            
            if (infos.Count > 0)
            {
               // VideoSources.SelectedIndex = 0;
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
            vp.Add(ip, new VideoPreview(mode, ip) { Nick = UserList.Get(ip).nick ,myip=ip});
            vp[ip].window = mParent;
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
                audio.sourceStream.StopRecording();
               //writer.Close();
               waveWriter.Close();
            }
            else
            {

                videoDevice = new VideoCaptureDevice(infos[VideoSources.SelectedIndex].MonikerString) { DesiredFrameRate = 20 };
                //writer.Open("test.avi", 640,480);
                //writer.FrameRate =20;
                //writer.Quality = 0;
                System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                document.Load(AppDomain.CurrentDomain.BaseDirectory+"\\UserSettings.xml");
                System.Xml.XmlNode camera = document.SelectSingleNode("UserProfile/Conference/Camera");
                System.Xml.XmlNode Microphone = document.SelectSingleNode("UserProfile/Conference/Microphone");

                List<string> productName= audio.sources.Select(x => x.ProductName).ToList();
             //   AudioSources.SelectedIndex = productName.FindIndex(x => x == Microphone.InnerText);
                audio.init(productName.FindIndex(x => x == Microphone.InnerText));
             //   waveWriter = new WaveFileWriter("test.wav",audio.sourceStream.WaveFormat);
                
                 videoDevice.NewFrame += capture_NewFrame;
                 audio.sourceStream.StartRecording();
                videoDevice.Start();
                b_start.Content = "Stop";
            }
        }



        void capture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            System.IO.MemoryStream ms=new System.IO.MemoryStream();
           // for (int i = 0; i < 10;i++ )
           //     writer.AddFrame(eventArgs.Frame);
            eventArgs.Frame.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            Byte[] b = ms.GetBuffer();
                       
            System.IO.MemoryStream withSize = new System.IO.MemoryStream();
            Byte[] length = new Byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (i < BitConverter.GetBytes(ms.GetBuffer().Length).Length)
                    length[i] = BitConverter.GetBytes(ms.GetBuffer().Length)[i];
            }
            withSize.Write(length, 0, 4);
            withSize.Write(ms.GetBuffer(), 0, ms.GetBuffer().Length);
            if (audio != null && audio.listBytes.Count > 0)
            {

                int oldsize = audio.listBytes.Count;
                withSize.Write(audio.listBytes.ToArray(), 0, oldsize);
            //    waveWriter.WriteData(audio.listBytes.ToArray(), 0, audio.listBytes.Count);
                
                audio.listBytes = audio.listBytes.Skip(oldsize).ToList();

            }
            rtpClient.rtpSender.Send(withSize.GetBuffer());

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
  
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
          if(IsHost==true)
          {
              foreach(IPAddress ip in requestedUsers)
              {
                  udp.SendMessageTo(UDP.ExitCall, ip);
              }
          }
            if (audio.sourceStream != null)
                audio.sourceStream.StopRecording();
            videoDevice.Stop();
            rtpClient.Dispose();

            _stack.Children.RemoveRange(0, _stack.Children.Count);
            mParent.videoConf = null;
            mParent.VideoConfB.IsEnabled = true;
        }

        

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            vol = 0;
            Volumn.Value = 0;
        }

        private void Volumn_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vol = (float)Volumn.Value;
        }


    }
    class Audio
    {
        public List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
      //  public static WaveInCapabilities _default;
        public WaveIn sourceStream;
        public List<Byte> listBytes = new List<Byte>();
      //  public WinSound.Recorder recorder;
        public Audio()
        {
             for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                sources.Add(WaveIn.GetCapabilities(i));
                System.Diagnostics.Debug.WriteLine(WaveIn.GetCapabilities(i).ProductName);
            }
            
            
        }
        public void init(int deviceNumber)
        {
            if (sources.Count >= 0)
            {

            
                sourceStream = new WaveIn();
                
                sourceStream.DeviceNumber = deviceNumber;
                sourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);
                sourceStream.RecordingStopped += sourceStream_RecordingStopped;
                sourceStream.DataAvailable += sourceStream_DataAvailable;
                
            }
        }
        void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            listBytes = listBytes.Concat(e.Buffer).ToList();
        }

        void sourceStream_RecordingStopped(object sender, EventArgs e)
        {

            listBytes.Clear();
        }
    }

    }
