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
using System.IO;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
   
    public partial class Conversation : UserControl
    {
        List<XMLClient.Message> messages;
        ResourceDictionary rd = new ResourceDictionary();
        XMLClient client;
        IPAddress ip;
        public UDP udp;
        public Conversation(IPAddress sender)
        {
            InitializeComponent();
            client = UserList.xml[sender];
            ip = sender;
           
        }
        int messIndex = 0;
        int timeIndex=1;
        private void MessagePanel_Loaded(object sender, RoutedEventArgs e)
        {
            Draw();
           
        }
        public static FlowDocument TransformImages(FlowDocument flowDocument,IPAddress ip,int index)
        {
            FlowDocument img_flowDocument = flowDocument;
            Type inlineType;
            InlineUIContainer uic;
            System.Windows.Controls.Image replacementImage;
            int count = 0;
            
            foreach (Block b in flowDocument.Blocks)
            {

                foreach (Inline i in ((Paragraph)b).Inlines)
                {

                    inlineType = i.GetType();
                    if (inlineType == typeof(InlineUIContainer))
                    {
                        uic = ((InlineUIContainer)i);


                        if (uic.Child.GetType() == typeof(System.Windows.Controls.Image))
                        {
                            replacementImage = (System.Windows.Controls.Image)uic.Child;
                            
                            string Path = AppDomain.CurrentDomain.BaseDirectory + "\\" + ip.ToString().Replace('.', '\\') + "\\" + index+"."+count + ".jpg";
                       
                            BitmapImage bitmapImage = new BitmapImage(new Uri(Path, UriKind.Absolute));
                            replacementImage.Source = bitmapImage;
                            replacementImage.Height = bitmapImage.Height;
                            replacementImage.Width = bitmapImage.Width;
                            count++;

                        }
                    }
                }
            }
          
            return img_flowDocument;
        }
               
        public void Redraw()
        {
            MessagePanel.Children.Clear();
            Draw();

        }
        private void Draw()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                messages = client.fetchMessages();
                foreach (XMLClient.Message m in messages)
                {
                    if (m.self)
                    {
                        
                        ReceMessage s = new ReceMessage(ip, m.value, m.time.ToString("hh:mm"),client,m.index);
                        s.SetMessage(m);
                        MessagePanel.Children.Add(s);
                    }
                    else
                    {
                        SentMessage s = new SentMessage(ip, m.value, m.time.ToString("hh:mm"), client,m.index);
                        s.SetMessage(m);
                        MessagePanel.Children.Add(s);
                    }
                }
            }));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(udp!=null)
            udp.SendMessageTo(UDP.Message + SendBox.Text+UDP.Message, ip);
            MessagePanel.UpdateLayout();
            this.UpdateLayout();
        }
    }
}
