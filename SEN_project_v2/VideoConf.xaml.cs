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
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for VideoConf.xaml
    /// </summary>
    public partial class VideoConf : Window
    {
        public List<User> Users;
        public VideoConf(List<IPAddress> selectedUsers)
        {
            Users = new List<User>();
            foreach (IPAddress ip in selectedUsers)
                Users.Add(UserList.Get(ip));
            InitializeComponent();
        }



    }
}
