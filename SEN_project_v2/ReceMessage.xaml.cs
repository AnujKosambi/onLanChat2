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

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for ReceMessage.xaml
    /// </summary>
    public partial class ReceMessage : UserControl
    {
        public ReceMessage(System.Net.IPAddress ip,string nick,string time)
        {
            InitializeComponent();
            Message.Text = nick;
            Time.Text = time;
        }
    }
}
