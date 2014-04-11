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
using System.IO;
using System.Xml;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Sharing.xaml
    /// </summary>
    public partial class Sharing : Window
    {
        private UDP udp = MainWindow.udp;
        private IPAddress ip;
        string path;
        public XmlDocument sharingDoc;
        public List<String> listFiles;
        public Sharing(IPAddress ip)
        {
            InitializeComponent();
            listFiles = new List<string>();
            this.ip = ip;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            path = AppDomain.CurrentDomain.BaseDirectory + ip.ToString().Replace('.', '\\') + "\\" + "Sharing.xml";
            if (File.Exists(path))
                File.Delete(path);
       //     UserList.Get(ip).userView.Dispatcher.BeginInvoke((Action)(() => {
            udp.SendMessageTo(UDP.Sharing, ip);
       //     }));
            int i = 0;
            while (!File.Exists(path) && i++ < 100)
            {
                System.Threading.Thread.Sleep(100);

            }
            if (!File.Exists(path))
                MessageBox.Show("Problem in Fetching File");
            else
            {
                MessageBox.Show("Loaded");
                sharingDoc = new XmlDocument();
                sharingDoc.Load(path);
                TreeViewItem tvi = CreateDirectoryNode(GetFolderRoot().SelectSingleNode("Folder"), false);
                FolderView.Items.Add(tvi);

            }
        }
        private XmlNode GetFolderRoot()
        {

            return sharingDoc.SelectSingleNode("//Sharing//Folders");
        }
        private TreeViewItem CreateDirectoryNode(XmlNode dir, bool isSelected)
        {
            int checkIndex = 1;

            TreeViewItem directoryNode1 = new System.Windows.Controls.TreeViewItem();
            if(dir.Name!="File")
            directoryNode1.Items.Add(new TreeViewItem() { Height = 0 });

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            CheckBox cb = new CheckBox();
            cb.Margin = new Thickness(5, 0, 0, 0);
            cb.Content = dir.Attributes.GetNamedItem("name");
            directoryNode1.Header = sp;
            if(dir.Name!="File")
            sp.Children.Add(new Image() { Source = BitmapFrame.Create(new Uri("pack://application:,,,/Images/FolderIcon.png")), Width = 16, Height = 16 });
            else
            sp.Children.Add(new Image() { Source = BitmapFrame.Create(new Uri("pack://application:,,,/Images/File.png")), Width = 16, Height = 16 });
            sp.Children.Add(cb);
            cb.Checked += (sender, e) =>
            {
                #region Check
                listFiles.Add(dir.Attributes.GetNamedItem("Path").Value);
                {
                    /*
                    foreach (TreeViewItem item in directoryNode1.Items)
                    {
                        if (item.HasHeader)
                        {
                            ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                        }
                    }*/
                }
                #endregion 
                System.Diagnostics.Debug.WriteLine(string.Join(":", listFiles.ToArray()));
            };


            cb.Unchecked += (sender, e) =>
            {
                #region Uncheck
                listFiles.Remove(dir.Attributes.GetNamedItem("Path").Value);
                /*
                foreach (TreeViewItem item in directoryNode1.Items)
                {
                    if (item.HasHeader)
                    {
                        ((item.Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = false;
                    }
                }*/
                System.Diagnostics.Debug.WriteLine(string.Join(":", listFiles.ToArray()));
                #endregion
            };

            if (dir.Name != "File")
            #region addingFiles
            directoryNode1.Expanded += (sender, e) =>
            {

                if (directoryNode1.Items.Count <= 1)
                {
                    foreach (XmlNode directory in dir.ChildNodes)
                    {
                        try
                        {
                            int index = directoryNode1.Items.Count;

                            directoryNode1.Items.Insert(index, CreateDirectoryNode(directory, cb.IsChecked.Value));
                         //   (((directoryNode1.Items[index] as TreeViewItem).Header as StackPanel).Children[checkIndex] as CheckBox).IsChecked = cb.IsChecked.Value;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

            };
            #endregion





            return directoryNode1;

        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            foreach (string filepath in listFiles)
                MainWindow.udp.SendMessageTo(UDP.SendFile + filepath, ip);
            
        }

     
    }
}
