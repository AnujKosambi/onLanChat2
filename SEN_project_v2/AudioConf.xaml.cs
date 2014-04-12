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
using NAudio.Wave;
using System.Net;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for AudioConf.xaml
    /// </summary>
    public partial class AudioConf : Window
    {
        public List<IPAddress> Users;
        public Boolean IsHost = false;
        private UDP udp;
        public List<IPAddress> requestedUsers;
        public Dictionary<IPAddress, AudioPreview> vp;
        public IPAddress host;
        public MainWindow mParent;
        public System.Windows.Forms.Timer timer;
        private ScreenCapture sc = new ScreenCapture();
        public RTPClient rtpClient;
        public static Audio audio;
        public static float vol = 1;
        private WaveFileWriter waveWriter;
        BitmapImage bi = new BitmapImage();
        public AudioConf(MainWindow parent, IPAddress host)
        {
            parent.audioConfB.IsEnabled = false;
            InitializeComponent();
            this.Background = MainWindow.brushColor;
       
            this.host = host;
            this.udp = MainWindow.udp;
            mParent = parent;
            Users = new List<IPAddress>();
            vp = new Dictionary<IPAddress, AudioPreview>();
            requestedUsers = UserList.Selected.Where(x => MainWindow.hostIPS.Contains(x) == false).ToList();
            InitializeComponent();
            if (mParent.rtpClient != null)
                mParent.rtpClient.Dispose();

            rtpClient = new RTPClient(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("224.5.6.7"),
                (int)MainWindow.Ports.RTP),vp,string.Join("#",MainWindow.hostIPS.Select(x=>x.ToString()).ToArray()), "224.5.6.7");
            mParent.rtpClient = this.rtpClient;
            rtpClient.window = this;
            audio = new Audio();
           
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((Grid)sender).Width = ((Grid)sender).RenderSize.Height * 1.5;

        }
        public void AddUser(IPAddress ip)
        {
            Users.Add(ip);
            //this.mParent.rtpClient.rBuffer.Add(ip, new System.IO.MemoryStream());

        }
        public void MakeUserPreview(IPAddress ip, AudioPreview.Mode mode)
        {
            vp.Add(ip, new AudioPreview(mode, ip) { Nick = UserList.Get(ip).nick, myip = ip });
            vp[ip].window = mParent;
            vp[ip].udp = udp;
            vp[ip].HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            vp[ip].VerticalAlignment = VerticalAlignment.Stretch;
            vp[ip].hostIP = host;
            vp[ip].SizeChanged += AudioConf_SizeChanged;
            _stack.Children.Add(vp[ip]);
            _stack.SizeChanged += _stack_SizeChanged;
        }
         private void Window_Loaded(object sender, RoutedEventArgs e)
        {
 
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 500;
            //foreach (var souces in audio.sources)
            //    AudioSources.Items.Add(souces.ProductName);
            //AudioSources.SelectedIndex = 0;
        }
         void AudioConf_SizeChanged(object sender, SizeChangedEventArgs e)
         {

             ((AudioPreview)sender).Width = ((AudioPreview)sender).RenderSize.Height * 1.5;
             AudioPreview v = ((AudioPreview)sender);
         }
         void _stack_SizeChanged(object sender, SizeChangedEventArgs e)
         {
             ((StackPanel)_stack).InvalidateVisual();
             foreach (AudioPreview s in _stack.Children)
             {
                 s.Height = _stack.Height;
                 s.Width = s.RenderSize.Height * 1.5;

             }
         }
        private void timer_Tick(object sender, EventArgs e)
         {
             System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
             if (audio != null && audio.listBytes.Count > 0)
             {

                 int oldsize = audio.listBytes.Count;
                 memoryStream.Write(audio.listBytes.ToArray(), 0, oldsize);
                audio.listBytes = audio.listBytes.Skip(oldsize).ToList();
                rtpClient.rtpSender.Send(memoryStream.GetBuffer());
             }
           
         }
         public void Start() //IF Host
         {
             foreach (IPAddress ip in UserList.Selected)
             {
                 if (!MainWindow.hostIPS.Contains(ip))
                     udp.SendMessageTo(UDP.Audiocall, ip);

             }


         }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (b_start.Content.ToString().Equals("Stop"))
            {
                b_start.Content = "Start";
                
                audio.sourceStream.StopRecording();

                foreach(var  ip in vp.Values)
                {
                    if(ip.canRecord ==true)
                    {
                        ip.Record_Click(null, null);
                    }
                }
                //writer.Close();
                //waveWriter.Close();
            }
            else
            {
            
          
                System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                document.Load(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml");
                System.Xml.XmlNode Microphone = document.SelectSingleNode("UserProfile/Conference/Microphone");

                int index = 0;
                List<string> productName = audio.sources.Select(x => x.ProductName).ToList();

                index = productName.FindIndex(x => x == Microphone.InnerText);
                audio.init(index);
                //   waveWriter = new WaveFileWriter("test.wav",audio.sourceStream.WaveFormat);
                timer.Start();
            
                audio.sourceStream.StartRecording();
                b_start.Content = "Stop";
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mParent.audioConfB.IsEnabled = true;
            if (IsHost == true)
            {
                foreach (IPAddress ip in requestedUsers)
                {
                    udp.SendMessageTo(UDP.ExitCallA, ip);
                }
            }
            if (audio.sourceStream != null)
                audio.sourceStream.StopRecording();
            
            rtpClient.Dispose();

            _stack.Children.RemoveRange(0, _stack.Children.Count);
            mParent.audioConf = null;
            
        }
      
  public class Audio
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
                {   sourceStream = new WaveIn();
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

  private void AddMember_Click(object sender, RoutedEventArgs e)
  {
      AddMembers adm = new AddMembers(this);
      adm.Show();
  }

    
    }
}
