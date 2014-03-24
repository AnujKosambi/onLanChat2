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
    /// Interaction logic for SentMessage.xaml
    /// </summary>
    public partial class SentMessage : UserControl
    {
        public SentMessage(IPAddress ip, string message, string time)
        {
            InitializeComponent();
            Message.Text = message;
            Time.Text = time;
        }
    }
}
