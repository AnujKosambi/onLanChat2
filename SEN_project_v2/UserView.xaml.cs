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
using System.Net.Sockets;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    
    public partial class UserView : UserControl
    {
    //    public Dictionary<string,NetworkStream> stream;
        public Dictionary<NetworkStream, long> lenght;
        public Dictionary<NetworkStream, ProgressBar> ProgressBars;
        public string u_nick{
            get {
                return (string)ul_Nick.Content;
            }
            set{
                ul_Nick.Content=(string)value;
            }
        }
        public string u_machine
        {
            get
            {
                return (string)ul_machine.Content;
            }
            set
            {
                ul_machine.Content = (string)value;
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
          
            lenght = new Dictionary<NetworkStream, long>();
            ProgressBars = new Dictionary<NetworkStream, ProgressBar>();
            InitializeComponent();
            
           // Progressbar.Visibility = Visibility.Hidden;
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
        public void AddProgressBar(NetworkStream stream)
        {
           
            
            ProgressBar prog = new ProgressBar();
            
            prog.VerticalAlignment = VerticalAlignment.Stretch;
            prog.HorizontalAlignment = HorizontalAlignment.Stretch;
         
            prog.Width =150;
            ProgressBars.Add(stream, prog);
            StackProgress.Children.Add(prog);
            
        }
        public void RemoveProgressBar(NetworkStream stream)
        {
            StackProgress.Children.Remove(ProgressBars[stream]);
            ProgressBars.Remove(stream);

        }
        private void openChat_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            Conversation conver =  new Conversation(u_ip) { udp = MainWindow.udp };
            if (UserList.conversation.ContainsKey(u_ip))
                UserList.conversation.Remove(u_ip);
            UserList.conversation.Add(u_ip, conver);
            w.Closing+=(a,b)=>{
               
                UserList.conversation.Remove(u_ip);
            };
            conver.udp = MainWindow.udp;
            //w.WindowStyle = WindowStyle.None;
            //w.AllowsTransparency = true;

            w.Content = conver;
            //w.Background = System.Windows.Media.Brushes.Transparent;
            conver.Nick.Content = u_nick+" ("+u_ip.ToString()+") "; conver.Group.Content = UserList.GroupList[u_ip];
            //w.BorderThickness = new Thickness(25);
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.Title = "Conversation";
            w.Show();
            w.MinWidth = 600;
            w.MinHeight = 500;
            w.MaxWidth = 700;
            w.MaxHeight = 700;
            if (UserList.xml[u_ip].UnreadMessages > 0)
                MainWindow.udp.SendMessageTo(UDP.RMessage, u_ip);
            UserList.xml[u_ip].UnreadMessages = 0;
            
            this.openChat.Content = UserList.xml[u_ip].UnreadMessages;
        }

        private void sharing_Click(object sender, RoutedEventArgs e)
        {
            Sharing sharing = new Sharing(u_ip);
            sharing.Show();
        }

   
    }
}
