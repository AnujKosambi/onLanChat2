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
        public static Dictionary<System.Net.IPAddress, bool> SelectedUsers = new Dictionary<System.Net.IPAddress, bool>();
        static Dictionary<System.Net.IPAddress,int> indexOf=new Dictionary<System.Net.IPAddress,int>();
      
        public static bool Add(User user)
        {


            if (Users.Where(x => x.ip.Equals(user.ip)).ToList().Count == 0)
            {
                Users.Add(user);
                indexOf.Add(user.ip, Users.IndexOf(user));
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
            }
            catch { }            
        }
 
    }
  
}
