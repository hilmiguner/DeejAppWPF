using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.CoreAudioApi;
using DeejAppWPF.Scripts;
using System.IO.Ports;
using System;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DeejAppWPF
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainPage mainPage;
        public SettingsPage settingsPage;
        private string currentPage;
        public MainWindow(NAudioManagement nAudioManager, SerialPort serialPort)
        {
            mainPage = new MainPage(nAudioManager, serialPort);
            settingsPage = new SettingsPage();
            InitializeComponent();
            MainFrame.Navigate(mainPage);
            currentPage = "mainPage";
        }

        private void AudioLevelsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(currentPage != "mainPage")
            {
                MainFrame.Navigate(mainPage);
                currentPage = "mainPage";
            }
        }

        private void SettingsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentPage != "settingsPage")
            {
                MainFrame.Navigate(settingsPage);
                currentPage = "settingsPage";
            }
        }
        
        public void showApp(object sender, EventArgs e)
        {
            this.Show();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}