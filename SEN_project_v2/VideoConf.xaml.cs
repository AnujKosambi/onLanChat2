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
        public List<User> Users;
        private UDP udp;
        public VideoConf(UDP udp)
        {
            this.udp = udp;
            Users = new List<User>();
         
            InitializeComponent();
        }
        public void AddUser(IPAddress ip)
        {
            Users.Add(UserList.Get(ip));
            
            _stack.Children.Add(new Label() { Content = ip.ToString() });
        }
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
