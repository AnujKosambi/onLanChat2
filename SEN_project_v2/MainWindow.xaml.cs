﻿using System;
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
//using WPF.Themes;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static UDP udp;
        Threads threads;
        
        public RTPClient rtpClient;
        public VideoConf videoConf;
        private List<string> selectedFiles;
        public static List<IPAddress> hostIPS;

        public static IPAddress hostIP;
        public Window waiting;
        public Window remoteWin;
        public Remote remote;
        
        public MainWindow()
        {
            InitializeComponent();
         //   ThemeManager.ApplyTheme(this, "BureauBlack");


            udp = new UDP((int)Ports.UDP);
            udp.SetWindow(this);
            
            threads = new Threads(this);
            indexer = new Dictionary<IPAddress, int>();
            groupLists = new Dictionary<string, TreeViewItem>();
            listView = new Dictionary<string, ListView>();
            _index = new Dictionary<string, Dictionary<System.Net.IPAddress,int>> ();
            selectedFiles = new List<string>();
            #region hostIP init
            hostIPS = new List<IPAddress>();
            foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
               
                foreach (var x in ni.GetIPProperties().UnicastAddresses)
                {


                    if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        hostIPS.Add(x.Address);
                        System.Diagnostics.Debug.WriteLine(x.Address);
                    }
                }
            }
            #endregion
        }

        #region UI Stuffs
        private Dictionary<IPAddress, int> indexer;
        private Dictionary<string, TreeViewItem> groupLists;
        private Dictionary<string, ListView> listView;
        private Dictionary<string, Dictionary<System.Net.IPAddress,int>> _index;
        private TreeViewItem CreateNewGroup(string groupName)
        {
            TreeViewItem node = new TreeViewItem();

            node.Background = System.Windows.Media.Brushes.Transparent;
            node.FontSize = 16;
            node.FontWeight = FontWeights.SemiBold;
            Style focus = new Style(typeof(TreeViewItem));
            focus.Setters.Add(new Setter(ForegroundProperty, System.Windows.Media.Brushes.DarkBlue));

            node.FocusVisualStyle = focus;
            node.Header = groupName;
            ListView userOfGroup = new ListView();
            
            Style itemStyle = new Style(typeof(ListViewItem));
            itemStyle.Setters.Add(new Setter(BackgroundProperty, new ImageBrush(new BitmapImage(new Uri("rectangle_darkwhite_96x30.png", UriKind.Relative))) { Opacity=20}));
            itemStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));

            userOfGroup.ItemContainerStyle = itemStyle;
           
            GridView grid = new GridView();
            
            Style style = new Style(typeof(GridViewColumnHeader));
            style.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
            style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
            grid.ColumnHeaderContainerStyle = style;
            
            grid.Columns.Add(new GridViewColumn() {Width=Groups.RenderSize.Width-40});
            userOfGroup.HorizontalAlignment = HorizontalAlignment.Stretch;
            userOfGroup.View = grid;
            userOfGroup.BorderThickness = new Thickness(0);
            userOfGroup.Background = System.Windows.Media.Brushes.Transparent;

            listView.Add(groupName, userOfGroup);
            node.Items.Add(userOfGroup);
            Groups.Items.Add(node);
            _index.Add(groupName, new Dictionary<IPAddress, int>());
         
       
            return node;
        }
        public void AddUserToTree(User user)
        {
            if (!groupLists.ContainsKey(user.groupName))
                groupLists.Add(user.groupName, CreateNewGroup(user.groupName));
            _index[user.groupName].Add(user.ip, _index[user.groupName].Keys.Count);
            listView[user.groupName].Items.Insert(_index[user.groupName][user.ip], user.CreateView());
        
       

  
        }
           
        public void RemoveUserFromTree(User user)
        {
            try
            {

                listView[user.groupName].Items.RemoveAt(_index[user.groupName][user.ip]);
                _index[user.groupName].Remove(user.ip);
            }
            catch
            {
                AddUserToTree(user);
                 RemoveUserFromTree(user);
            }
        }
      
        #endregion
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
   
            threads.StartAll();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

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
                    udp.SendMessageTo(UDP.Connect + Environment.MachineName+UDP.Breaker+Environment.MachineName, BroadCasting.SEND.Address);
                    Thread.Sleep(5000);
                  
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
                udp.SendMessageTo(UDP.Disconnect+Environment.MachineName, BroadCasting.SEND.Address);
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
        public struct BroadCasting
        {
            public static IPEndPoint SEND = new IPEndPoint(IPAddress.Parse("255.255.255.255"), (int)Ports.UDP);
            public static IPEndPoint RECEIVE = new IPEndPoint(IPAddress.Any, (int)Ports.UDP);

        }
        public enum Ports : int
        {
            UDP = 1716,
            TCP = 12316,
            RTP = 56789,

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (remote != null)
            {
                remote.StartSending();
                remote.Close();
            }
            threads.StopAll();

            udp.recevingClient.Close();
        }
        private void VideoConfB_Click(object sender, RoutedEventArgs e)
        {
            
            videoConf = new VideoConf(this,hostIP);
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
                if (UserList.Selected.Count > 0)
                {
                    remote = new Remote(this, UserList.Selected.First());
                    remote.Show();
                    remote.Start();
                }
                else
                    MessageBox.Show("Select At least One User...");
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
            Settings setting = new Settings();
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
            //foreach(string group in groupLists.Keys)
            //foreach(var sel in listview[group].selecteditems)
            //{
            foreach (IPAddress ip in UserList.Selected)
            {

                string Messeage =new TextRange(sendBox.Document.ContentStart, sendBox.Document.ContentEnd).Text;
                UserList.xml[ip].addSelfMessage(DateTime.Now, Messeage);
                udp.SendMessageTo(UDP.Message + Messeage + UDP.Message, ip);
            }
            //}
            
           
       
        }

        private void sendBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

        private void sendBox_GotFocus(object sender, RoutedEventArgs e)
        {
           // sendBox.Selection.Select(sendBox.Document.ContentStart, sendBox.Document.ContentEnd);

        }

   

    
    



    }
}