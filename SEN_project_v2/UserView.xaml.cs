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

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserView : UserControl
    {
        public string u_nick{
            get {
                return (string)ul_Nick.Content;
            }
            set{
                ul_Nick.Content=(string)value;
            }
        }
        public System.Net.IPAddress u_ip
        {
            get
            {
                return System.Net.IPAddress.Parse((string)ul_ip.Content);
            }
            set
            {
                ul_ip.Content = value.ToString();
            }
        }
        public String u_ips
        {
            get
            {
                return (string)ul_ip.Content;
            }
            set
            {
                ul_ip.Content = value;
            }
        }
   

        public UserView()
        {
            InitializeComponent();
            Progressbar.Visibility = Visibility.Hidden;
            //this.openChat.Content = UserList.xml[u_ip].UnreadMessages;
        }

        private void Open_Conf(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            w.Content = new Conversation(u_ip);
         //   w.SizeToContent = SizeToContent.WidthAndHeight;
            
            w.Show();
              
        }

        void ul_check_Checked(object sender, RoutedEventArgs e)
        {

            //if (ul_check.IsChecked==true)
            //    UserList.SelectedUsers.Add(u_ip, true);
            //else
            //    UserList.SelectedUsers.Remove(u_ip);
        }

        private void openChat_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            Conversation conver = new Conversation(u_ip) { udp = MainWindow.udp };
            w.Content = conver;
            conver.Header.Content= u_nick;

            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.Title = u_nick;
            w.Show();
            w.MinWidth = 400;
            w.MinHeight = 400;
            w.MaxWidth = 700;
            if (UserList.xml[u_ip].UnreadMessages > 0)
                MainWindow.udp.SendMessageTo(UDP.RMessage, u_ip);
            UserList.xml[u_ip].UnreadMessages = 0;
            this.openChat.Content = UserList.xml[u_ip].UnreadMessages;
        }

   
    }
}
