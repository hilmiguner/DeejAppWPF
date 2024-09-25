using DeejAppWPF.Scripts;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeejAppWPF
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private SettingsManager settingsManager;
        private bool runOnStartUp;
        public SettingsPage()
        {
            settingsManager = new SettingsManager();
            InitializeComponent();

            if (settingsManager.runOnStartUp) runOnStartUp_ToggleButton.IsChecked = true;
            else runOnStartUp_ToggleButton.IsChecked = false;
        }

        private void StartUpButton_Checked(object sender, RoutedEventArgs e)
        {
            settingsManager.SetSettings("runOnStartUp", "true");
            settingsManager.AddToStartup();
        }

        private void StartUpButton_Unchecked(object sender, RoutedEventArgs e)
        {
            settingsManager.SetSettings("runOnStartUp", "false");
            settingsManager.RemoveFromStartup();
        }
    }
}
