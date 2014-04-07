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
using System.Windows.Shapes;
using System.Net;
using System.IO;
namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Sharing.xaml
    /// </summary>
    public partial class Sharing : Window
    {
        private UDP udp = MainWindow.udp;
        private IPAddress ip;
        public Sharing(IPAddress ip)
        {
            InitializeComponent();
            this.ip = ip;
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path=AppDomain.CurrentDomain.BaseDirectory + ip.ToString().Replace('.', '\\') + "\\" + "Sharing.xml";
            if(File.Exists(path))
                File.Delete(path);
            udp.SendMessageTo(UDP.Sharing, ip);
            int i=0;
            while (!File.Exists(path) && i++<10*10)
            {
                System.Threading.Thread.Sleep(100);
               
            }
            if (!File.Exists(path))
            MessageBox.Show("Problem in Fetching File");
            MessageBox.Show("Loaded");

        }

     
    }
}
