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
using System.Windows.Markup;
using System.IO;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for SentMessage.xaml
    /// </summary>
    public partial class SentMessage : UserControl
    {
        XMLClient xmlClient;
        XMLClient.Message message;
        public SentMessage(IPAddress ip, string message, string time,XMLClient client,int index)
        {
            try
            {
                InitializeComponent();
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(message));
                if (!UserList.Get(ip).IsMobile)
                {
                    Message.Document = Conversation.TransformImages((FlowDocument)XamlReader.Load(ms), ip, index);
                }
                else
                {
                    string[] splits = message.Split(new String[] { UDP.Breaker }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length > 1)
                    {
                        Paragraph para = new Paragraph();
                        para.Inlines.Add(new Run(splits[0]+"\n"));
                        Paragraph para2 = new Paragraph();
                        para.Inlines.Add(new Run(splits[1]));
                        
                        Message.Document.Blocks.Add(para);
                        Message.Document.Blocks.Add(para2);
                    }
                }
                //  Message.Text = message;
                Time.Text = time;
                this.xmlClient = client;
            }
            catch(Exception e)
            {
                MessageBox.Show("Error "+e.Message);
            }
        }
 
        public void SetMessage(XMLClient.Message m)
        {
            message = m;
        }
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            xmlClient.deleteMessage(message);
            (this.Parent as StackPanel).Children.Remove(this);
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Label).Foreground = System.Windows.Media.Brushes.Orange;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Foreground = System.Windows.Media.Brushes.DarkGray;
        }
    }
}
