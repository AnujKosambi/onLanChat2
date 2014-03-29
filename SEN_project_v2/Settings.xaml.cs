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
            if (File.Exists("UserSettings.xml") == true)
            {
                settings = new XmlDocument();
                settings.Load("UserSettings.xml");
                NickName.Text = settings.SelectSingleNode("UserProfile/General/Nick").InnerText;
                GroupName.Text = settings.SelectSingleNode("UserProfile/General/GroupName").InnerText;
                PasswordBox.Password = settings.SelectSingleNode("UserProfile/General/Password").InnerText;
                Camera.Text = settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText;
                Microphone.Text = settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText;
            }
            else {
                NickName.Text = "";
                GroupName.Text = "";
                PasswordBox.Password = "";
                Camera.Text = "";
                Microphone.Text = "";
            }
            PasswordBox.IsEnabled = false;
            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
           if (File.Exists("UserSettings.xml") == false)
            {
                XmlWriter xw = XmlWriter.Create("UserSettings.xml");
                xw.WriteStartElement("UserProfile");
                xw.WriteStartElement("General");
                xw.WriteStartElement("Nick");
                xw.WriteString(NickName.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("GroupName");
                xw.WriteString(GroupName.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("Password");
                xw.WriteString(PasswordBox.Password);
                xw.WriteEndElement();
                xw.WriteEndElement(); 
                xw.WriteStartElement("Conference");
                xw.WriteStartElement("Camera");
                xw.WriteString(Camera.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("Microphone");
                xw.WriteString(Microphone.Text);
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();
                settings = new XmlDocument();
                settings.Load("UserSettings.xml");
                settings.SelectSingleNode("UserProfile/General/Nick").InnerText = NickName.Text;
                settings.SelectSingleNode("UserProfile/General/GroupName").InnerText = GroupName.Text;
                settings.SelectSingleNode("UserProfile/General/Password").InnerText = PasswordBox.Password;
                settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText = Camera.Text;
                settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText = Microphone.Text;
                settings.Save("UserSettings.xml"); 
           }
            else
            {
                settings = new XmlDocument();
                settings.Load("UserSettings.xml");
                settings.SelectSingleNode("UserProfile/General/Nick").InnerText = NickName.Text;
                settings.SelectSingleNode("UserProfile/General/GroupName").InnerText = GroupName.Text;
                settings.SelectSingleNode("UserProfile/General/Password").InnerText = PasswordBox.Password;
                settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText = Camera.Text;
                settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText = Microphone.Text;
                settings.Save("UserSettings.xml"); 
           }
            if (PasswordCheck.IsChecked == true) {
                if (PasswordBox.Password == "") { new passworderror().Show(); }
            }
            else this.Close();
            
        }
        XmlDocument settings = null;
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("UserSettings.xml") == false)
            {
                XmlWriter xw = XmlWriter.Create("UserSettings.xml");
                xw.WriteStartElement("UserProfile");
                xw.WriteStartElement("General");
                xw.WriteStartElement("Nick");
                xw.WriteString(NickName.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("GroupName");
                xw.WriteString(GroupName.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("Password");
                xw.WriteString(PasswordBox.Password);
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.WriteStartElement("Conference");
                xw.WriteStartElement("Camera");
                xw.WriteString(Camera.Text);
                xw.WriteEndElement();
                xw.WriteStartElement("Microphone");
                xw.WriteString(Microphone.Text);
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.Close();
                settings = new XmlDocument();
                settings.Load("UserSettings.xml");
                settings.SelectSingleNode("UserProfile/General/Nick").InnerText = NickName.Text;
                settings.SelectSingleNode("UserProfile/General/GroupName").InnerText = GroupName.Text;
                settings.SelectSingleNode("UserProfile/General/Password").InnerText = PasswordBox.Password;
                settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText = Camera.Text;
                settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText = Microphone.Text;
                settings.Save("UserSettings.xml");
            }
            else {
                settings = new XmlDocument();
                settings.Load("UserSettings.xml");
                settings.SelectSingleNode("UserProfile/General/Nick").InnerText = NickName.Text;
                settings.SelectSingleNode("UserProfile/General/GroupName").InnerText = GroupName.Text;
                settings.SelectSingleNode("UserProfile/General/Password").InnerText = PasswordBox.Password;
                settings.SelectSingleNode("UserProfile/Conference/Camera").InnerText = Camera.Text;
                settings.SelectSingleNode("UserProfile/Conference/Microphone").InnerText = Microphone.Text;
                settings.Save("UserSettings.xml");
            }
            if (PasswordCheck.IsChecked == true) {
                if (PasswordBox.Password == "") { new passworderror().Show(); }
                else { PasswordBox.IsEnabled = false; }
            }

        }

        private void PasswordCheck_Checked(object sender, RoutedEventArgs e)
        {
            PasswordBox.IsEnabled = true;
        }

        private void PasswordCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = "";
            PasswordBox.IsEnabled = false;
            ChangePassword.IsEnabled = false;

        }
        
        
    }
}
