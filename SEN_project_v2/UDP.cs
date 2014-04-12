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
using Microsoft.Win32;
using System.Xml;

namespace SEN_project_v2
{
    public class UDP
    {
        #region Fields
        public UdpClient recevingClient = null;
        public UdpClient sendingClient = null;
        public UdpClient rtpReceClient = null;
        public UdpClient rtpSendClient = null;
        public const string Connect = "<#Connect#>";
        public const string RConnect = "<\\#Connect#>";
        public const string Disconnect = "<#Disconnect#>";
        public const string Message = "<#Message#>";
        public const string RMessage = "<#RMessage#>";
        public const string MConnect = "<#MConnect#>";
        public const string RMConnect = "<\\#MConnect#>";
        public const string MMessage = "<#MMessage#>";

        public const string Videocall = "<#VideoCall#>";
        public const string RVideocall = "<\\#VideoCall#>";
        public const string Audiocall = "<#Audiocall#>";
        public const string RAudiocall = "<\\#Audiocall#>";
        public const string ExitCallA = "<#ExitCallA#>";
        public const string ExitCall = "<#ExitCall#>";
        public const string AddMember = "<#Add#>"; /// Format <#Add#>+"UserIP"
        public const string RemoveMember = "<#Remove#>";
        public const string AddMemberA = "<#AddA#>"; /// Format <#Add#>+"UserIP"
        public const string RemoveMemberA = "<#RemoveA#>";
        public const string Remote = "<#Remote#>";
        public const string RRemote = "<\\#Remote#>";
        public const string MobileMouse = "<#MoblieMouse#>";


        public const string Sharing = "<#Sharing#>";
        public const string SendFile = "<#SendFile#>";
        public const string SendDir = "<#SendDir#>";
        public const string RSendDir = "<#RSendDir#>";
        public const string MakeFolder = "<#MakeFolder#>";
        public const string UpdatePic = "<#UpdatePic#>";
        
        public const string EndRemote = "<XEndX>";
        public const string Mouse = "<#M#>";
        public const string Keyboard = "<#K#>";
        public const string Breaker = "<#>";
        private int port;
        private MainWindow window;
        private XmlDocument settings = new XmlDocument();
        private List<String> blocked = new List<String>();
        #endregion
        string nickname, groupname;
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
           
            if (System.IO.File.Exists("UserSettings.xml") == true)
            {
              
                settings.Load(AppDomain.CurrentDomain.BaseDirectory+ "\\UserSettings.xml");
                nickname = settings.SelectSingleNode("UserProfile/General/NickName").InnerText;
                groupname = settings.SelectSingleNode("UserProfile/General/GroupName").InnerText;
            }
            else
            {
                nickname = Environment.MachineName;
                groupname = Environment.MachineName;
            }
            sendingClient = new UdpClient();
            rtpSendClient = new UdpClient();
            this.port = port;


        }
        public void SendMessageTo(string value, IPAddress ip)
        {
            sendingClient.Connect(new IPEndPoint(ip, port));
            sendingClient.Send(Encoding.ASCII.GetBytes(value), value.Length);
#if UDPConnection
            System.Diagnostics.Debug.WriteLine("UDP:||-----Sending:" + value + " to " + ip.ToString() + "------");
#endif
        }
        public void SendMessageTo(string value, IPAddress ip,string category)
        {
            sendingClient.Connect(new IPEndPoint(ip, port));
            sendingClient.Send(Encoding.ASCII.GetBytes(category+UDP.Breaker+value), value.Length);
#if UDPConnection
            System.Diagnostics.Debug.WriteLine("UDP:||-----Sending:" + value + " to " + ip.ToString() + "------");
#endif
        }
        public void SendMessageTo(Byte[] value, IPAddress ip, String category)
        {
            // Connects to the client ip and sends the message

            sendingClient.Connect(new IPEndPoint(ip, port));
            //            System.Diagnostics.Debug.WriteLine("\nvalue---" + System.Text.Encoding.Default.GetString(value));
            sendingClient.Send(value.Concat(Encoding.ASCII.GetBytes(Breaker + category)).ToArray(), value.Length + Breaker.Length + category.Length);
#if UDP
            System.Diagnostics.Debug.WriteLine("\nUDP:||-----Sending:" + System.Text.Encoding.Default.GetString(value) + " to " + ip.ToString() + "------");
#endif
        }
        public void SendMessageTo(Byte[] value, IPAddress ip)
        {
            sendingClient.Connect(new IPEndPoint(ip, port));
            sendingClient.Send(value, value.Length);
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
                settings.Load(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml");
                System.Diagnostics.Debug.WriteLine("UDP||-----Recevied " + stringData + " from " + recevied.Address + " ----");
                int asdf = 0;
                foreach (XmlNode user in settings.SelectNodes("UserProfile/BlockedList/Users/Blockeduser"))
                {
                    if (user.InnerText.Equals(recevied.Address.ToString()))
                    {
                        asdf = 100;
                        break;
                    }
                }
                if(asdf==100)
                {
                    continue;
                }

                

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
                    if (splits.Length < 2)
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
                    if(window.videoConf!=null)
                    receviedRVoiceCall(recevied);
                }
                else if (stringData.StartsWith(AddMember))
                {
                    string[] splits = stringData.Split(new String[] { AddMember }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 0)
                    {
                        if (window.videoConf == null)
                        {
                            window.Dispatcher.Invoke((Action)(() => { window.CreateVideoConf(recevied.Address); }));
                        }
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
                else if (stringData.StartsWith(ExitCall))
                {
                    if (window.waiting != null)
                    {
                        window.waiting.Dispatcher.Invoke((Action)(() =>
                        {
                            window.waiting.Close();
                        }));
                    }
                    if (window.videoConf != null)
                    {
                        window.videoConf.Dispatcher.Invoke((Action)(() =>
                        {
                            window.videoConf.Close();
                        }));
                    }

                }

                #endregion

                #region AudioCall Connection

                else if (stringData.StartsWith(Audiocall))
                {
                    window.Dispatcher.Invoke((Action)(() => { window.CreateAudioConf(recevied.Address); }));

                }
                else if (stringData.StartsWith(RAudiocall))
                {
                    if (window.audioConf != null)
                        receviedRAudioCall(recevied);
                }
                else if (stringData.StartsWith(AddMemberA))
                {
                    string[] splits = stringData.Split(new String[] { AddMemberA }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 0)
                    {
                        if (window.audioConf == null)
                        {
                            window.Dispatcher.Invoke((Action)(() => { window.CreateAudioConf(recevied.Address); }));
                        }
                        window.audioConf.Dispatcher.Invoke((Action)(() =>
                        {
                           window.audioConf.AddUser(IPAddress.Parse(splits[0]));
                            window.audioConf.MakeUserPreview(IPAddress.Parse(splits[0]), AudioPreview.Mode.InCall);
                        }));
                    }


                }
                else if (stringData.StartsWith(RemoveMemberA))
                {
                    window.audioConf.Dispatcher.Invoke((Action)(() =>
                    {
                        window.audioConf.requestedUsers.Remove(recevied.Address);
                        window.audioConf._stack.Children.Remove(window.audioConf.vp[recevied.Address]);
                        window.audioConf.statusLabel.Content = UserList.Get(recevied.Address).nick + " can't Join :(";
                        if (window.audioConf.Users.Count == window.audioConf.requestedUsers.Count)
                            window.audioConf.statusLabel.Content = "Room Created Successfully With (" + window.audioConf.requestedUsers.Count + ")Members ...:D";

                    }));
                }
                else if (stringData.StartsWith(ExitCallA))
                {
                    if (window.waiting != null)
                    {
                        window.waiting.Dispatcher.Invoke((Action)(() =>
                        {
                            window.waiting.Close();
                        }));
                    }
                    if (window.audioConf != null)
                    {
                        window.audioConf.Dispatcher.Invoke((Action)(() =>
                        {
                            window.audioConf.Close();
                        }));
                    }

                }

                #endregion
                #region Remote
                else if (stringData.StartsWith(Remote))
                {
                    window.Dispatcher.Invoke((Action)(() =>
                    {
                        window.RequestRemote(recevied.Address);
                    }));
                }
                else if (stringData.StartsWith(RRemote))
                {
                    window.remote.Dispatcher.Invoke((Action)(() =>
                    {
                        //  window.remote.Screen._Mode = VideoPreview.Mode.InCall;
                    }));
                }
                else if (stringData.StartsWith(Mouse))
                {
                    if (window.remote != null)
                    {
                        window.remote.change = true;
                        string[] splits = stringData.Split(new String[] { Mouse, Breaker }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 3)
                        {
                            SEN_project_v2.Remote.MouseFlag = Convert.ToInt32(splits[0]);
                            
                            SEN_project_v2.Remote.mousePos.X = Convert.ToInt32(splits[1]);
                            SEN_project_v2.Remote.mousePos.Y = Convert.ToInt32(splits[2]);

                        }
                        else { }
                    }

                }
                else if (stringData.StartsWith(Keyboard))
                {
                    if (window.remote != null)
                    {
                        string[] splits = stringData.Split(new String[] { Keyboard, Breaker }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length == 2)
                        {
                            Remote.KeyStatus ks = new Remote.KeyStatus();
                            ks.code = (SEN_project_v2.Remote.Keys)Convert.ToByte(splits[0]);
                            ks.Flag = Convert.ToByte(splits[1]);
                            window.remote.waiting.Add(ks);

                        }
                        else { }
                    }
                }
                else if (stringData.StartsWith(EndRemote))
                {
                    window.remote.StopSending();

                }
                #endregion

                #region Message
                else if (stringData.StartsWith(Message))
                {
                    String[] splits = stringData.Split(new String[] { Message }, StringSplitOptions.RemoveEmptyEntries);
                    
                    receviedMessage(recevied, splits);



                }
                else if (stringData.StartsWith(RMessage))
                {
                    window.nicon.ShowBalloonTip(5, "Message was opened", UserList.Get(recevied.Address).nick, System.Windows.Forms.ToolTipIcon.Info);
                }
                #endregion

                    else if(stringData.StartsWith(MConnect))
                {
                        string[] spilts=stringData.Split(new String[]{MConnect},StringSplitOptions.RemoveEmptyEntries);
                        SendMessageTo(RMConnect, recevied.Address);
                        window.Dispatcher.BeginInvoke((Action)(() => {
                            User user=new User(recevied.Address, spilts[0]);
                            user.IsMobile = true;
                            if (UserList.Add(user))
                                window.AddUserToMobileTree(user);
                        }));

                }
                else if(stringData.StartsWith(MMessage))
                {
                    string[] splits = stringData.Split(new String[] { MMessage }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 0)
                    {


                

                        try
                        {
                            window.nicon.ShowBalloonTip(5, "Moblie Message Received In", UserList.Get(recevied.Address).nick, System.Windows.Forms.ToolTipIcon.Info);

                            UserList.conversation[recevied.Address].Dispatcher.BeginInvoke((Action)(() =>
                            {
                                UserList.conversation[recevied.Address].Redraw();
                            }));
                        }
                        catch (Exception e)
                        {

                        }

                        UserList.xml[recevied.Address].addMessage(DateTime.Now, splits[0],"Mobile");
                        UserView uv = UserList.Get(recevied.Address).userView;
                        uv.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            uv.openChat.Content = UserList.xml[recevied.Address].UnreadMessages;
                        }));


                    }
                }

                else if(stringData.StartsWith(MobileMouse))
                {
                    string[] splits = stringData.Split(new String[] { MobileMouse,Breaker }, StringSplitOptions.RemoveEmptyEntries);
          //          if(splits.Length>1)
                    SEN_project_v2.Remote.User32.SetCursorPos((int)Double.Parse(splits[0]),(int) Double.Parse(splits[1]));
                }
                else if (stringData.StartsWith(Sharing))
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "\\Sharing.xml";
                    MainWindow.tcp.SendFile(path, recevied.Address, 2);
               
                }
                else if(stringData.StartsWith(SendFile))
                {
                    string[] splits = stringData.Split(new String[] { SendFile }, StringSplitOptions.RemoveEmptyEntries);
                    MainWindow.tcp.SendFile(splits[0], recevied.Address,0);
                }
                else if(stringData.StartsWith(SendDir)){

                     string[] splits = stringData.Split(new String[] { SendDir,Breaker }, StringSplitOptions.RemoveEmptyEntries);
                
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.RestoreDirectory=true;
                    bool? result = saveFileDialog.ShowDialog();
                    saveFileDialog.Title = splits[0];

                    if (result.Value == true)
                    {
                        System.IO.File.Delete(saveFileDialog.FileName);
                        System.IO.Directory.CreateDirectory(saveFileDialog.FileName);

                        SendMessageTo(RSendDir + saveFileDialog.FileName+Breaker+ splits[0], recevied.Address);
                    }

                }
                else if (stringData.StartsWith(RSendDir))
                {
                     string[] splits = stringData.Split(new String[] { RSendDir,Breaker }, StringSplitOptions.RemoveEmptyEntries);
                    System.IO.DirectoryInfo dir=new System.IO.DirectoryInfo(splits[1]);
                    if(dir.Exists){
                        foreach(var file in dir.GetFiles())
                     MainWindow.tcp.SendFileToFolder(file.FullName, splits[0], recevied.Address, 3);
                    }
                    else
                    {
                        MessageBox.Show("Invaild Dir");
                    }
                }
                else if(stringData.StartsWith(UpdatePic))
                {  string picpath=AppDomain.CurrentDomain.BaseDirectory + "\\Pic.png";
                   MainWindow.tcp.SendFile(picpath, recevied.Address, 2);
                }
            }


        }
        private void receviedConnect(IPEndPoint recevied, String[] splits)
        {
            if (MainWindow.hostIPS.Contains(recevied.Address))
                MainWindow.hostIP = recevied.Address;
           
            SendMessageTo(RConnect + nickname + UDP.Breaker + groupname, recevied.Address);
            
          //  SendMessageTo(RConnect + Environment.MachineName + Breaker + Environment.MachineName+Breaker+recevied.Address, recevied.Address);
#if UDPConnection
            System.Diagnostics.Debug.WriteLine("-----Sending:" + RConnect + "------");
#endif

#if !Fake
            User user = new User(recevied.Address, splits[0],splits[1]);
#endif
#if Fake
                    User[] list=new User[25];

                    for (int i = 0; i < 25; i++)
                    {

                        list[i] = new User(IPAddress.Parse("127.0.0." + i), "FakeUser" + i, "FakeGroup" + i / 5);
                        User user = list[i];
                    
#endif
 #if !Fake
            if (UserList.Add(user)| UserList.Get(recevied.Address).IsOffline )
                 window.Dispatcher.Invoke((Action)(() =>
                {
                    //this.window.AddToUserList(user.CreateView(),splits[1]);
                    window.AddUserToTree(user);
                    UserList.Get(recevied.Address).IsOffline = false;
                }));
#endif

#if Fake
                        if (UserList.Add(user))window.Dispatcher.Invoke((Action)(() =>{window.AddUserToTree(user);}));
                    }
#endif
   


        }
        private void receviedRConnect(IPEndPoint recevied, String[] splits)
        {
            User user = new User(recevied.Address, splits[0],splits[ 1]);
            if(splits.Length==3)
            {
                user.hostIP = IPAddress.Parse(splits[2]);
            }

            if (UserList.Add(user)|UserList.Get(recevied.Address).IsOffline  )
                window.Dispatcher.Invoke((Action)(() =>
                {
                    

                    window.AddUserToTree(user);
                    UserList.Get(recevied.Address).IsOffline = false;
                }));

        }
        private void receviedDisconnect(IPEndPoint recevied,String[] splits)
        {
            
            User user = new User(recevied.Address,"No Information", "Others");
            if (!UserList.Add(user))
                user = UserList.Get(recevied.Address);
        
            window.Dispatcher.Invoke((Action)(() =>
            {
                window.RemoveUserFromTree(user);
                
            }));
            if(UserList.xml[recevied.Address].UnreadMessages>0)
            {
                window.Dispatcher.Invoke((Action)(() =>
                {
                    window.AddUserToOffile(user);

                }));
            }else
            UserList.Remove(recevied.Address);
        }
        private void receviedMessage(IPEndPoint recevied,String[] splits)
        {
      
          
            if (splits.Length > 0 && splits[0].Contains(Breaker))
            {
              

                    // MessageBox.Show("Message from ..." + UserList.Get(recevied.Address).nick + splits[0]);
                    window.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        (window.groupLists[UserList.Get(recevied.Address).groupName].Header as System.Windows.Controls.Grid).Background =
                            new ImageBrush(new BitmapImage(new Uri(
                "pack://application:,,,/Images/rectangle_mediumblue_154x48.png",
                    UriKind.Absolute))) { };
                        System.Diagnostics.Debug.WriteLine(window.groupLists[UserList.Get(recevied.Address).groupName]);
                    }));

                  String cate = splits[0].Substring(splits[0].IndexOf(Breaker) + 3, splits[0].Length - splits[0].IndexOf(Breaker) - Breaker.Length);
                  settings.Load(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml");
                  XmlNode node = settings.SelectSingleNode("UserProfile/BlockedList/Block_Others");
                  
                if (cate == "Others" &&  node.InnerText == "Yes")
                {
                    return;
                }
                node = settings.SelectSingleNode("UserProfile/BlockedList/Block_Games");
                  
               if (cate == "Games" && node.InnerText== "Yes")
                {
                    return;
                }
               node = settings.SelectSingleNode("UserProfile/BlockedList/Block_Study");
                  
                if (cate == "Study" && node.InnerText == "Yes")
                {
                    return;
                }
              

                    try
                    {
                        window.nicon.ShowBalloonTip(5, "Message Received From", UserList.Get(recevied.Address).nick, System.Windows.Forms.ToolTipIcon.Info);

                        UserList.conversation[recevied.Address].Dispatcher.BeginInvoke((Action)(() =>
                        {
                            UserList.conversation[recevied.Address].Redraw();
                        }));
                    }
                    catch (Exception e)
                    {

                    }

                         UserList.xml[recevied.Address].addMessage(DateTime.Now, splits[0].Substring(0, splits[0].IndexOf(Breaker)), cate);
                        UserView uv = UserList.Get(recevied.Address).userView;
                        uv.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            uv.openChat.Content = UserList.xml[recevied.Address].UnreadMessages;
                        }));
                    
                
            }
        }
        private void receviedRAudioCall(IPEndPoint recevied)
        {
            
            foreach (IPAddress ip in window.audioConf.Users)
            {
                SendMessageTo(AddMemberA + recevied.Address + AddMemberA, ip);
            }
            foreach (IPAddress ip in window.audioConf.Users)
            {
                SendMessageTo(AddMemberA + ip.ToString() + AddMemberA, recevied.Address);
            }
            window.audioConf.Users.Add(recevied.Address);
            window.audioConf.Dispatcher.Invoke((Action)(() =>
            {
                window.audioConf.vp[recevied.Address]._Mode = AudioPreview.Mode.InCall;
                if (window.audioConf.Users.Count == window.audioConf.requestedUsers.Count)
                    window.audioConf.statusLabel.Content = "Room Created Successfully...";

            }));
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