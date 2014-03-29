#define UDP
#if UDP
#define UDPConnection
#endif
//#define Fake
///<Debug>
///(1) For Debuging UDP sending/reciving data  verbose ... Define UDP
///(2) For UDP sending/reciving data verbose ... Define VideoCall
///(3) For Testing As a Fake User ...Define Fake
///</Debug>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
namespace SEN_project_v2
{
    public class UDP
    {
        public UdpClient recevingClient = null;
        public UdpClient sendingClient = null;
        public UdpClient rtpReceClient = null;
        public UdpClient rtpSendClient = null;
        public const string Connect = "<#Connect#>";
        public const string RConnect = "<\\#Connect#>";
        public const string Disconnect = "<#Disconnect#>";
        public const string Message = "<#Message#>";
        public const string Videocall = "<#VideoCall>";
        public const string RVideocall = "<\\#VideoCall>";
        public const string AddMember = "<#Add#>"; /// Format <#Add#>+"UserIP"
        public const string Breaker = "<#>";
        public const string RemoveMember = "<#Remove#>";
        
        private int port;
        private MainWindow window;

        public UDP(int port)
        {
            if (port == (int)MainWindow.Ports.UDP)
            {
                recevingClient = new UdpClient(port);
               
            }
            else if (port == (int)MainWindow.Ports.RTP)
            {
                rtpReceClient = new UdpClient(port);
              
            }
            sendingClient = new UdpClient();
            rtpSendClient = new UdpClient();
            this.port = port;


        }
        public void SendMessageTo(string value, IPAddress ip)
        {
            sendingClient.Connect(new IPEndPoint(ip, port));
            sendingClient.Send(Encoding.ASCII.GetBytes(value), value.Length);
#if UDP
            System.Diagnostics.Debug.WriteLine("UDP:||-----Sending:" + value + " to " + ip.ToString() + "------");
#endif
        }

        public void recevingThread()
        {
            recevingClient.Client.ReceiveBufferSize = 1024 * 1024;

            while (true)
            {


                byte[] data;
                IPEndPoint recevied = new IPEndPoint(IPAddress.Any, port);

                data = recevingClient.Receive(ref recevied);
                string stringData = Encoding.ASCII.GetString(data);
#if UDPConnection
                System.Diagnostics.Debug.WriteLine("UDP||-----Recevied " + stringData + " from " + recevied.Address + " ----");
#endif   


                #region Connection
                if (stringData.StartsWith(Connect))
                {
                    string[] splits = stringData.Split(new String[] { Connect, Breaker }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length != 2)
                    {
#if UDPConnection
                        System.Diagnostics.Debug.WriteLine("-----" + recevied.Address + "Does  not Contain PC/Nick Name----");
#endif
                        continue;
                    }

                    receviedConnect(recevied, splits);
                }
                else if (stringData.StartsWith(RConnect))
                {
                    string[] splits = stringData.Split(new String[] { RConnect, Breaker }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length != 2)
                    {
#if UDPConnection
                        System.Diagnostics.Debug.WriteLine("-----" + recevied.Address + "Does  not Contain PC/Nick Name/Group Name----");
#endif
                        continue;
                    }
                    receviedRConnect(recevied, splits);
         
                }

                else if (stringData.StartsWith(Disconnect))
                {
                    string[] splits = stringData.Split(new String[] { Disconnect }, StringSplitOptions.RemoveEmptyEntries);
                    receviedDisconnect(recevied, splits);
                }
                #endregion

                #region VideoCall Connection

                else if (stringData.StartsWith(Videocall))
                {
                    window.Dispatcher.Invoke((Action)(() => { window.CreateVideoConf(recevied.Address); }));

                }
                else if (stringData.StartsWith(RVideocall))
                {

                    receviedRVoiceCall(recevied);
                }
                else if (stringData.StartsWith(AddMember))
                {
                    string[] splits = stringData.Split(new String[] { AddMember }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 0)
                    {
                        window.videoConf.Dispatcher.Invoke((Action)(() =>
                        {
                            window.videoConf.AddUser(IPAddress.Parse(splits[0]));
                            window.videoConf.MakeUserPreview(IPAddress.Parse(splits[0]), VideoPreview.Mode.InCall);
                        }));
                    }


                }
                else if (stringData.StartsWith(RemoveMember))
                {
                    window.videoConf.Dispatcher.Invoke((Action)(() =>
                    {
                        window.videoConf.requestedUsers.Remove(recevied.Address);
                        window.videoConf._stack.Children.Remove(window.videoConf.vp[recevied.Address]);
                        window.videoConf.statusLabel.Content = UserList.Get(recevied.Address).nick + " can't Join :(";
                        if (window.videoConf.Users.Count == window.videoConf.requestedUsers.Count)
                            window.videoConf.statusLabel.Content = "Room Created Successfully With (" + window.videoConf.requestedUsers.Count + ")Members ...:D";

                    }));
                }

                #endregion

                #region Message
                else if (stringData.StartsWith(Message))
                {
                    String[] splits = stringData.Split(new String[] { Message }, StringSplitOptions.RemoveEmptyEntries);
                    receviedMessage(recevied, splits);
                 


                }
              #endregion
            }


        }
        private void receviedConnect(IPEndPoint recevied, String[] splits)
        {
            if (MainWindow.hostIPS.Contains(recevied.Address))
                MainWindow.hostIP = recevied.Address;

           
            
            SendMessageTo(RConnect + Environment.MachineName + Breaker + Environment.MachineName, recevied.Address);
#if UDPConnection
            System.Diagnostics.Debug.WriteLine("-----Sending:" + RConnect + "------");
#endif

#if !Fake
            User user = new User(recevied.Address, splits[0],splits[1]);
#endif
#if Fake
                    User[] list=new User[25];

                    for (int i = 0; i < 25;i++ )
                    {

                        list[i] = new User(IPAddress.Parse("127.0.0." + i), "FakeUser" + i,"FakeGroup"+i/5);
                        User user = list[i];
#endif
            if (UserList.Add(user))
                window.Dispatcher.Invoke((Action)(() =>
                {
                    //this.window.AddToUserList(user.CreateView(),splits[1]);
                    window.AddUserToTree(user);
                }));
#if Fake
                    }
#endif


        }
        private void receviedRConnect(IPEndPoint recevied, String[] splits)
        {
            User user = new User(recevied.Address, splits[0],splits[ 1]);

            if (UserList.Add(user))
                window.Dispatcher.Invoke((Action)(() =>
                {
                    
                    window.AddUserToTree(user);
                }));

        }
        private void receviedDisconnect(IPEndPoint recevied,String[] splits)
        {
            User user = new User(recevied.Address, "Disconnecting", splits[0]);
            UserList.Add(user);
            UserList.Remove(recevied.Address);
            window.Dispatcher.Invoke((Action)(() =>
            {
                window.RemoveUserFromTree(user);
                
            }));
        }
        private void receviedMessage(IPEndPoint recevied,String[] splits)
        {
      
          
            if (splits.Length > 0)
            {
                MessageBox.Show("Message from ..." + UserList.Get(recevied.Address).nick + splits[0]);
                UserList.xml[recevied.Address].addMessage(DateTime.Now, splits[0]);
            }
        }
        private void receviedRVoiceCall(IPEndPoint recevied)
        {
            foreach (IPAddress ip in window.videoConf.Users)
            {
                SendMessageTo(AddMember + recevied.Address + AddMember, ip);
            }
            foreach (IPAddress ip in window.videoConf.Users)
            {
                SendMessageTo(AddMember + ip.ToString() + AddMember, recevied.Address);
            }
            window.videoConf.Users.Add(recevied.Address);
            window.videoConf.Dispatcher.Invoke((Action)(() =>
            {
                window.videoConf.vp[recevied.Address]._Mode = VideoPreview.Mode.InCall;
                if (window.videoConf.Users.Count == window.videoConf.requestedUsers.Count)
                    window.videoConf.statusLabel.Content = "Room Created Successfully...";

            }));
        }
        public void SetWindow(MainWindow window)
        {
            this.window = window;
        }
  
    }
}