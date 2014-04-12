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
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Markup;
using System.IO;
namespace SEN_project_v2
{

    public partial class MainWindow : Window
    {
        public static  String category = "General";

        public const int REFRESH_INTERVAL = 5;
        public static UDP udp;
        Threads threads;
        static Registry reg;
        public RTPClient rtpClient;
        public VideoConf videoConf;
        public AudioConf audioConf;
        public static TCP tcp;
        private List<string> selectedFiles;
        public static List<IPAddress> hostIPS;
        public System.Windows.Forms.NotifyIcon nicon;
        public static IPAddress hostIP;
        public Window waiting;
        public Window remoteWin;
        public Remote remote;
    
        private int nomem;
        private int nosel;
        private int nogro;
        public static RadialGradientBrush brushColor;
        private int NoMembers
        {
            set
            {
             l_Members.Content = value.ToString();
             nomem = value;
            }
            get
            {

                return nomem;
            }
        }
        private int NoSelected
        {
            set
            {
             l_Selected.Content = value.ToString();
             nosel = value;
            }
            get
            {

                return nosel;
            }
        }
        private int NoGroup
        {
            set
            {
                l_Group.Content = value.ToString();
                nogro = value;
            }
            get
            {

                return nogro;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml"))
            {
                xd.Load(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml");

                String colour = xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText;
                Color color = (Color)ColorConverter.ConvertFromString(colour);
                RadialGradientBrush rgb = new RadialGradientBrush();
                rgb.RadiusY = 0.75;
                Point p = new Point(0.5, 0);
                rgb.Center = p;
                rgb.GradientStops.Add(new GradientStop(color, 1));
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFFFFFFF"), 0));
          
            brushColor = rgb;
            this.Background = rgb;
            }
            titleBar.SetWindow(this);
         //   ThemeManager.ApplyTheme(this, "BureauBlack");
            tcp = new TCP();
       //     MainWindow.icon = new NotifyIcon();
            nicon = App.nicon;
            
            nicon.Text = "OnLanChat";
            nicon.Icon = new System.Drawing.Icon("OnLanChat.ico");
            nicon.Visible = true;
            nicon.Click += nicon_Click;
            System.Windows.Forms.ContextMenu cmenu = new  System.Windows.Forms.ContextMenu();

            cmenu.MenuItems.Add("Exit");
            cmenu.MenuItems[0].Click+=(a,b)=>{
                nicon.Dispose();
                this.Close();
            };
            nicon.ContextMenu = cmenu;
            udp = new UDP((int)Ports.UDP);
            udp.SetWindow(this);
            threads = new Threads(this);
            
            groupLists = new Dictionary<string, TreeViewItem>();
            listView = new Dictionary<string, ListView>();
            _index = new Dictionary<string, Dictionary<System.Net.IPAddress,int>> ();
            
            ogroupLists = new Dictionary<string, TreeViewItem>();
            olistView = new Dictionary<string, ListView>();
            o_index = new Dictionary<string, Dictionary<System.Net.IPAddress, int>>();
            mobile_index = new Dictionary<IPAddress, int>();
           
            selectedFiles = new List<string>();
            #region hostIP init
            hostIPS = new List<IPAddress>();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {   foreach (var x in ni.GetIPProperties().UnicastAddresses)
                {
                if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        hostIPS.Add(x.Address);
                        System.Diagnostics.Debug.WriteLine(x.Address);
                    }
                }
            }

            #endregion
            reg = new Registry();
            

        }

        void nicon_Click(object sender, EventArgs e)
        {
            
            this.Show();
            this.WindowState = WindowState.Normal;

        }

        #region UI Stuffs
        
        public Dictionary<string, TreeViewItem> groupLists;
        private Dictionary<string, ListView> listView;
        private Dictionary<string, Dictionary<System.Net.IPAddress,int>> _index;
        
        private Dictionary<string, TreeViewItem> ogroupLists;
        private Dictionary<string, ListView> olistView;
        private Dictionary<string, Dictionary<System.Net.IPAddress, int>> o_index;

        private Dictionary<IPAddress, int> mobile_index;
        public static ImageBrush restoreImage;
        private TreeViewItem CreateNewGroup(string groupName,TreeView groups)
        {
            
            TreeViewItem node = new TreeViewItem();
            Dictionary<string, ListView> listViewDic;
            if(groups==Groups)
            {
                listViewDic = listView;
            }
            else
            {
                listViewDic = olistView;
            }
            
            node.Background = System.Windows.Media.Brushes.Transparent;
            node.FontSize = 16;
            node.FontWeight = FontWeights.SemiBold;
            Style focus = new Style(typeof(TreeViewItem));
            focus.Setters.Add(new Setter(ForegroundProperty, System.Windows.Media.Brushes.DarkBlue));
            
            //// Header
            node.FocusVisualStyle = focus;
            Grid g = new Grid();
            
            g.Width = 400;
            Style Headstyle = new System.Windows.Style();
            restoreImage= new ImageBrush(new BitmapImage(new Uri(
            "pack://application:,,,/Images/rectangle_blue_154x48.png", 
                UriKind.Absolute))) { Opacity = 0.60 };
            Headstyle.Setters.Add(new Setter(BackgroundProperty, restoreImage));
            g.Style = Headstyle;
            Label b = new Label() { Content = groupName, Foreground = System.Windows.Media.Brushes.White};
            b.Background = System.Windows.Media.Brushes.Transparent;
            b.MouseDown += ((sender, e) => {
                if (listViewDic[(sender as Label).Content.ToString()].SelectedItems.Count == listViewDic[(sender as Label).Content.ToString()].Items.Count)
                {
                    listViewDic[(sender as Label).Content.ToString()].UnselectAll();
                    groupLists[(sender as Label).Content.ToString()].IsExpanded = false;
                }
                else
                {
                    listViewDic[(sender as Label).Content.ToString()].SelectAll();
                    groupLists[(sender as Label).Content.ToString()].IsExpanded = true;
                }
            });
            g.Children.Add(b);

            node.Header = g;
          
            ListView userOfGroup = new ListView();
            
            userOfGroup.SelectionChanged+=userOfGroup_SelectionChanged;
            Style itemStyle = new Style(typeof(ListViewItem));
            itemStyle.Setters.Add(new Setter(BackgroundProperty,
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/rectangle_darkwhite_96x30.png", UriKind.Absolute))) { Opacity = 0.75 }));
            itemStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

            userOfGroup.ItemContainerStyle = (Style) Resources["ListViewItemStyle"];
           
            GridView grid = new GridView();

            Style style = new Style(typeof(GridViewColumnHeader));
            style.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
         
            grid.ColumnHeaderContainerStyle = style;
         
            grid.Columns.Add(new GridViewColumn() {Width=400});
           userOfGroup.HorizontalAlignment = HorizontalAlignment.Stretch;
            userOfGroup.View = grid;
            userOfGroup.BorderThickness = new Thickness(0);
            userOfGroup.Background = System.Windows.Media.Brushes.Transparent;

            listViewDic.Add(groupName, userOfGroup);
            node.Items.Add(userOfGroup);
            groups.Items.Add(node);
            if (groups == this.Groups)
            {
                _index.Add(groupName, new Dictionary<IPAddress, int>());
                NoGroup++;
            }
            else
            {
                o_index.Add(groupName, new Dictionary<IPAddress, int>());
            }

           
            return node;
        }
        private void userOfGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (UserView item in e.RemovedItems)
            {
                UserList.SelectedUsers.Remove(item.u_ip);
            }
            foreach (UserView item in e.AddedItems)
            {
                UserList.SelectedUsers.Add(item.u_ip, true);
            
            }
            NoSelected = UserList.Selected.Count;

            
        }
        public void AddUserToTree(User user)
        {
          
            if (ogroupLists.ContainsKey(user.groupName) && o_index[user.groupName].ContainsKey(user.ip))
            {
                RemoveUserFromOffline(user);
            }
           user.IsOffline = false;
            
            if (!groupLists.ContainsKey(user.groupName))
                groupLists.Add(user.groupName, CreateNewGroup(user.groupName,Groups));
            _index[user.groupName].Add(user.ip, _index[user.groupName].Keys.Count);
            
            listView[user.groupName].Items.Insert(_index[user.groupName][user.ip], user.CreateView());
           
            NoMembers++;
        }

        public void RemoveUserFromTree(User user)
        {
            try
            {
               
                listView[user.groupName].Items.RemoveAt(_index[user.groupName][user.ip]);
                _index[user.groupName].Remove(user.ip);
                if(listView[user.groupName].Items.Count==0)
                {
                    Groups.Items.Remove(groupLists[user.groupName]);
                    groupLists.Remove(user.groupName);
                    listView.Remove(user.groupName);
                    _index.Remove(user.groupName);
                    NoGroup--;
                }
                NoMembers--;
            }
            catch
            {
                UserList.UserOfGroup.Remove(user.groupName);
                AddUserToTree(user);
                 RemoveUserFromTree(user);
            }
        }
        public void AddUserToOffile(User user)
        {
            user.IsOffline = true;

            if (!ogroupLists.ContainsKey(user.groupName))
                ogroupLists.Add(user.groupName, CreateNewGroup(user.groupName, OfflineGroups));
            o_index[user.groupName].Add(user.ip, o_index[user.groupName].Keys.Count);
            olistView[user.groupName].Items.Insert(o_index[user.groupName][user.ip], user.CreateView());
        }
        public void RemoveUserFromOffline(User user)
        {
            
            try
            {
                olistView[user.groupName].Items.RemoveAt(o_index[user.groupName][user.ip]);
                o_index[user.groupName].Remove(user.ip);
                if (olistView[user.groupName].Items.Count == 0)
                {
                    OfflineGroups.Items.Remove(ogroupLists[user.groupName]);
                    ogroupLists.Remove(user.groupName);
                    olistView.Remove(user.groupName);
                    o_index.Remove(user.groupName);
                }
            }
            catch(Exception e) {
                MessageBox.Show("RemoveUserFromOffine" + e.Message);
            }
       
        }
        public void AddUserToMobileTree(User user)
        {
           if(!mobile_index.ContainsKey(user.ip))
           {
               Mobile.Items.Insert(mobile_index.Count, user.CreateView());
           }
          
         
        }

        #endregion
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
   
            threads.StartAll();

        }
        private void Button_Click(object sender, RoutedEventArgs e){

            this.WindowState = WindowState.Minimized;

        }
        public class Threads
        {
            public static Thread broadcast;
            public static Thread udpReceving;
            public static Thread tcpReceving;
            public static Thread fileSending;
            public static Thread fileReceving;
            public static Thread rtpReceving;
            public static MainWindow w;
            public Threads(MainWindow window)
            {
                w = window;
                broadcast = new Thread(new ThreadStart(broadcast_proc));
                udpReceving = new Thread(new ThreadStart(udp.recevingThread));
                udpReceving.SetApartmentState(ApartmentState.STA);
    //            rtpReceving = window.rtpClient.listen_thread;
                udpReceving.SetApartmentState(ApartmentState.STA);
            }

            private void broadcast_proc()
            {
                while (true)
                {
                    BroadCasting.Do();
                    Thread.Sleep(REFRESH_INTERVAL*1000);
                  
                }
            }
            public void StartAll()
            {
                udpReceving.Start();
                broadcast.Start();

            }
            public void StopAll()
            {
                StopThread(broadcast);
                
                BroadCasting.Disconnect();
                if(w.rtpClient!=null)
                w.rtpClient.Dispose();
                StopThread(udpReceving);
                StopThread(tcpReceving);
                StopThread(fileSending);
                StopThread(fileReceving);
                StopThread(rtpReceving);
            }

            public void StopThread(Thread thread)
            {
                if (thread != null && thread.IsAlive)
                    thread.Abort();
            }
        }
        public static class BroadCasting
        {
            public static IPEndPoint SEND = new IPEndPoint(IPAddress.Parse("255.255.255.255"), (int)Ports.UDP);
            public static IPEndPoint RECEIVE = new IPEndPoint(IPAddress.Any, (int)Ports.UDP);
           
            public static void Do()
            {
                string nickname, groupname;
                if (File.Exists("UserSettings.xml") == true)
                {
                    System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                    xd.Load("UserSettings.xml");
                    nickname = xd.SelectSingleNode("UserProfile/General/NickName").InnerText;
                    groupname = xd.SelectSingleNode("UserProfile/General/GroupName").InnerText;
                    if(nickname==""  || nickname==null)
                    {
                        nickname = Environment.MachineName;
                    }
                }
                else
                {
                    nickname = Environment.MachineName;
                    groupname = Environment.MachineName;
                }
                udp.SendMessageTo(UDP.Connect + nickname + UDP.Breaker + groupname, BroadCasting.SEND.Address);
                    foreach (IPAddress ip in reg.Read())
                    {
                        udp.SendMessageTo(UDP.Connect + nickname + UDP.Breaker + groupname, ip);
                    }
                
            }
            public static void Disconnect()
            {
                udp.SendMessageTo(UDP.Disconnect + Environment.MachineName, BroadCasting.SEND.Address);
                foreach (IPAddress ip in reg.Read())
                {
                    udp.SendMessageTo(UDP.Disconnect + Environment.MachineName + UDP.Breaker + Environment.MachineName, ip);
                }
            }

        }
        public enum Ports : int
        {
            UDP = 1716,
            TCP = 12316,
            RTP = 56789,

        }

        public class Registry
        {
            public List<IPAddress> Read()
            {
                try{
                RegistryKey rkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE", true);

                rkey=rkey.CreateSubKey("OnLanChat");
                rkey = rkey.CreateSubKey("BroadCast");

                return rkey.GetValueNames().Select(x => IPAddress.Parse(rkey.GetValue(x).ToString())).ToList();
                    
        
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                    return null;
                }
           //     System.Diagnostics.Debug.WriteLine ( string.Join(" ", subKey.GetValueNames()) );
            }

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (remote != null)
            {
                remote.StartSending();
                remote.Close();
            }
            threads.StopAll();
            tcp.Stop();
            udp.recevingClient.Close();
            if(snippingWindow!=null)
            snippingWindow.Close();
        }
        private void VideoConfB_Click(object sender, RoutedEventArgs e)
        {
            
            videoConf = new VideoConf(this,hostIP);
            
            videoConf.IsHost = true;
            videoConf.AddMember.IsEnabled = true;
            if (videoConf.SetVideoSources())
            {
                videoConf.Show();//  CreateVideoConf(null);
                videoConf.statusLabel.Content = "Waiting For Users's Responses...";
                //videoConf.requestedUsers.Clear();
                //foreach(string group in groupLists.Keys)
                //{
                //    foreach (UserView uv in listView[group].SelectedItems)
                //        videoConf.requestedUsers.Add(uv.u_ip);
                //}
                foreach (IPAddress ip in videoConf.requestedUsers)
                {
                    //    videoConf.vp.Add(ip, new VideoPreview(VideoPreview.Mode.Watting, null) { Nick = UserList.Get(ip).nick });
                    //  videoConf._stack.Children.Add(videoConf.vp[ip]);
                    videoConf.MakeUserPreview(ip, VideoPreview.Mode.Watting);

                }
                videoConf.Start();
            }
        }
        public void CreateVideoConf(IPAddress host)
        {
        //    videoConf = new VideoConf(udp, host);

//            videoConf.Show();

            waiting = new Window();
            waiting.BorderThickness = new Thickness(0, 0, 0, 0);
            waiting.AllowsTransparency = true;
            waiting.Topmost = true;
            waiting.HorizontalAlignment = HorizontalAlignment.Center;
            waiting.VerticalAlignment = VerticalAlignment.Center;
            
            VideoPreview vp = new VideoPreview(VideoPreview.Mode.Request,host);
            vp.Nick = UserList.Get(host).nick;
            vp.window = this;
            waiting.Content = vp;
            waiting.Width = 250; waiting.Height=250;
            waiting.WindowStyle = WindowStyle.None;
            vp.udp = udp;
            waiting.Show();

        }
        private void audioConfB_Click(object sender, RoutedEventArgs e)
        {
            audioConf = new AudioConf(this, hostIP);
            audioConf.IsHost = true;
            audioConf.AddMember.IsEnabled = true ;
            audioConf.Show();
            audioConf.statusLabel.Content = "Waiting For Users's Responses...";
            foreach (IPAddress ip in audioConf.requestedUsers)
            {
                audioConf.MakeUserPreview(ip, AudioPreview.Mode.Watting);
            }
            audioConf.Start();
            
        }
        public void CreateAudioConf(IPAddress host)
        {
            waiting = new Window();
            waiting.BorderThickness = new Thickness(0, 0, 0, 0);
            waiting.AllowsTransparency = true;
            waiting.Topmost = true;
            waiting.HorizontalAlignment = HorizontalAlignment.Center;
            waiting.VerticalAlignment = VerticalAlignment.Center;

            AudioPreview vp = new AudioPreview(AudioPreview.Mode.Request, host);
            vp.Nick = UserList.Get(host).nick;
            vp.window = this;
            waiting.Content = vp;
            waiting.Width = 250; waiting.Height = 250;
            waiting.WindowStyle = WindowStyle.None;
            vp.udp = udp;
            waiting.Show();

        }
        private void Remote_Click(object sender, RoutedEventArgs e)
        {

            if (Remote.Content.Equals("Stop Remote"))
            {
                remote.StopSending();
                if(remote!=null)
                remote.rtpClient.Dispose();
                Remote.Content = "Remote";
                remote.Close();
            }
            else
            {
                
                if (UserList.Selected.Count == 1)
                {
                    if (!MainWindow.hostIPS.Contains(UserList.Selected.First()))
                    {
                        remote = new Remote(this, UserList.Selected.First());
                        remote.Show();
                        remote.Start();
                        Remote.IsEnabled = false;
                    }
                    else
                        MessageBox.Show("You can't remotly access you..!");
                }
                else
                    MessageBox.Show("Select Excatly One User...");
            }
        }
        public void RequestRemote(IPAddress host)
        {
            //    videoConf = new VideoConf(udp, host);

            //            videoConf.Show();

            remoteWin = new Window();
            remoteWin.BorderThickness = new Thickness(0, 0, 0, 0);
            //remoteWin.AllowsTransparency = true;
            remoteWin.Topmost = true;
            remoteWin.HorizontalAlignment = HorizontalAlignment.Center;
            remoteWin.VerticalAlignment = VerticalAlignment.Center;
            remoteWin.Width = 250; remoteWin.Height = 250;


            remoteWin.Title ="Remote Request From"+host.ToString();
            remoteWin.Show();
            VideoPreview vp = new VideoPreview(VideoPreview.Mode.Request, host,true);
            vp.Height = 250; vp.Width = 250;
            vp.Nick = UserList.Get(host).nick;
            vp.window = this;
            remoteWin.SizeToContent = SizeToContent.WidthAndHeight;
            remoteWin.Content = vp;
     

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Settings setting = new Settings(this);
            setting.Show();
        }


        #region FileSending
        private void filesButton_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.All;
        }

        private void filesButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                selectedFiles = files.ToList();
                if (files.Length == 1)
                    (sender as Button).Content = files[0];
                else
                    (sender as Button).Content = "(" + files.Length + ") Files Added...! Click For Clear";

            }
        }
        #endregion

        private void filesButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFiles.Count == 0)
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = "Select Any Files";

                dlg.Multiselect = true;
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {

                    string[] files = (string[])dlg.FileNames;
                    selectedFiles = files.ToList();
                    if (files.Length == 1)
                        (sender as Button).Content = files[0];
                    else
                        (sender as Button).Content = "(" + files.Length + ") Files Added...! Click Again For Clear";

                }
            }
            else
            {
                (sender as Button).Content = "<< Drag Files Here >>";
                selectedFiles.Clear();
            }
        }
        private void SendB_Click(object sender, RoutedEventArgs e)
        {
     


            foreach (IPAddress ip in UserList.Selected)

            {

                MemoryStream stream = new MemoryStream();
    
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stream))
                {
                    XamlWriter.Save(TransformImages(sendBox.Document,ip), writer);
                }
                
                Byte[] Messeage = stream.GetBuffer().Skip(3).ToArray();
              
                UserList.xml[ip].addSelfMessage(DateTime.Now, Encoding.ASCII.GetString(Messeage),category);
                udp.SendMessageTo(Encoding.ASCII.GetBytes(UDP.Message).Concat(Messeage) .ToArray(), ip,category);
                 stream.Close();
                
            }
       
            if(selectedFiles.Count>0)
            {
              
                tcp.SendFiles(selectedFiles,UserList.Selected);
                
            }
            
           
       
        }

        public  FlowDocument TransformImages(FlowDocument flowDocument,IPAddress ip)
        {
            FlowDocument img_flowDocument = flowDocument;
            Type inlineType;
            InlineUIContainer uic;
            System.Windows.Controls.Image replacementImage;
            List<string> files = new List<string>();
            int count = 0;
            int index = UserList.xml[ip].CountMessages;
            foreach (Block b in flowDocument.Blocks)
            {
                if (b is Paragraph)
                {
                    foreach (Inline i in ((Paragraph)b).Inlines)
                    {

                        inlineType = i.GetType();
                        if (inlineType == typeof(InlineUIContainer))
                        {
                            uic = ((InlineUIContainer)i);


                            if (uic.Child.GetType() == typeof(System.Windows.Controls.Image))
                            {
                                replacementImage = (System.Windows.Controls.Image)uic.Child;
                                 count=setImages(files, ip, count, index, replacementImage);


                            }
                        }
                    }
                }
                else
                {
                   
                }
            }
            List<IPAddress> ips= new List<IPAddress>();
            ips.Add(ip);
            foreach(IPAddress  ipa in ips)
            foreach(string  file in files)
            {
                tcp.SendFile(file, ipa, 1);
            }
            return img_flowDocument;
        }
        private int  setImages(List<string> files, IPAddress ip, int count,int index, Image replacementImage)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)replacementImage.Source));
            string Path = AppDomain.CurrentDomain.BaseDirectory + "\\" + ip.ToString().Replace('.', '\\') + "\\" + index + "." + count + ".jpg";
            using (FileStream stream = File.Create(Path))
            {
                encoder.Save(stream);
                
                stream.Flush();
                stream.Close();
                
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri(Path, UriKind.Absolute));
            replacementImage.Source = bitmapImage;
            replacementImage.Height = bitmapImage.Height;
            replacementImage.Width = bitmapImage.Width;


            files.Add(Path);
            count++;
            return count;
        }

        private void sendBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

       

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content.Equals("SelectAll"))
            {
                foreach (ListView lv in listView.Values)
                    lv.SelectAll();
                (sender as Button).Content = "DeselectAll";
            }
            else
            {
                foreach (ListView lv in listView.Values)
                    lv.UnselectAll();
                (sender as Button).Content = "SelectAll";
            }
             

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            NoMembers = 0;
            NoGroup = 0;
            NoSelected = 0;
            groupLists.Clear();
            listView.Clear();
            _index.Clear();
            selectedFiles.Clear();
            
            hostIPS.Clear();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                foreach (var x in ni.GetIPProperties().UnicastAddresses)
                {
                    if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        hostIPS.Add(x.Address);
                        System.Diagnostics.Debug.WriteLine(x.Address);
                    }
                }
            UserList.ClearAllList();
            Groups.Items.Clear();
            BroadCasting.Do();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }
        Snipping snippingWindow;
        private IAsyncResult asynC;
        private void Snipping_Click(object sender, RoutedEventArgs e)
        {
            snippingWindow = new Snipping();
            snippingWindow.Show();
        }

        private void sendBox_GotFocus(object sender, RoutedEventArgs e)
        {
            sendBox.SelectAll();
        }

        private void sendBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            sendBox.SelectAll();
        }
        ///////////////////////////////////////
        private void categoryGames_Checked(object sender, RoutedEventArgs e)
        {
            // Handler for 'Games' radiobutton
            change_category("Games");
        }

        private void categoryStudy_Checked(object sender, RoutedEventArgs e)
        {
            // Handler for 'Study' radiobutton
            change_category("Study");
        }

        private void categoryOthers_Checked(object sender, RoutedEventArgs e)
        {
            // Handler for 'Others' radiobutton
            change_category("Others");
        }

        public void change_category(String c)
        {
            // Setting the radiobuttons of categories
            category = c;
            if (c == "Games")
                categoryGames.IsChecked = true;
            else if (c == "Study")
                categoryStudy.IsChecked = true;
            else if (c == "Others")
                categoryOthers.IsChecked = true;
        }









    }
}