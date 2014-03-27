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
using System.Xml;
using System.IO;

namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            if (File.Exists("UserSettings.xml") == false)
            {
                //NickName.Text = "";
                //PasswordBox.Password = "";
            }
        }
        String path = ""; //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        XmlWriter xw = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(path+"UserSettings.xml"))
            xw = XmlWriter.Create("UserSettings.xml");
        }


    }
}
