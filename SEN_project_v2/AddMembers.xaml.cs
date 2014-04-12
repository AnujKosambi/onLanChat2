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

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddMembers : Window
    {
        List<System.Net.IPAddress> ips;
        VideoConf videoConf;
        AudioConf audioConf;
        public AddMembers(VideoConf vc)
        {
            this.videoConf = vc;
            this.audioConf = null;
            InitializeComponent();
            ips = new List<System.Net.IPAddress>();
            this.Title = "AddMember";
            foreach(User user in UserList.Users)

            {
                if (!vc.Users.Contains(user.ip) && !MainWindow.hostIPS.Contains(user.ip))
                {
                    listView.Items.Insert(listView.Items.Count, user.nick + " " + user.ip);
                    ips.Insert(ips.Count, user.ip);
                }
            }
        }
        public AddMembers(AudioConf vc)
        {
            this.audioConf = vc;
            this.videoConf = null;
            InitializeComponent();
            ips = new List<System.Net.IPAddress>();
            this.Title = "AddMember";
            foreach (User user in UserList.Users)
            {
                if (!vc.Users.Contains(user.ip) && !MainWindow.hostIPS.Contains(user.ip))
                {
                    listView.Items.Insert(listView.Items.Count, user.nick + " " + user.ip);
                    ips.Insert(ips.Count, user.ip);
                }
            }
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach(string ipstring in listView.SelectedItems)
            {
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipstring.Split(' ')[1]);
                if(videoConf==null)
                {
                    audioConf.MakeUserPreview(ip, AudioPreview.Mode.Watting);
                    MainWindow.udp.SendMessageTo(UDP.Audiocall, ip);
                }
                else if (audioConf != null)
                {
                    videoConf.MakeUserPreview(ip, VideoPreview.Mode.Watting);
                    MainWindow.udp.SendMessageTo(UDP.Videocall, ip);

                }
            }
            this.Close();

        }
    }
}
