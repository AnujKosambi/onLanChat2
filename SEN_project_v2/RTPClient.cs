using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
//using System.Windows.Threading;
namespace SEN_project_v2
{
    public class RTPClient
    {
        private MemoryStream stream;
        private UdpClient rtpReceClient;
        private UdpClient rtpSendClient;
        private IPAddress client_ip;
        public Thread listen_thread;
        private VideoConf vcWind;
        private bool listening=false;
        private int port;
        public RTPClient()
        {
            stream = new MemoryStream();
            listen_thread = new Thread(new ThreadStart(listener_proc));
            listen_thread.SetApartmentState(ApartmentState.STA);
            port =(int)MainWindow.Ports.RTP;
            rtpSendClient = new UdpClient();
            rtpReceClient = new UdpClient((int)MainWindow.Ports.RTP);
        }
        public void SetConfWind(VideoConf vc){
            this.vcWind = vc;
        }
        public void SendRTPMessageTo(Byte[] value, IPAddress ip)
        {
            rtpSendClient.Connect(new IPEndPoint(ip, (int)MainWindow.Ports.RTP));
            rtpSendClient.Send(value, value.Length);
            System.Diagnostics.Debug.WriteLine("RTP:||-----Sending:" + value.Length + " to " + ip.ToString() + "------");

        }
        public RTPPacket[] MakeRTPPacketS(byte[] data)
        {
            return null;
        }
        private Image GetImage(byte[] data)
        {
         
            JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(data), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            Image image = new Image();
            image.Source = decoder.Frames[0];
            image.Stretch = System.Windows.Media.Stretch.Fill;
            return image;
        }
        public void listener_proc()
        {
            rtpReceClient.Client.ReceiveBufferSize = 1024 * 1024;
            while (listening)
            {
                byte[] data;
                IPEndPoint recevied = new IPEndPoint(IPAddress.Any, port);
                data = rtpReceClient.Receive(ref recevied);
                //string stringData = Encoding.ASCII.GetString(data);
                //System.Diagnostics.Debug.WriteLine("RTP||-----Recevied " + stringData + " from " + recevied.Address + " ----");
                vcWind.Dispatcher.Invoke((Action)(() =>
                {
                    vcWind.vp[recevied.Address].prev.Source = GetImage(data).Source;
                }))
                ;
            }
        }
        public static int GetRTPHeaderValue(byte[] packet, int startBit, int endBit)
        {
            int result = 0;

            // Number of bits in value
            int length = endBit - startBit + 1;

            // Values in RTP header are big endian, so need to do these conversions
            for (int i = startBit; i <= endBit; i++)
            {
                int byteIndex = i / 8;
                int bitShift = 7 - (i % 8);
                result += ((packet[byteIndex] >> bitShift) & 1) * (int)Math.Pow(2, length - i + startBit - 1);
            }
            return result;
        }
        public void Start()
        {
           
            listening = true;
            listen_thread.Start();
        }
        public void Stop()
        {
            listening = false;
            rtpReceClient.Close();
        }
    

        public class RTPPacket
        {
            Byte[] data;
            Byte[] packet;
            int Timestamp;
            int SequenceNumber;
            
            public RTPPacket(byte[] data, int SequenceNumber, int Timestamp,int Size) {
                this.data = data;
                this.Timestamp = Timestamp;
                this.SequenceNumber = SequenceNumber;
                this.packet = new Byte[Size];
            }
            //public int version
            //{
            //    get { return GetRTPHeaderValue(packet, 0, 1); }
            //}
            //public int padding
            //{
            //    get { return GetRTPHeaderValue(data, 2, 2); }
            //}
            //public int extension
            //{
            //    get { return GetRTPHeaderValue(data, 3, 3); }
            //}
            //public int csrcCount
            //{
            //    get { return GetRTPHeaderValue(packet, 4, 7); }
            //}
            //public int marker
            //{
            //    get { return GetRTPHeaderValue(packet, 8, 8); }
            //}
            //public int payloadType
            //{
            //    get { return GetRTPHeaderValue(packet, 9, 15); }
            //}
            //public int sequenceNum
            //{
            //    get { return GetRTPHeaderValue(packet, 16, 31); }
            //}
            //public int timestamp { get { return GetRTPHeaderValue(packet, 32, 63); } }
            //public int ssrcId { get { return GetRTPHeaderValue(packet, 64, 95);}}

            private void EncodeRTPHeader(){
                Byte[] RtpBuf=new Byte[12];
                RtpBuf[0]  = 0x80;                               // RTP version
                RtpBuf[1]  = 0x9a;                               // JPEG payload (26) and marker bit
                RtpBuf[2] = (byte)(SequenceNumber & 0x0FF);           // each packet is counted with a sequence counter
                RtpBuf[3] = (byte)(SequenceNumber >> 8);
                RtpBuf[4] = (byte)((Timestamp & 0xFF000000) >> 24);   // each image gets a timestamp
                RtpBuf[5] = (byte)((Timestamp & 0x00FF0000) >> 16);
                RtpBuf[6] = (byte)((Timestamp & 0x0000FF00) >> 8);
                RtpBuf[7] = (byte)(Timestamp & 0x000000FF);
                RtpBuf[8] = 0x13;                               // 4 byte SSRC (sychronization source identifier)
                RtpBuf[9] = 0xf9;                               // we just an arbitrary number here to keep it simple
                RtpBuf[10] = 0x7e;
                RtpBuf[11] = 0x67;
                RtpBuf.CopyTo(packet,0);

            }

        }
    
    }
}
