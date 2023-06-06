using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EO_Mod_Manager
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            txtEO1.Text = Properties.Settings.Default.EO1_Path;
            txtEO2.Text = Properties.Settings.Default.EO2_Path;
            txtEO3.Text = Properties.Settings.Default.EO3_Path;
            chkCloseWindow.IsChecked = Properties.Settings.Default.progress_close_status;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string paths_not_set = "";
            if (txtEO1.Text != String.Empty && !System.IO.Directory.Exists(txtEO1.Text))
                paths_not_set += "- Etrian Odyssey HD\n";
            if (txtEO2.Text != String.Empty && !System.IO.Directory.Exists(txtEO2.Text))
                paths_not_set += "- Etrian Odyssey II HD\n";
            if (txtEO3.Text != String.Empty && !System.IO.Directory.Exists(txtEO3.Text))
                paths_not_set += "- Etrian Odyssey III HD\n";

            if (paths_not_set != String.Empty)
                MessageBox.Show("The following game paths do not exist:\n" + paths_not_set, "Paths not set", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                Properties.Settings.Default.progress_close_status = (bool)chkCloseWindow.IsChecked;
                Properties.Settings.Default.Save();
                this.Close();
            }
        }

        private void txtEO1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.EO1_Path = txtEO1.Text;
        }

        private void txtEO2_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.EO2_Path = txtEO2.Text;
        }

        private void txtEO3_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.EO3_Path = txtEO3.Text;
        }

        private void chkCloseWindow_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.progress_close_status = (bool)chkCloseWindow.IsChecked;
        }
    }
}
