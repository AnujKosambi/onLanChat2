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
using System.Net;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class Conversation : UserControl
    {
        static List<XMLClient.Message> messages;
        ResourceDictionary rd = new ResourceDictionary();
        XMLClient client;
        IPAddress ip;
             
        public Conversation(IPAddress sender)
        {
            InitializeComponent();
            client = new XMLClient(sender);
            ip = sender;
           
        }
        int messIndex = 0;
        int timeIndex=1;
        private void MessagePanel_Loaded(object sender, RoutedEventArgs e)
        {

            Dispatcher.BeginInvoke((Action)(() =>
            {
                messages = client.fetchMessages();
                foreach (XMLClient.Message m in messages)
                {
                    if(m.self)
                    {
                        ReceMessage s = new ReceMessage(ip, m.value, m.time.ToString("hh:mm"));
                        MessagePanel.Children.Add(s);
                    }
                    else { 
                    SentMessage s = new SentMessage(ip,m.value,m.time.ToString("hh:mm"));
                    MessagePanel.Children.Add(s);
                    }
                }
            }));
        }
    }
}
