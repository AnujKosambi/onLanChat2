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
    /// Interaction logic for VirtualDirectory.xaml
    /// </summary>
    public partial class VirtualDirectory : Window
    {
        public bool ok;
        public string vname;
       
        public VirtualDirectory(string name,ref string vname,ref bool ok)
        {
            InitializeComponent();
            this.Title = name;
            this.Topmost = true;
            ok = this.ok;
            vname = this.vname;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.VirtualName.ContainsValue(VirtualName.Text))
            {
                MessageBox.Show("Name is assigned already to other Directory..\n Please give Different Name");
            }
            else
            {
                ok = true;
                vname = VirtualName.Text;
                this.Close();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
         
            this.Activate();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ok = false;
            this.Close();
        }

     
    }
}
