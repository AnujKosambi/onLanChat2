using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace SEN_project_v2
{
    static class UserList
    {
         static List<User> Users=new List<User>();
        public static Dictionary<System.Net.IPAddress, User> OfflineUsers = new Dictionary<System.Net.IPAddress,User>();
        public static Dictionary<System.Net.IPAddress, bool> SelectedUsers = new Dictionary<System.Net.IPAddress, bool>();
        static Dictionary<System.Net.IPAddress,int> indexOf=new Dictionary<System.Net.IPAddress,int>();
        public static Dictionary<System.Net.IPAddress, string> GroupList = new Dictionary<System.Net.IPAddress, string>();
        public static Dictionary<System.Net.IPAddress, XMLClient> xml = new Dictionary<System.Net.IPAddress, XMLClient>();
        public static void ClearAllList()
        {
            Users.Clear();
            SelectedUsers.Clear();
            indexOf.Clear();
            GroupList.Clear();
            xml.Clear();
        }
        public static bool Add(User user)
        {

            
            if (Users.Where(x => x.ip.Equals(user.ip)).ToList().Count == 0 )
            {
                string localPath = "";
                string[] ip_parts = user.ip.ToString().Split('.');
                for (int i = 0; i < 4; i++)
                {
                    if (!System.IO.Directory.Exists(localPath + ip_parts[i])) ;
                    System.IO.Directory.CreateDirectory(localPath + ip_parts[i]);
                    localPath = localPath + ip_parts[i] + "/";
                }

                Users.Add(user);
                indexOf.Add(user.ip, Users.IndexOf(user));
                GroupList.Add(user.ip, user.groupName);
                xml.Add(user.ip, new XMLClient(user.ip));
                return true;
            }
            return false;
        }
      
        public static User Get(System.Net.IPAddress ip)
        {
           
                return Users.ElementAt(indexOf[ip]);
            
            

        }
        public static List<System.Net.IPAddress> Selected
        {
            get
            {
                return SelectedUsers.Where(x => x.Value == true).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
            }
        }
        public static List<System.Net.IPAddress> All
        {
            get
            {
                return Users.Select(x => x.ip).ToList();
            }
        }
        public static void Referese()
        {
            Users=new List<User>();
            SelectedUsers = new Dictionary<System.Net.IPAddress, bool>();
            indexOf=new Dictionary<System.Net.IPAddress,int>();
        }
        public static void  Remove(System.Net.IPAddress ip){
            try
            {
                Users.RemoveAt(indexOf[ip]);
                SelectedUsers.Remove(ip);
                indexOf.Remove(ip);
                GroupList.Remove(ip);
                xml.Remove(ip);
            }
            catch { }            
        }
 
    }
  
}
