﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using MSR.LST.Net.Rtp;
using MSR.LST;
using System.Windows;
using NAudio.Wave;
namespace SEN_project_v2
{
    public class RTPClient
    {
        private MemoryStream stream;
        private UdpClient rtpReceClient;
        private UdpClient rtpSendClient;
        private IPAddress client_ip;
        public Thread listen_thread;
        public Window window;
        private bool listening=false;
        public Dictionary<IPAddress, System.IO.MemoryStream> sBuffer;
        public Dictionary<IPAddress, System.IO.MemoryStream> rBuffer;
        public Dictionary<IPAddress, VideoPreview> vpList;
        public Dictionary<IPAddress, AudioPreview> apList;
        private Dictionary<IPAddress, WaveFileWriter> waveWriters;
        private Image image;
        private int port;
        ///
        /// 
        /// 
        public DirectSoundOut waveOut;
        public BufferedWaveProvider waveProvider;
        /// <summary>
        /// 
        /// </summary>
        public RtpSession rtpSession;
        public RtpSender rtpSender;
        public IPEndPoint ipe;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiCastIP"></param>
        /// <param name="vpList"></param>
        /// <param name="cname"></param>
        /// <param name="name"></param>
        public RTPClient(IPEndPoint multiCastIP,Dictionary<IPAddress, VideoPreview> vpList,String cname,String name)

        { 
            UnhandledExceptionHandler.Register();
            this.ipe = multiCastIP;
            this.vpList = vpList;
            this.image = null;
            rtpSession = new RtpSession(ipe, new RtpParticipant(cname,name ), true, true);
            
            System.Diagnostics.Debug.WriteLine(rtpSession.MulticastInterface.ToString());
            rtpSender = rtpSession.CreateRtpSenderFec(name, PayloadType.JPEG, null, 0, 1);

            EvetnBinding();
            //stream = new MemoryStream();
            //sBuffer = new Dictionary<IPAddress, System.IO.MemoryStream>();
            //rBuffer = new Dictionary<IPAddress, System.IO.MemoryStream>();
            //listen_thread = new Thread(new ThreadStart(listener_proc));
            //listen_thread.SetApartmentState(ApartmentState.STA);
            //port =(int)MainWindow.Ports.RTP;
            //rtpSendClient = new UdpClient();
            //rtpReceClient = new UdpClient((int)MainWindow.Ports.RTP);
        }
        public RTPClient(IPEndPoint multiCastIP,Dictionary<IPAddress, AudioPreview> apList, String cname, String name)
        {
            UnhandledExceptionHandler.Register();
            this.ipe = multiCastIP;
            this.image = null;
            this.vpList = null;
            this.apList = apList;
            rtpSession = new RtpSession(ipe, new RtpParticipant(cname, name), true, true);

            System.Diagnostics.Debug.WriteLine(rtpSession.MulticastInterface.ToString());
            rtpSender = rtpSession.CreateRtpSenderFec(name, PayloadType.Chat, null, 0, 1);
            waveWriters = new Dictionary<IPAddress, WaveFileWriter>();
            EvetnBinding();
         
        }
        public RTPClient(IPEndPoint multiCastIP, Image image, String cname, String name)
        {
            UnhandledExceptionHandler.Register();
            this.ipe = multiCastIP;
            this.image = image;
            this.vpList = null;
            rtpSession = new RtpSession(ipe, new RtpParticipant(cname, name), true, true);

            System.Diagnostics.Debug.WriteLine(rtpSession.MulticastInterface.ToString());
            rtpSender = rtpSession.CreateRtpSenderFec(name, PayloadType.JPEG, null, 0, 1);
            waveWriters=new Dictionary<IPAddress,WaveFileWriter>();
             
            EvetnBinding();
          }
        private void EvetnBinding()
        {
            RtpEvents.RtpParticipantAdded += RtpParticipantAdded;
            RtpEvents.RtpParticipantRemoved += RtpParticipantRemoved;
            RtpEvents.RtpStreamAdded += RtpEvents_RtpStreamAdded;
            RtpEvents.RtpStreamRemoved += RtpEvents_RtpStreamRemoved;
            AudioLoader();
            
        }
        private void AudioLoader()
        {
            waveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            waveProvider.DiscardOnBufferOverflow = true;
            waveOut = new DirectSoundOut();
            waveOut.Init(waveProvider);
            waveOut.Volume = VideoConf.vol;
        }
        void RtpEvents_RtpStreamAdded(object sender, RtpEvents.RtpStreamEventArgs ea)
        {
            ea.RtpStream.FrameReceived += RtpStream_FrameReceived;
        }
        void RtpEvents_RtpStreamRemoved(object sender, RtpEvents.RtpStreamEventArgs ea)
        {
            ea.RtpStream.FrameReceived -= RtpStream_FrameReceived;
        }
        
        void RtpStream_FrameReceived(object sender, RtpStream.FrameReceivedEventArgs ea)
        {

         //   System.Diagnostics.Debug.WriteLine(ea.RtpStream.Properties.CName+""+ea.RtpStream.Properties.Name);
          window.Dispatcher.Invoke((Action)(() => {
              if (image == null && vpList!=null)
              {
                  System.IO.MemoryStream ms = new System.IO.MemoryStream(ea.Frame.Buffer);
  
                  int sizeBytes = 0;
                  Byte[] buffer = new Byte[4];
                  ms.Read(buffer, 0, 4);
                  sizeBytes = BitConverter.ToInt32(buffer, 0);
                    Byte[] imageData = new Byte[sizeBytes];
                    ms.Read(imageData, 0, imageData.Length);
                  string[] hostIPS = ea.RtpStream.Properties.CName.Split('#');

                  foreach (var ip in hostIPS)
                  {
                      if (vpList.ContainsKey(IPAddress.Parse(ip)))
                      {
                          vpList[IPAddress.Parse(ip)].prev.Source = GetImage(imageData).Source;
                          break;
                      }
                  }
                  Byte[] audio = new Byte[ms.Length - ms.Position];
                  ms.Read(audio, 0, audio.Length);

                  {
                    
                          waveProvider.AddSamples(audio, 0, audio.Length);
                          if (waveOut.PlaybackState != PlaybackState.Playing)
                          {
                              waveOut.Play();

                          }
                      
                  }
              }else if(vpList==null && image!=null)
              {
                  image.Source = GetImage(ea.Frame.Buffer).Source;
              }
              else if (vpList == null && image == null)
              {
                  System.IO.MemoryStream ms = new System.IO.MemoryStream(ea.Frame.Buffer);
                  
                  Byte[] audio = new Byte[ms.Length ];
                  ms.Read(audio, 0, audio.Length);
                  if (ea.RtpStream.FramesReceived > 10)
                      waveProvider.AddSamples(audio, 0, audio.Length);
          //        if (waveOut.PlaybackState != PlaybackState.Playing)
                      waveOut.Play();
                  string[] hostIPS = ea.RtpStream.Properties.CName.Split('#');
                  foreach (var ip in hostIPS)
                  {
                      IPAddress IP=IPAddress.Parse(ip);
                      if (apList.ContainsKey(IP))
                      {
                        if(apList[IP].canRecord)
                        {
                            string Path = AppDomain.CurrentDomain.BaseDirectory + "\\" + IP.ToString().Replace('.', '\\').ToString() + "\\" + DateTime.Now.ToString("yy.MM.dd.HH.mm.ss.ffff") + ".wav";
                            if (!waveWriters.ContainsKey(IP))
                                waveWriters.Add(IP,new WaveFileWriter(Path, AudioConf.audio.sourceStream.WaveFormat));

                            waveWriters[IP].WriteData(audio, 0, audio.Length);
                            waveWriters[IP].Flush();
                            
                        }
                        else
                        {

                        }
                      }
                  }
              }
                
         }));
                   
        }

        public Image GetImage(byte[] _data)
        {

            JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(_data), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            Image image = new Image();
            image.Source = decoder.Frames[0];
            image.Stretch = System.Windows.Media.Stretch.Fill;
            return image;
        }
        private void RtpParticipantRemoved(object sender, RtpEvents.RtpParticipantEventArgs ea)
        {
            
        }

        private void RtpParticipantAdded(object sender, RtpEvents.RtpParticipantEventArgs ea)
        {
            
        }

        public void Dispose()
        {
            RtpEvents.RtpParticipantAdded -= RtpParticipantAdded;
            RtpEvents.RtpParticipantRemoved -= RtpParticipantRemoved;
            RtpEvents.RtpStreamAdded -= RtpEvents_RtpStreamAdded;
            RtpEvents.RtpStreamRemoved -= RtpEvents_RtpStreamRemoved;
            if(waveWriters!=null)
            foreach(var wave in waveWriters.Values)
            {
                wave.Close();
                
            }
            if (waveOut != null)
                waveOut.Dispose();
            if (rtpSession != null)
            {
                rtpSession.Dispose();
                rtpSession = null;
                rtpSender = null;
            }
        }
        #region backup
        //public void SetConfWind(VideoConf vc){
        //    this.vcWind = vc;
        //}
        //public void SendRTPMessageTo(Byte[] value, IPAddress ip)
        //{
        //    rtpSendClient.Connect(new IPEndPoint(ip, (int)MainWindow.Ports.RTP));
        //    rtpSendClient.Send(value, value.Length);
        //    System.Diagnostics.Debug.WriteLine("RTP:||-----Sending:" + value.Length + " to " + ip.ToString() + "------");

        //}
        //public RTPPacket[] MakeRTPPacketS(byte[] data)
        //{
        //    return null;
        //}

        //public void listener_proc()
        //{
        //    try { 
        //    rtpReceClient.Client.ReceiveBufferSize = 1024 * 1024;
        //    while (listening)
        //    {
        //        byte[] data;
        //        IPEndPoint recevied = new IPEndPoint(IPAddress.Any, port);
        //        data = rtpReceClient.Receive(ref recevied);
              
        //        RTPPacket _Packet = new RTPPacket(data);
        //        if (_Packet.Fragment ==  (uint)0 && _Packet.SequenceNumber!=0)
        //        {
        //            vcWind.Dispatcher.Invoke((Action)(() =>
        //            {
        //                vcWind.vp[recevied.Address].prev.Source = GetImage(rBuffer[recevied.Address].GetBuffer()).Source;
        //            }));
        //            rBuffer[recevied.Address].SetLength(0);
        //            }
        //        rBuffer[recevied.Address].Write(_Packet.data, 0, _Packet.data.Length);
        //         //string stringData = Encoding.ASCII.GetString(data);
        //        //System.Diagnostics.Debug.WriteLine("RTP||-----Recevied " + stringData + " from " + recevied.Address + " ----");

       
        //    }
        //        }catch(Exception e)
        //    {

        //    }
        //}
        //public static int GetRTPHeaderValue(byte[] packet, int startBit, int endBit)
        //{
        //    int result = 0;

        //    // Number of bits in value
        //    int length = endBit - startBit + 1;

        //    // Values in RTP header are big endian, so need to do these conversions
        //    for (int i = startBit; i <= endBit; i++)
        //    {
        //        int byteIndex = i / 8;
        //        int bitShift = 7 - (i % 8);
        //        result += ((packet[byteIndex] >> bitShift) & 1) * (int)Math.Pow(2, length - i + startBit - 1);
        //    }
        //    return result;
        //}
        //public void Start()
        //{
           
        //    listening = true;
        //    listen_thread.Start();
        //}
        //public void Stop()
        //{
        //    listening = false;
        //    rtpReceClient.Close();
        //}
        //int seq = 0;
        //public List<byte[]> GetPackets(Byte[] data,int Width,int Height)
        //{
        //    List<byte[]> packets=new List<byte[]>();
        //    uint LastFragment =0;
     
        //    int timeStamp=0;
        //    while (LastFragment < data.Length)
        //    {
        //        RTPPacket p = new RTPPacket(data.Skip((int)LastFragment).Take(Math.Min(data.Length, RTPPacket.DEFAULTSIZE)).ToArray(), seq++, timeStamp++, LastFragment, RTPPacket.DEFAULTSIZE, Width, Height);
        //        LastFragment += RTPPacket.DEFAULTSIZE - 20;
        //        packets.Add(p.packet);
        //    }
        //    return packets;
        //}
        //public class RTPPacket
        //{
        //  public  const int DEFAULTSIZE=1024*50;
        //   public  Byte[] data;
        //   public Byte[] packet;
        //   public uint Timestamp;
        //   public ushort SequenceNumber;
        //   public uint Width;
        //   public uint Height;
        //   public uint Fragment;
           
        //    public RTPPacket(byte[] data, int SequenceNumber, int Timestamp,uint Fragment,int Size,int Width,int Height) {
        //        this.data = data;
        //        this.Timestamp =(uint) Timestamp;
        //        this.SequenceNumber = (ushort)SequenceNumber;
                
        //        this.packet = new Byte[Size];
        //        this.Width = (uint)Width;
        //        this.Height = (uint)Height;
        //        this.Fragment = (uint)Fragment;
        //        EncodeRTPHeader();
        //        EncodeJPEGHeader();
        //        AppendData();
        //    }
        //    public RTPPacket(byte[] packet)
        //    {
        //        this.packet = packet;
        //        DecodeData();
        //    }
        //    //public int version
        //    //{
        //    //    get { return GetRTPHeaderValue(packet, 0, 1); }
        //    //}
        //    //public int padding
        //    //{
        //    //    get { return GetRTPHeaderValue(data, 2, 2); }
        //    //}
        //    //public int extension
        //    //{
        //    //    get { return GetRTPHeaderValue(data, 3, 3); }
        //    //}
        //    //public int csrcCount
        //    //{
        //    //    get { return GetRTPHeaderValue(packet, 4, 7); }
        //    //}
        //    //public int marker
        //    //{
        //    //    get { return GetRTPHeaderValue(packet, 8, 8); }
        //    //}
        //    //public int payloadType
        //    //{
        //    //    get { return GetRTPHeaderValue(packet, 9, 15); }
        //    //}
        //    //public int sequenceNum
        //    //{
        //    //    get { return GetRTPHeaderValue(packet, 16, 31); }
        //    //}
        //    //public int timestamp { get { return GetRTPHeaderValue(packet, 32, 63); } }
        //    //public int ssrcId { get { return GetRTPHeaderValue(packet, 64, 95);}}
            
        //    private void EncodeRTPHeader(){
        //        Byte[] RtpBuf=new Byte[12];
        //        RtpBuf[0]  = 0x80;                               
        //        RtpBuf[1]  = 0x9a;                               
        //        RtpBuf[2] = (byte)(SequenceNumber & 0x0FF);          
        //        RtpBuf[3] = (byte)(SequenceNumber >> 8);
        //        //RtpBuf[4] = (byte)((Timestamp & 0xFF000000) >> 24);  
        //        //RtpBuf[5] = (byte)((Timestamp & 0x00FF0000) >> 16);
        //        //RtpBuf[6] = (byte)((Timestamp & 0x0000FF00) >> 8);
        //        //RtpBuf[7] = (byte)(Timestamp & 0x000000FF);
        //        BitConverter.GetBytes(Timestamp).CopyTo(RtpBuf, 4);
        //        RtpBuf[8] = 0x13;                          
        //        RtpBuf[9] = 0xf9;                           
        //        RtpBuf[10] = 0x7e;
        //        RtpBuf[11] = 0x67;
        //        RtpBuf.CopyTo(packet,0);

        //    }
        //    private void EncodeJPEGHeader()
        //    {
        //        Byte[] jpgBuf = new Byte[8];
        //        jpgBuf[0] = 0x00;
        //        jpgBuf[1] = (byte)((Fragment >> 0x00FF0000) >> 16);
        //        jpgBuf[2] = (byte)((Fragment >> 0x0000FF00) >> 8);
        //        jpgBuf[3] = ((byte)(Fragment >> 0x000000FF));
        //        jpgBuf[4] = 0x01;
        //        jpgBuf[5] = 0x5e;
        //        jpgBuf[6] =(byte)( Width >> 3);
        //        jpgBuf[7] =(byte)( Height >> 3);
        //        jpgBuf.CopyTo(packet, 12);

        //    }
        //    private void AppendData()
        //    {
        //        data.Take(Math.Min(data.Length,packet.Length-20)).ToArray().CopyTo(packet, 20);
               
        //    }

        //    public void DecodeData()
        //    {
        //        SequenceNumber = (ushort)BitConverter.ToUInt16(packet.Take(4).ToArray(),2);
        //        Timestamp = (ushort)BitConverter.ToUInt32(packet.Take(8).ToArray(), 4);
        //        Fragment = (uint)BitConverter.ToUInt32(packet.Take(16).ToArray(), 12);
        //        data = packet.Skip(20).ToArray();
        //    }
        //}
        #endregion



    }
}
