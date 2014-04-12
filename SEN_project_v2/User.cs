using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;

using System.Windows.Threading;
namespace SEN_project_v2
{
   public  class User
    {
        public IPAddress ip;
        public string nick;
        public string groupName="";
        public IPAddress hostIP;
         public UserView userView;
         public Boolean IsOffline = false;
         public Boolean IsMobile = false;
        public User(IPAddress _ip, string _nick)
        {
            this.ip = _ip;
            this.nick = _nick;
            this.groupName = "Others";
        }
        public User(IPAddress _ip, string _nick, string _groupName)
        {
            this.ip = _ip;
            this.nick = _nick;
            
            this.groupName = _groupName;
            if (groupName == "" || groupName==null)
                this.groupName = "Others";
        }
        public UserView  CreateView()
        {

            if (userView == null)
            {
                if (!IsMobile)
                { userView = new UserView() { u_ip = ip, u_nick = nick }; }
                else
                {
                    userView = new UserView() { u_ip = ip, u_nick = nick };
                    userView.sharing.Visibility = System.Windows.Visibility.Hidden;
                }
                userView.openChat.Content = UserList.xml[ip].UnreadMessages;
            }
            return userView;
        }
    }

}
