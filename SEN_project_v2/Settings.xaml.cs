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
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Threading;
//using System.Windows.Forms;
using System.Windows.Controls;
using AForge.Video.DirectShow;
using AForge.Video;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Management;
namespace SEN_project_v2
{

    public partial class Settings : Window
    {
        public static Dictionary<string, List<string>> childs;
       // public static Dictionary<CheckBox, string> sharedFiles;
        public static Dictionary<string, string> VirtualName;
        public XmlDocument sharingDoc;
        SecurityPW security = null;
        public static Color backcolor;
        string mainPath = AppDomain.CurrentDomain.BaseDirectory+"\\";
        MainWindow mw;
        public Settings(MainWindow mw)
        {
            this.mw = mw;
            mw.Settings.IsEnabled = false;
            InitializeComponent();
           // sharedFiles = new Dictionary<CheckBox,string>();
            VirtualName = new Dictionary<string, string>();
            childs = new Dictionary<string, List<string>>();
            loadUser();
            if (File.Exists(mainPath+"UserSettings.xml") == true)
            {
                String key = NickName.Text;
                XmlDocument settings = new XmlDocument();
                settings.Load(mainPath+"UserSettings.xml");
                NickName.Text = settings.SelectSingleNode("UserProfile/General/NickName").InnerText;
                GroupName.Text = settings.SelectSingleNode("UserProfile/General/GroupName").InnerText;
                if (settings.SelectSingleNode("UserProfile/General/passwordenabled").InnerText == "yes")
                {
                    PasswordCheck.IsChecked = true;
                    ChangePassword.IsEnabled = true;
                    security = new SecurityPW();
                    PasswordBox.Password = security.Decryptstring(settings.SelectSingleNode("UserProfile/General/Password").InnerText);
                    PasswordBox.IsEnabled = false;
                }
                else
                {
                    PasswordCheck.IsChecked = false;
                    ChangePassword.IsEnabled = false;
                    PasswordBox.IsEnabled = false;
                }
                Camera.Text = settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText;
                Microphone.Text = settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText;
              
                pic_path.Text = settings.SelectSingleNode("UserProfile/Appearance/ProfilepicPath").InnerText;
                if (pic_path.Text != null & pic_path.Text!="")
                {
                    try
                    {
                        Uri uri = new Uri(@pic_path.Text, UriKind.Absolute);
                        ImageSource imgSource = new BitmapImage(uri);
                        Profile_pic.Source = imgSource;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                else
                {
                    string strUri2 = "pack://application:,,,/Images/user-icon.png";
                    Profile_pic.Source = new BitmapImage(new Uri(strUri2));

                }
                String colour = settings.SelectSingleNode("UserProfile/Appearance/Colour").InnerText;
                Color color = (Color)ColorConverter.ConvertFromString(colour);
                ColorPicker.SelectedColor = color;
                BrushConverter bc = new BrushConverter();
                this.Background = (Brush)bc.ConvertFrom(colour);
                RadialGradientBrush rgb = new RadialGradientBrush();
                rgb.RadiusY = 0.75;
                Point p = new Point(0.5, 0);
                rgb.Center = p;
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colour), 1));
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFFFFFFF"), 0));
                this.Background = rgb;


            }
            else
            {
                NickName.Text = "";
                GroupName.Text = "";
                PasswordCheck.IsChecked = false;
                PasswordBox.IsEnabled = false;
                ChangePassword.IsEnabled = false;

                Camera.Text = "";
                Microphone.Text = "";
                Recording.IsChecked = true;
                AudioSave_path.Text = "";

                ColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString("#FF6E6969");

                   string strUri2 = "pack://application:,,,/Images/user-icon.png";
                    Profile_pic.Source = new BitmapImage(new Uri(strUri2));
                
                RadialGradientBrush rgb = new RadialGradientBrush();
                rgb.RadiusY = 0.75;
                Point p = new Point(0.5, 0);
                rgb.Center = p;
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BF919191"), 1));
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFFFFFFF"), 0));
                this.Background = rgb;
            }
            FilterInfoCollection cam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            {
                foreach (FilterInfo info in cam)
                {
                    Camera.Items.Add(info.Name);

                }
            }
            Audio aw = new Audio();
            foreach (var i in aw.sources)
            {
                Microphone.Items.Add(i.ProductName);
            }
            foreach (var i in UserList.GroupList.Values.Distinct())
            {
                GroupName.Items.Add(i);

            }
            if(File.Exists("Sharing.xml"))
            {
                loadSharedFolders();
            }
        }

        private void loadSharedFolders()
        {
            if (sharingDoc == null)
            {
                sharingDoc = new XmlDocument();
                sharingDoc.Load("Sharing.xml");
            }
             foreach(XmlNode folder in  GetFolderRoot().SelectNodes("Folder"))
             {

                 VirtualName.Add(folder.Attributes.GetNamedItem("Path").Value, folder.Attributes.GetNamedItem("name").Value);
                 loadList(folder);
                 
             }
        }

        private void loadList(XmlNode node)
        {
            DirectoryInfo info=new DirectoryInfo(node.Attributes.GetNamedItem("Path").Value);
            if(!childs.ContainsKey(info.FullName))
            childs.Add(info.FullName, new List<string>());
            if(childs.ContainsKey(info.Parent.FullName))
                childs[info.Parent.FullName].Add(info.FullName);
            foreach(XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Folder")
                {
                    loadList(child);
                }

            }

        }
        private void loadUser()
        {
            foreach (IPAddress ip in UserList.All)
            {
                CompleteList.Items.Add(ip);
            }
        }
        private void BlockedUsersList_Expanded(object sender, RoutedEventArgs e)
        {
            if (Conference.IsExpanded == true)
            {
                Conference.IsExpanded = false;
            }
        }

        public string storedpassword;
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(mainPath+"UserSettings.xml") == false)
            {
                XmlWriter xw = XmlWriter.Create(mainPath+"UserSettings.xml");
                #region XML Elements creation
                xw.WriteStartElement("UserProfile");

                xw.WriteStartElement("General");
                xw.WriteStartElement("NickName");
                xw.WriteEndElement();
                xw.WriteStartElement("GroupName");
                xw.WriteEndElement();
                xw.WriteStartElement("passwordenabled");
                xw.WriteEndElement();
                xw.WriteStartElement("Password");
                xw.WriteEndElement();
                xw.WriteEndElement();

                xw.WriteStartElement("Appearance");
                xw.WriteStartElement("ProfilepicPath");
                xw.WriteEndElement();
                xw.WriteStartElement("Colour");
                xw.WriteEndElement();
                xw.WriteEndElement();

                xw.WriteStartElement("Conference");
                xw.WriteStartElement("Camera");
                xw.WriteEndElement();
                xw.WriteStartElement("Microphone");
                xw.WriteEndElement();
                xw.WriteStartElement("Recordingenabled");
                xw.WriteEndElement();
                xw.WriteStartElement("Audiosavepath");
                xw.WriteEndElement();
                xw.WriteEndElement();

                xw.WriteStartElement("BlockedList");
                xw.WriteStartElement("Send_Games");
                xw.WriteEndElement();
                xw.WriteStartElement("Send_Study");
                xw.WriteEndElement();
                xw.WriteStartElement("Send_Others");
                xw.WriteEndElement();
                xw.WriteStartElement("Receive_Games");
                xw.WriteEndElement();
                xw.WriteStartElement("Receive_Study");
                xw.WriteEndElement();
                xw.WriteStartElement("Receive_Others");
                xw.WriteEndElement();
                xw.WriteEndElement();

                xw.WriteStartElement("FileSharing");
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();
                #endregion
                Write_to_XML();
            }
            else
            {
                Write_to_XML();
            }
            if (PasswordCheck.IsChecked == true)
            {
                if (PasswordBox.Password == "")
                {
                    System.Windows.MessageBox.Show("Please put a password or disable it");
                }
                else
                {
                    PasswordBox.IsEnabled = false;
                }
            }

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Apply_Click(null, null);
            this.Close();
        }

        private void Write_to_XML()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(mainPath+"UserSettings.xml");
            #region General
            xd.SelectSingleNode("UserProfile/General/NickName").InnerText = NickName.Text;
            xd.SelectSingleNode("UserProfile/General/GroupName").InnerText = GroupName.Text;
            if (PasswordCheck.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/General/passwordenabled").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/General/passwordenabled").InnerText = "no";
            }
            string key = NickName.Text;
            //security = new SecurityPW();
            //storedpassword = security.Encryptstring(PasswordBox.Password);
            xd.SelectSingleNode("UserProfile/General/Password").InnerText = PasswordBox.Password;
            #endregion
            #region Confernce
            xd.SelectSingleNode("UserProfile/Conference/Camera").InnerText = Camera.Text;
            xd.SelectSingleNode("UserProfile/Conference/Microphone").InnerText = Microphone.Text;
            if (Recording.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/Conference/Recordingenabled").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/Conference/Recordingenabled").InnerText = "no";

            }
            xd.SelectSingleNode("UserProfile/Conference/Audiosavepath").InnerText = AudioSave_path.Text;
            #endregion
            #region Appearance
            xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText = ColorPicker.SelectedColor.ToString();
            String colour = xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText;
            Color color = (Color)ColorConverter.ConvertFromString(colour);
            ColorPicker.SelectedColor = color;
            RadialGradientBrush rgb = new RadialGradientBrush();
            rgb.RadiusY = 0.75;
            Point p = new Point(0.5, 0);
            rgb.Center = p;
            rgb.GradientStops.Add(new GradientStop(color, 1));
            rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFFFFFFF"), 0));
            this.Background = rgb;
            mw.Background = rgb;
            //System.Diagnostics.Debug.WriteLine(pic_path.Text);
            xd.SelectSingleNode("UserProfile/Appearance/ProfilepicPath").InnerText = pic_path.Text;
            

            System.Diagnostics.Debug.WriteLine(xd.SelectSingleNode("UserProfile/Appearance/ProfilepicPath"));
            #endregion
            #region Blocking Category
            if (Send_Games.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Games").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Games").InnerText = "no";

            }
            if (Send_Study.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Study").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Study").InnerText = "no";

            }
            if (Send_others.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Others").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Send_Others").InnerText = "no";

            }
            if (Receive_Games.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Games").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Games").InnerText = "no";

            }
            if (Receive_Study.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Study").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Study").InnerText = "no";

            }
            if (Receive_others.IsChecked == true)
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Others").InnerText = "yes";
            }
            else
            {
                xd.SelectSingleNode("UserProfile/BlockedList/Receive_Others").InnerText = "no";

            }
            #endregion
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml";
            xd.Save(path);
            //UserList.Get(MainWindow.hostIP).userView.u_nick = NickName.Text;
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            //new ChangePassword().Show();
        }

        private void PasswordCheck_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.IsEnabled = true;
            ChangePassword.IsEnabled = true;
        }

        private void PasswordCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = "";
            PasswordBox.IsEnabled = false;
            ChangePassword.IsEnabled = false;
        }

        private void Recording_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Recording_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FileSharing_Expanded(object sender, RoutedEventArgs e)
        {
            General.IsExpanded = false;
            Appearance.IsExpanded = false;
            Conference.IsExpanded = false;
            BlockedUsersList.IsExpanded = false;
            ListDirectory(SharedFiles);

        }
        /// <summary>
        /// ANUJ
        /// </summary>
        /// <param name="treeview"></param>
        /// <param name="path"></param>

        protected ManagementObjectCollection getDrives()
        {
            //get drive collection 
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * From Win32_LogicalDisk ");
            ManagementObjectCollection queryCollection = query.Get();
            return queryCollection;
        } 
        private void ListDirectory(System.Windows.Controls.TreeView treeview)
        {
            treeview.Items.Clear();
            
          //  TreeViewItem tvi=CreateDirectoryNode(rootDirectoryInfo,false);
           // treeview.Items.Add(tvi);
            ManagementObjectCollection queryCollection = getDrives(); 
             foreach ( ManagementObject mo in queryCollection)
             {
                 DirectoryInfo rootDirectoryInfo = new DirectoryInfo(mo["name"].ToString());
                  TreeViewItem tvi=CreateDirectoryNode(rootDirectoryInfo.Root,false);
                  treeview.Items.Add(tvi);
             }
            
        }
        
        private  TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo,bool isSelected)
        {
            int checkIndex = 1;
         
            TreeViewItem directoryNode1 = new System.Windows.Controls.TreeViewItem();
            directoryNode1.Items.Add(new TreeViewItem() { Height=0});
            
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            CheckBox cb = new CheckBox();
            cb.Margin = new Thickness(5, 0, 0, 0);
            cb.Content = directoryInfo.Name;
            directoryNode1.Header = sp;
           
            sp.Children.Add(new Image() { Source = BitmapFrame.Create(new Uri("pack://application:,,,/Images/FolderIcon.png")) ,Width=16,Height=16});
            sp.Children.Add(cb);
            cb.Checked += (sender, e) =>{
#region Check
                                             string vname = "";
                bool ok = false;
               // if(!VirtualName.ContainsKey(directoryInfo.FullName))
                    //ERROR 
                if (directoryInfo.FullName.Equals(directoryInfo.Root.FullName)||
                    ((((directoryNode1.Parent as TreeViewItem).Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked.Value == false))
                {
                    VirtualDirectory vd = new VirtualDirectory(directoryNode1.Name, ref vname, ref ok);

                    while (vd.ShowDialog().Value)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    System.Diagnostics.Debug.WriteLine(vd.ok + " " + vd.vname);
                    if (vd.ok == false)
                    {
                        (sender as CheckBox).IsChecked = false;
                    }
                    if (vd.ok == true)
                    {
                        if (VirtualName.ContainsKey(directoryInfo.FullName))
                            VirtualName.Remove(directoryInfo.Name);

                            VirtualName.Add(directoryInfo.FullName, vd.vname);
                            if (childs.ContainsKey(directoryInfo.FullName))
                        childs.Remove(directoryInfo.FullName);
                            childs.Add(directoryInfo.FullName, new List<string>());

                        
                   
                    }
                }
                else
                {
                    childs[(string)directoryInfo.Parent.FullName].Add(directoryInfo.FullName);
                    childs.Add(directoryInfo.FullName, new List<string>());
                    System.Diagnostics.Debug.WriteLine(string.Join(":", childs[(string)directoryInfo.Parent.FullName].ToArray()));
                    //foreach (TreeViewItem item in directoryNode1.Items)
                    //{
                    //    if (item.HasHeader)
                    //    {
                    //        ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                    //    }
                    //}
                }
#endregion 
                                         };
           
            
            cb.Unchecked += (sender, e) =>{
#region Uncheck

             foreach (TreeViewItem item in directoryNode1.Items)
            {
                if (item.HasHeader)
                {
                    ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                }
            }
             if (!directoryInfo.FullName.Equals(directoryInfo.Root.FullName))
             {
                 if (childs.ContainsKey((string)directoryInfo.Parent.FullName))
                 {
                     childs[(string)directoryInfo.Parent.FullName].Remove(directoryInfo.FullName);
                     childs.Remove(directoryInfo.FullName);
                     System.Diagnostics.Debug.WriteLine(string.Join(":", childs[(string)directoryInfo.Parent.FullName].ToArray()));
                 }
                 else
                 {
                     childs.Remove(directoryInfo.FullName);
                     VirtualName.Remove(directoryInfo.FullName);

                 }
             }
             else
             {
                 childs.Remove(directoryInfo.FullName);
                 VirtualName.Remove(directoryInfo.FullName);
             }
#endregion 
            };
   #region addingFiles
            directoryNode1.Expanded += (sender, e) =>
            {
         
                if (directoryNode1.Items.Count <= 1)
                {
                    
                    try
                    {
                        foreach (var directory in directoryInfo.GetDirectories())
                        {
                            try
                            {
                                int index = directoryNode1.Items.Count;

                                directoryNode1.Items.Insert(index, CreateDirectoryNode(directory, cb.IsChecked.Value));
                                (((directoryNode1.Items[index] as TreeViewItem).Header as StackPanel).Children[checkIndex] as CheckBox)
                                    .IsChecked = childs.ContainsKey(directory.FullName) | isSelected;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

            };
            #endregion





            return directoryNode1;
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            sharingDoc = new XmlDocument();

            string path = AppDomain.CurrentDomain.BaseDirectory + "\\" + "\\Sharing.xml";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);

            }  
                #region Making Document
                XmlWriter xmlWriter = XmlWriter.Create(path);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Sharing");
                xmlWriter.WriteStartElement("Size");
                xmlWriter.WriteCData("0");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Folders");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                #endregion

            
            sharingDoc.Load(path);
            foreach (string folder in VirtualName.Keys)
            {
               AddFolder(GetFolderRoot(), new DirectoryInfo(folder));
            }
        }
        private XmlNode GetFolderRoot()
        {

            return sharingDoc.SelectSingleNode("//Sharing//Folders");
        }

        private XmlNode AddFolder(XmlNode parent,DirectoryInfo  folderinfo)
        {   string path=AppDomain.CurrentDomain.BaseDirectory +  "\\Sharing.xml";

      
            XmlElement folder = sharingDoc.CreateElement("Folder");
            if (parent == GetFolderRoot())
            {
                folder.SetAttribute("Path", folderinfo.FullName);
                folder.SetAttribute("name", VirtualName[folderinfo.FullName]);
            }
            else
            {
                folder.SetAttribute("name", folderinfo.Name);
                folder.SetAttribute("Path", folderinfo.FullName);
            }
            GetFolderRoot().AppendChild(folder);
        
            parent.AppendChild(folder);
            foreach(DirectoryInfo directory in childs[folderinfo.FullName].Select(x=>new DirectoryInfo(x)) )
            {
                AddFolder(folder, directory);
            }
            foreach(FileInfo file in folderinfo.GetFiles())
            {
                XmlElement fileE = sharingDoc.CreateElement("File");
                fileE.SetAttribute("name", file.Name);
                fileE.SetAttribute("Path", file.FullName);
                fileE.SetAttribute("type", file.Extension);
                fileE.SetAttribute("size",""+file.Length);
                folder.AppendChild(fileE);
            }

            sharingDoc.Save(path);
            return folder;
        }
        /// <summary>
        /// Ashok
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseProfilePic_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Choose Profile Pic";
            ofd.DefaultExt = ".png";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Nullable<bool> res = ofd.ShowDialog();

            if (res.Value)
            {
                pic_path.Text = ofd.FileName;
                Uri uri;
                if (pic_path.Text != null)
                {

                    uri = new Uri(@pic_path.Text, UriKind.Absolute);
                    ImageSource imgSource = new BitmapImage(uri);
                    Profile_pic.Source = imgSource;
                }
            }
        
        }
        private void toBlocked_Click(object sender, RoutedEventArgs e)
        {
            MoveListBoxItems(CompleteList, BlockedList);
        }

        private void toComplete_Click(object sender, RoutedEventArgs e)
        {
            MoveListBoxItems(BlockedList, CompleteList);
        }

        private void MoveListBoxItems(System.Windows.Controls.ListView source, System.Windows.Controls.ListView destination)
        {
            while (source.SelectedItems.Count > 0)
            {
                destination.Items.Add(source.SelectedItems[0]);
                source.Items.Remove(source.SelectedItems[0]);
            }
        }
        private void audiosave_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowDialog();
            AudioSave_path.Text = dlg.SelectedPath;
        }

        private void RemoveProfilePic_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result= MessageBox.Show("Do you want remove profile pic .??", "Alert", MessageBoxButton.YesNo);
            if(result==MessageBoxResult.Yes)
            {
                Profile_pic.Source = new BitmapImage(new Uri("pack://application:,,,/Images/user-icon.png"));
                XmlDocument settings= new XmlDocument();
                
                settings.Load(AppDomain.CurrentDomain.BaseDirectory+"\\UserSettings.xml");

                settings.SelectSingleNode("UserProfile/Appearance/ProfilepicPath").InnerText = "";
                settings.Save(AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml");
                pic_path.Text = "";
            }
        }

        private void allo_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mw.Settings.IsEnabled = true;
        }
    }
}
