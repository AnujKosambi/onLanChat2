using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;


namespace SEN_project_v2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static System.Windows.Forms.NotifyIcon nicon;
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\UserSettings.xml";
        public App()
        {
            nicon = new System.Windows.Forms.NotifyIcon();
            if (File.Exists("UserSettings.xml") == true)
            {
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                xd.Load(path);
                if (xd.SelectSingleNode("UserProfile/General/passwordenabled").InnerText == "yes")
                {
                    this.StartupUri = new Uri("pack://application:,,,/login.xaml");
                }
                else
                {
                    this.StartupUri = new Uri("pack://application:,,,/MainWindow.xaml");
                }
            }
            else
            {
                this.StartupUri = new Uri("pack://application:,,,/MainWindow.xaml");
            }
        }

    }
}
