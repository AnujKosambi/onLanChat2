using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace SEN_Project_v1
{
    public class clients
    {
        private IPEndPoint ipaddress;
        private String username;
        public clients(IPEndPoint ipaddress)
        {
            this.ipaddress = ipaddress;
            
        }
        public clients(IPEndPoint ipaddress,String name)
        {
            this.ipaddress = ipaddress;
            this.username = name;
            

        }
        public String[] getMessages(){

            return null;
        }
    }
}
