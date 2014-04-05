using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
namespace SEN_project_v2
{
    public class XMLClient

    {

        private IPAddress ip;
        private String nick;
        private String groupName;
        private String path;
        private XmlDocument xmlDoc;
        public Message lastMessage;
        public XMLClient(IPAddress user)
        {
            this.ip = user;
            this.nick = UserList.Get(user).nick;
            this.groupName = UserList.Get(user).groupName;
            
            path =AppDomain.CurrentDomain.BaseDirectory + "\\" + user.ToString().Replace('.', '\\') + "\\Message.xml";
            xmlDoc = new XmlDocument();

            if (!System.IO.File.Exists(path))
            {
                XmlWriter xmlWriter = XmlWriter.Create(path);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Messages");
                xmlWriter.WriteStartElement("Count");
                xmlWriter.WriteCData("0");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Unread");
                xmlWriter.WriteCData("0");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();

            }

            xmlDoc.Load(path);
        }
     
        public struct Message
        {
            public int index;
            public DateTime time;
            public String value;
            public Boolean self;
        }
        public List<Message> fetchMessages()
        {
            List<Message> messageList = new List<Message>();
            XmlNodeList nodelist = xmlDoc.SelectNodes("//Messages//Message");

            foreach (XmlNode xn in nodelist)
            {
                Message m = new Message();
                m.index = Int32.Parse(xn.Attributes.GetNamedItem("index").Value);
                m.time = DateTime.Parse(xn.Attributes.GetNamedItem("time").Value);
                m.self = Boolean.Parse(xn.Attributes.GetNamedItem("self").Value);
                m.value = xn.InnerText;

                messageList.Add(m);
            }
            return messageList;
        }
        private XmlNode fetchRootOfMessages()
        {

            return xmlDoc.GetElementsByTagName("Messages")[0];
        }
        public void addMessage(DateTime time, String value)
        {
            XmlElement message = xmlDoc.CreateElement("Message");
            message.SetAttribute("index", CountMessages.ToString());
            message.SetAttribute("time", DateTime.Now + "");
            message.SetAttribute("self", "false");
            message.InnerText = value;
            fetchRootOfMessages().AppendChild(message);
            lastMessage.time = time;
            lastMessage.index = CountMessages;
            lastMessage.value = value;
            CountMessages++;
            UnreadMessages++;
        }
        public void addSelfMessage(DateTime time, String value)
        {
            XmlElement message = xmlDoc.CreateElement("Message");
            message.SetAttribute("index", CountMessages.ToString());
            message.SetAttribute("time", DateTime.Now + "");
            message.SetAttribute("self", "true");

            message.InnerText = value;
            fetchRootOfMessages().AppendChild(message);
            CountMessages++;
            xmlDoc.Save(path);
        }

        public void deleteMessage(Message m)
        {
            XmlNodeList nodelist = xmlDoc.SelectNodes("//Messages//Message");

            foreach (XmlNode xn in nodelist)
            {
                if (xn.Attributes.GetNamedItem("index").Value.Equals(m.index.ToString()))
                  //  if (xn.Attributes.GetNamedItem("self").Value.Equals(m.self.ToString()))
                    fetchRootOfMessages().RemoveChild(xn);
            }
            xmlDoc.Save(path);
        }
        public int CountMessages
        {
            get
            {

                return Int32.Parse(xmlDoc.SelectSingleNode("//Messages//Count").InnerText);
            }
            set
            {
                xmlDoc.SelectSingleNode("//Messages//Count").InnerText = value + "";
                try
                {
                    xmlDoc.Save(path);
                }
                catch (Exception e) { }
            }
        }
        public int UnreadMessages
        {
            get
            {

                return Int32.Parse(xmlDoc.SelectSingleNode("//Messages//Unread").InnerText);
            }
            set
            {
                xmlDoc.SelectSingleNode("//Messages//Unread").InnerText = value + "";
                try
                {
                    xmlDoc.Save(path);
                }
                catch (Exception e) { }
            }
        }
    }

}
