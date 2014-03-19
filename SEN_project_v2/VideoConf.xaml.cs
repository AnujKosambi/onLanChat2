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
//using System.Net.Sockets;
using System.Net;
using System.Net.Sockets;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for VideoConf.xaml
    /// </summary>
    public partial class VideoConf : Window
    {
        public List<IPAddress> Users;
        private UDP udp;
        public List<IPAddress> requestedUsers;
        public Dictionary<IPAddress,VideoPreview> vp;
        public IPAddress host;
        public VideoConf(UDP udp,IPAddress host)
        {
            this.host = host;
            this.udp = udp;
            Users = new List<IPAddress>();
            vp = new Dictionary<IPAddress, VideoPreview>();
            requestedUsers = UserList.Selected.Where(x => MainWindow.hostIPS.Contains(x) == false).ToList();
            InitializeComponent();
        }

        public void AddUser(IPAddress ip)
        {
            Users.Add(ip);

            
        }
        public void MakeUserPreview(IPAddress ip,VideoPreview.Mode mode)
        {
            vp.Add(ip, new VideoPreview(mode, ip) { Nick = UserList.Get(ip).nick });
            vp[ip].udp = udp;
            vp[ip].hostIP = host;
            _stack.Children.Add(vp[ip]);
        }
/// <summary>
/// only for host
/// </summary>

        public void Start() //IF Host
        {
            foreach (IPAddress ip in UserList.Selected)
            {
                if (!MainWindow.hostIPS.Contains(ip))
                    udp.SendMessageTo(UDP.Videocall, ip);

            }
               

        }
        


    }
}
