using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
//using System.Windows.Threading;
namespace SEN_project_v2
{
    public class RTPClient
    {
        private MemoryStream stream;
        private UdpClient udpClient;
        private IPEndPoint ipendPoint;
        public Thread listen;
        private bool listening=false;
        public RTPClient()
        {
            stream = new MemoryStream();
            listen = new Thread(new ThreadStart(listener_proc));
    
        }
        public void listener_proc()
        {
            while(true)
            {
              
                byte[] bpacket = udpClient.Receive(ref ipendPoint);
                RTPPacket packet = new RTPPacket(bpacket);

                    
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
            listen.Start();
        }
        public void Stop()
        {
            listening = false;
        }
        public void Connect(IPEndPoint ipe)
        {
            udpClient = new UdpClient(5004);
            this.ipendPoint = ipe;
            udpClient.Connect(ipendPoint);
        }

        public class RTPPacket
        {
            Byte[] packet;
            public RTPPacket(byte[] packet) { this.packet = packet; }
            public int version
            {
                get { return GetRTPHeaderValue(packet, 0, 1); }
            }
            public int padding
            {
                get { return GetRTPHeaderValue(packet, 2, 2); }
            }
            public int extension
            {
                get { return GetRTPHeaderValue(packet, 3, 3); }
            }
            public int csrcCount
            {
                get { return GetRTPHeaderValue(packet, 4, 7); }
            }
            public int marker
            {
                get { return GetRTPHeaderValue(packet, 8, 8); }
            }
            public int payloadType
            {
                get { return GetRTPHeaderValue(packet, 9, 15); }
            }
            public int sequenceNum
            {
                get { return GetRTPHeaderValue(packet, 16, 31); }
            }
            public int timestamp { get { return GetRTPHeaderValue(packet, 32, 63); } }
            public int ssrcId { get { return GetRTPHeaderValue(packet, 64, 95);}}

        }
    
    }
}
