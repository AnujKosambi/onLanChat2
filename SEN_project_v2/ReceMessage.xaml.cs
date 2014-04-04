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
using System.IO;
using System.Windows.Markup;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for ReceMessage.xaml
    /// </summary>
    public partial class ReceMessage : UserControl
    {
        XMLClient.Message message;
        XMLClient xmlClient;
        public ReceMessage(System.Net.IPAddress ip, string message, string time, XMLClient client)
        {
            InitializeComponent();
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(message));

            Message.Document = (FlowDocument)XamlReader.Load(ms);
            this.xmlClient = client;
            Time.Text = time;
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
            (sender as Label).Foreground = System.Windows.Media.Brushes.DarkRed;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Foreground = System.Windows.Media.Brushes.DarkGray;
        }
    }
}
