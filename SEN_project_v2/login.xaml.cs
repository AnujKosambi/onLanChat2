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

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
        }
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            xd.Load(path);
            SecurityPW security = new SecurityPW();
            if (PasswordBoxLogin.Password == security.Decryptstring(xd.SelectSingleNode("UserProfile/General/Password").InnerText))
            {

                new MainWindow().Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Wrong Password entered");
            }
        }
    }
}
