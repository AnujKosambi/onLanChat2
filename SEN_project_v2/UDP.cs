using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media;
using RTPLib;
using System.Runtime.InteropServices;
namespace SEN_project_v2
{
    class UDP
    {
        public static UdpClient recevingClient = null;
        public static UdpClient sendingClient = null;
        public const string Connect = "<#Connect#>";
        public const string RConnect = "<\\#Connect#>";
        public const string Disconnect = "<#Disconnect#>";
        public const string Message = "<#Message#>";
        public const string Videocall = "<#VideoCall>";
        public const string RVideocall = "<\\#VideoCall>";

        private int port;
        private MainWindow window;
        public UDP(int port)
        {
            recevingClient = new UdpClient(port);
            sendingClient = new UdpClient();
            this.port = port;

        }

        public void SendMessageTo(string value, IPAddress ip)
        {
            sendingClient.Connect(new IPEndPoint(ip, port));
            sendingClient.Send(Encoding.ASCII.GetBytes(value), value.Length);
        }
        public void recevingThread()
        {
            recevingClient.Client.ReceiveBufferSize = 1024 * 1024;
       

            bool sync = false;
            while (true)
            {

                       
                byte[] data;
                IPEndPoint recevied = new IPEndPoint(IPAddress.Any, port);
                data = recevingClient.Receive(ref recevied);

                string stringData = Encoding.ASCII.GetString(data);
                #region
                // window.Dispatcher.Invoke((Action)(() =>
               // {
               //     this.window.status.Text = "-----Received:" + stringData.Length+ "------";
               // }));
               //System.Diagnostics.Debug.WriteLine("-----Received:" + stringData.Length +" " +string.Join(" ", data.Skip(0).Select(x => Convert.ToString(x, 16).PadLeft(2,'0')).ToArray()) + "------");
               // if (true)
               // {
               //     int version = RTPClient.GetRTPHeaderValue(data, 0, 1);
               //     int padding = RTPClient.GetRTPHeaderValue(data, 2, 2);
               //     int extension = RTPClient.GetRTPHeaderValue(data, 3, 3);
               //     int csrcCount = RTPClient.GetRTPHeaderValue(data, 4, 7);
               //     int marker = RTPClient.GetRTPHeaderValue(data, 8, 8);
               //     int payloadType = RTPClient.GetRTPHeaderValue(data, 9, 15);
               //     int sequenceNum = RTPClient.GetRTPHeaderValue(data, 16, 31);
               //     int timestamp = RTPClient.GetRTPHeaderValue(data, 32, 63);
               //     int ssrcId = RTPClient.GetRTPHeaderValue(data, 64, 95);
               //     //System.Diagnostics.Debug.Print("\n{0} {1} {2} {3} {4} {5} {6} {7} {8}",
               //     //    version, padding, extension, csrcCount, marker, payloadType,
               //     //    sequenceNum, timestamp, ssrcId);

                // }
                #endregion

                #region Connection
                if (stringData.StartsWith(Connect))
                {
                      string[] splits = stringData.Split(new String[] { Connect }, StringSplitOptions.RemoveEmptyEntries);
                      if(splits.Length==0)
                      {
                          System.Diagnostics.Debug.WriteLine("-----" + recevied.Address + "Does  not Contain PC/Nick Name----");
                          continue;
                      }
                    
                      SendMessageTo(RConnect+Environment.MachineName,recevied.Address);
                      System.Diagnostics.Debug.WriteLine("-----Sending:" + RConnect + "------");
                    
                     User user = new User(recevied.Address, splits[0]);
                    if(UserList.Add(user))
                    window.Dispatcher.Invoke( (Action)(() =>
                    {
                    this.window.AddToUserList(user.CreateView());
                    }));

                }
                else if (stringData.StartsWith(RConnect))
                {
                    string[] splits = stringData.Split(new String[] { RConnect }, StringSplitOptions.RemoveEmptyEntries);
                    User user = new User(recevied.Address, splits[0]);
                    if (UserList.Add(user))
                        window.Dispatcher.Invoke((Action)(() =>
                        {
                            this.window.AddToUserList(user.CreateView());
                        }));

                }

                else if(stringData.StartsWith(Disconnect))
                {
                    User user = new User(recevied.Address, "Disconnecting..");
                    UserList.Add(user);
                    UserList.Remove(recevied.Address);
                    window.Dispatcher.Invoke((Action)(() =>
                    {
                        this.window.RemoverFromUserList(recevied.Address);
                    }));
                }
                #endregion
                #region VideoCall Connection
                else if(stringData.StartsWith(Videocall))
                {

                }
                else if(stringData.StartsWith(RVideocall))
                {

                }
                     
#endregion


            }


        }

        public void SetWindow(MainWindow window)
        {
            this.window = window;
        }
    }
}
