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

namespace SEN_project_v2
{

    public partial class Settings : Window
    {
        public static Dictionary<string, List<string>> childs;
        public static Dictionary<CheckBox, string> sharedFiles;
        public static Dictionary<string, string> VirtualName;
        SecurityPW security = null;

        public Settings()
        {
            InitializeComponent();
            sharedFiles = new Dictionary<CheckBox,string>();
            VirtualName = new Dictionary<string, string>();
            childs = new Dictionary<string, List<string>>();
            loadUser();
            if (File.Exists("UserSettings.xml") == true)
            {
                String key = NickName.Text;
                XmlDocument settings = new XmlDocument();
                settings.Load("UserSettings.xml");
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
                String colour = settings.SelectSingleNode("UserProfile/Appearance/Colour").InnerText;
                Color color = (Color)ColorConverter.ConvertFromString(colour);
                ColorPicker.SelectedColor = color;
                BrushConverter bc = new BrushConverter();
                this.Background = (Brush)bc.ConvertFrom(colour);
                RadialGradientBrush rgb = new RadialGradientBrush();
                rgb.RadiusY = 0.75;
                Point p = new Point(0.5, 0);
                rgb.Center = p;
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BF919191"), 1));
                rgb.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#BFFFFFFF"), 0));
                this.Background = rgb;


            }
            else
            {
                NickName.Text = "";
                GroupName.Text = "";
                PasswordBox.IsEnabled = false;
                ChangePassword.IsEnabled = false;
                Camera.Text = "";
                Microphone.Text = "";
                PasswordCheck.IsChecked = false;
                //BrushConverter bc = new BrushConverter();
                //this.Background = (Brush)bc.ConvertFrom("FF4F4646");
            }
            FilterInfoCollection cam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            {
                foreach (FilterInfo info in cam)
                {
                    Camera.Items.Add(info.Name);

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
            if (File.Exists("UserSettings") == false)
            {
                XmlWriter xw = XmlWriter.Create("UserSettings.xml");

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
                xw.WriteEndElement();

                xw.WriteStartElement("FileSharing");
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();

                XmlDocument xd = new XmlDocument();
                xd.Load("UserSettings.xml");
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
                security = new SecurityPW();
                storedpassword = security.Encryptstring(PasswordBox.Password);
                xd.SelectSingleNode("UserProfile/General/Password").InnerText = storedpassword;
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
                xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText = ColorPicker.SelectedColor.ToString();
                String colour = xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText;
                Color color = (Color)ColorConverter.ConvertFromString(colour);
                ColorPicker.SelectedColor = color;
                BrushConverter bc = new BrushConverter();
                this.Background = (Brush)bc.ConvertFrom(colour);
                xd.Save("UserSettings.xml");

            }
            else
            {
                XmlDocument xd = new XmlDocument();
                xd.Load("UserSettings.xml");
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
                security = new SecurityPW();
                storedpassword = security.Encryptstring(PasswordBox.Password);
                xd.SelectSingleNode("UserProfile/General/Password").InnerText = storedpassword;
                PasswordBox.Password = security.Decryptstring(xd.SelectSingleNode("UserProfile/General/Password").InnerText);
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
                xd.SelectSingleNode("UserProfile/Appearance/Colour").InnerText = ColorPicker.SelectedColor.ToString();
                xd.Save("UserSettings.xml");
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
 
            this.Close();

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
            ListDirectory(SharedFiles, "H:\\");

        }
        private void ListDirectory(System.Windows.Controls.TreeView treeview, String path)
        {
            treeview.Items.Clear();
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);
            TreeViewItem tvi=CreateDirectoryNode(rootDirectoryInfo,false);
            treeview.Items.Add(tvi);
            
        }
        
        private  TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo,bool isSelected)
        {
            int checkIndex = 1;
            
            TreeViewItem directoryNode1 = new System.Windows.Controls.TreeViewItem();
            directoryNode1.Items.Add(new TreeViewItem());
            
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            CheckBox cb = new CheckBox();
            cb.Margin = new Thickness(5, 0, 0, 0);
            cb.Content = directoryInfo.Name;
            directoryNode1.Header = sp;
           
            sp.Children.Add(new Image() { Source = BitmapFrame.Create(new Uri("pack://application:,,,/Images/FolderIcon.png")) ,Width=16,Height=16});
            sp.Children.Add(cb);
            cb.Checked += (sender, e) => {
                string vname = "";
                bool ok = false;

                if ((((directoryNode1.Parent as TreeViewItem).Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked.Value == false)
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
                        if (!VirtualName.ContainsKey(directoryInfo.FullName))
                        {
                            VirtualName.Add(directoryInfo.FullName, vd.vname);
                            childs.Add(directoryInfo.FullName, new List<string>());

                        }
                        foreach (TreeViewItem item in directoryNode1.Items)
                        {
                            if (item.HasHeader)
                            {
                                ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = true;
                            }
                        }
                    }
                }
                else
                {
                    childs[(string)directoryInfo.Parent.FullName].Add(directoryInfo.FullName);
                    childs.Add(directoryInfo.FullName, new List<string>());
                    System.Diagnostics.Debug.WriteLine(string.Join(":", childs[(string)directoryInfo.Parent.FullName].ToArray()));
                    foreach (TreeViewItem item in directoryNode1.Items)
                    {
                        if (item.HasHeader)
                        {
                            ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                        }
                    }
                }
            };
           
            
            cb.Unchecked += (sender, e) => {

            foreach (TreeViewItem item in directoryNode1.Items)
            {
                if (item.HasHeader)
                {
                    ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                }
            }
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
            };
      

            directoryNode1.Expanded += (sender, e) =>
            {
         
                if (directoryNode1.Items.Count <= 1)
                {
                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        try
                        {
                            int index = directoryNode1.Items.Count;

                            directoryNode1.Items.Insert(index, CreateDirectoryNode(directory, cb.IsChecked.Value));
                            (((directoryNode1.Items[index] as TreeViewItem).Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = cb.IsChecked.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

            };
            


         

            return directoryNode1;
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
