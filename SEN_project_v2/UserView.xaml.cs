﻿using System;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserView : UserControl
    {
        public string u_nick{
            get {
                return (string)ul_Nick.Content;
            }
            set{
                ul_Nick.Content=(string)value;
            }
        }
        public System.Net.IPAddress u_ip
        {
            get
            {
                return System.Net.IPAddress.Parse((string)ul_ip.Content);
            }
            set
            {
                ul_ip.Content = value.ToString();
            }
        }
        public String u_ips
        {
            get
            {
                return (string)ul_ip.Content;
            }
            set
            {
                ul_ip.Content = value;
            }
        }
        public bool u_check
        {
            get
            {
                return (bool)ul_check.IsChecked;
            }
            set
            {
                ul_check.IsChecked = value;
            }
        }

        public UserView()
        {
            InitializeComponent();
            ul_check.Click += ul_check_Checked;

        }

        void ul_check_Checked(object sender, RoutedEventArgs e)
        {

            if (ul_check.IsChecked==true)
                UserList.SelectedUsers.Add(u_ip, true);
            else
                UserList.SelectedUsers.Remove(u_ip);
        }
    }
}