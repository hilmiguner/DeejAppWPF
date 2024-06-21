using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
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

namespace DeejAppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;

        private NAudioManagement nAudioManager;

        private System.Windows.Controls.Image[] images = new System.Windows.Controls.Image[2];
        private System.Windows.Controls.ComboBox[] comboBoxes = new System.Windows.Controls.ComboBox[2];
        private Slider[] sliders = new Slider[2];

        private SerialPort serialPort;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            nAudioManager = new NAudioManagement();

            InitializeMasterVolumeSystem();
            InitializeImages();
            InitializeComboBoxes();
            InitializeSliders();
            InitializeSessions();
            InitializeMicrophones();

            InitializeSerialCommunication();
        }

        public void InitializeSerialCommunication()
        {
            string arduinoPort;
            
            string FindArduinoPort()
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    try
                    {
                        using (SerialPort serialPort = new SerialPort(port, 9600))
                        {
                            serialPort.ReadTimeout = 2000;

                            if (serialPort.IsOpen)
                            {
                                serialPort.Close();
                            }

                            serialPort.Open();

                            string response = serialPort.ReadTo("\n");
                            Debug.Print(response);
                            if (response.Split("|")[0] == "DeejApp")
                            {
                                return port;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Hata: " + ex.Message);
                        continue;
                    }
                }
                return null;
            }
            
            arduinoPort = FindArduinoPort();
            if (arduinoPort != null)
            {
                Debug.Print("ARDUINO BULUNDU. " + arduinoPort);
                serialPort = new SerialPort(arduinoPort, 9600);
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                serialPort.Open();
            }
            else
            {
                Debug.Print("ARDUINO BULUNAMADI.");
            }
            
        }

        private float Normalize(float value)
        {
            return (value / 1023);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();
            string[] dataList = data.Split("|");
            float[] pins = new float[4];
            pins[0] = Normalize(float.Parse(dataList[1]));
            pins[1] = Normalize(float.Parse(dataList[2]));
            pins[2] = Normalize(float.Parse(dataList[3]));
            pins[3] = Normalize(float.Parse(dataList[4]));
            UpdateValues(pins);
        }

        private void UpdateValues(float[] pins)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateMasterLevel(pins[0]);

                UpdateMicrophoneLevel(pins[1]);

                UpdateSessionOne(pins[2]);

                UpdateSessionTwo(pins[3]);
            });
        }

        private void UpdateMasterLevel(float newValue)
        {
            // Mute check BEGIN
            if (newValue == 0.0d)
            {
                nAudioManager.audioDevice.AudioEndpointVolume.Mute = true;
            }
            else
            {
                if (nAudioManager.audioDevice.AudioEndpointVolume.Mute == true) nAudioManager.audioDevice.AudioEndpointVolume.Mute = false;
            }
            // Mute check END

            nAudioManager.audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newValue;
            run_masterVolumeLevel.Text = ((int)(newValue*100)).ToString();
        }

        private void UpdateMicrophoneLevel(float newValue)
        {
            MicrophoneItem microphoneItem = comboBox_microphones.SelectedItem as MicrophoneItem;
            if (microphoneItem != null)
            {
                // Mute check BEGIN
                if (newValue == 0.0f)
                {
                    microphoneItem.device.AudioEndpointVolume.Mute = true;
                }
                else
                {
                    if (microphoneItem.device.AudioEndpointVolume.Mute == true) microphoneItem.device.AudioEndpointVolume.Mute = false;
                }
                // Mute check END

                microphoneItem.device.AudioEndpointVolume.MasterVolumeLevelScalar = newValue;
                run_microphoneLevel.Text = ((int)(newValue * 100)).ToString();
            }
        }

        private void UpdateSessionOne(float newValue)
        {
            var currentItemOne = comboBox_sessionOne.SelectedItem;
            if (currentItemOne != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItemOne;

                List<SessionItem> currentSessionList = kvp.Value;
                if (currentSessionList != null)
                {
                    string currentSessionName = currentSessionList[0].name;

                    // Mute check BEGIN
                    if (newValue == 0.0f)
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            session.controller.SimpleAudioVolume.Mute = true;
                        }
                    }
                    else
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            if (session.controller.SimpleAudioVolume.Mute == true) session.controller.SimpleAudioVolume.Mute = false;
                        }
                    }
                    // Mute check END

                    foreach (SessionItem session in currentSessionList)
                    {
                        session.controller.SimpleAudioVolume.Volume = newValue;
                    }
                    run_sessionOne.Text = ((int)(newValue * 100)).ToString();
                }
            }
        }

        private void UpdateSessionTwo(float newValue)
        {
            var currentItemTwo = comboBox_sessionTwo.SelectedItem;
            if (currentItemTwo != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItemTwo;

                List<SessionItem> currentSessionList = kvp.Value;
                if (currentSessionList != null)
                {
                    string currentSessionName = currentSessionList[0].name;

                    // Mute check BEGIN
                    if (newValue == 0.0f)
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            session.controller.SimpleAudioVolume.Mute = true;
                        }
                    }
                    else
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            if (session.controller.SimpleAudioVolume.Mute == true) session.controller.SimpleAudioVolume.Mute = false;
                        }
                    }
                    // Mute check END

                    foreach (SessionItem session in currentSessionList)
                    {
                        session.controller.SimpleAudioVolume.Volume = newValue;
                    }
                    run_sessionTwo.Text = ((int)(newValue * 100)).ToString();
                }
            }
        }

        public void InitializeNotifyIcon()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open Deej App", null, showApp);
            contextMenu.Items.Add("Close App", null, closeApp);
            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/assets/image/deejApp.ico")).Stream);
            notifyIcon.Visible = true;
        }

        private void showApp(object sender, EventArgs e)
        {
            this.Show();
        }

        private void closeApp(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void InitializeMasterVolumeSystem()
        {
            slider_masterVolume.Value = (int)(nAudioManager.audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            run_masterVolumeLevel.Text = slider_masterVolume.Value.ToString();
        }

        private void InitializeImages()
        {
            images[0] = image_sessionOne;
            images[1] = image_sessionTwo;
        }

        private void InitializeComboBoxes()
        {
            comboBoxes[0] = comboBox_sessionOne;
            comboBoxes[1] = comboBox_sessionTwo;
        }

        private void InitializeSliders()
        {
            sliders[0] = slider_sessionOne;
            sliders[1] = slider_sessionTwo;
        }

        private void InitializeSessions()
        {
            nAudioManager.FetchSessions();
            foreach (System.Windows.Controls.ComboBox comboBox in comboBoxes)
            {
                Dictionary<String, List<SessionItem>> sessionList = nAudioManager.GetSessions();
                comboBox.ItemsSource = sessionList;
                comboBox.DisplayMemberPath = "Key";
                //comboBox.SelectedValuePath = "Value";
            }
        }

        private void InitializeMicrophones()
        {
            nAudioManager.FetchMicrophones();

            comboBox_microphones.ItemsSource = nAudioManager.GetAllMicrophones();
            comboBox_microphones.DisplayMemberPath = "name";
        }

        private void slider_masterVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volume = slider_masterVolume.Value / 100.0d;

            // Mute check BEGIN
            if (volume == 0.0d)
            {
                nAudioManager.audioDevice.AudioEndpointVolume.Mute = true;
            }
            else
            {
                if (nAudioManager.audioDevice.AudioEndpointVolume.Mute == true) nAudioManager.audioDevice.AudioEndpointVolume.Mute = false;
            }
            // Mute check END

            nAudioManager.audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)volume;
            run_masterVolumeLevel.Text = ((int)slider_masterVolume.Value).ToString();
        }

        private void updateSlidersByName(int parentIndex, String name)
        {
            for (int i = 0; i < 2; i++)
            {
                var currentItem = comboBoxes[i].SelectedItem;
                if (currentItem != null) 
                {
                    if (((KeyValuePair<string, List<SessionItem>>)currentItem).Key == name && i != parentIndex)
                    {
                        sliders[i].Value = (int)(((KeyValuePair<string, List<SessionItem>>)currentItem).Value[0].controller.SimpleAudioVolume.Volume * 100.0f);
                    }
                }
            }
        }

        private void updateLabels()
        {
            run_sessionOne.Text = ((int)slider_sessionOne.Value).ToString();
            run_sessionTwo.Text = ((int)slider_sessionTwo.Value).ToString();
        }

        private void comboBox_sessionOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = comboBox_sessionOne.SelectedItem;
            if (currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                List<SessionItem> currentItemList = kvp.Value;

                string currentSessionName = currentItemList[0].name;
                nAudioManager.currentSessions[currentSessionName] = currentItemList;
                image_sessionOne.Source = IconToBitmapImage(nAudioManager.currentSessions[currentSessionName][0].icon);
                slider_sessionOne.Value = (int)(this.nAudioManager.currentSessions[currentSessionName][0].controller.SimpleAudioVolume.Volume * 100);
                run_sessionOne.Text = slider_sessionOne.Value.ToString();
            }
        }

        private void slider_sessionOne_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var currentItem = comboBox_sessionOne.SelectedItem;
            if(currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                List<SessionItem> currentSessionList = kvp.Value;
                if (currentSessionList != null)
                {
                    string currentSessionName = currentSessionList[0].name;
                    double volume = slider_sessionOne.Value / 100.0d;
                    run_sessionOne.Text = ((int)slider_sessionOne.Value).ToString();

                    // Mute check BEGIN
                    if (volume == 0.0f)
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            session.controller.SimpleAudioVolume.Mute = true;
                        }
                    }
                    else
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            if (session.controller.SimpleAudioVolume.Mute == true) session.controller.SimpleAudioVolume.Mute = false;
                        }
                    }
                    // Mute check END

                    foreach (SessionItem session in currentSessionList)
                    {
                        session.controller.SimpleAudioVolume.Volume = (float)volume;
                    }
                    updateSlidersByName(0, nAudioManager.currentSessions[currentSessionName][0].name);
                    updateLabels();
                }
            }  
        }

        private void comboBox_sessionTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = comboBox_sessionTwo.SelectedItem;
            if (currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                List<SessionItem> currentItemList = kvp.Value;

                string currentSessionName = currentItemList[0].name;
                nAudioManager.currentSessions[currentSessionName] = currentItemList;
                image_sessionTwo.Source = IconToBitmapImage(nAudioManager.currentSessions[currentSessionName][0].icon);
                slider_sessionTwo.Value = (int)(this.nAudioManager.currentSessions[currentSessionName][0].controller.SimpleAudioVolume.Volume * 100);
                run_sessionTwo.Text = slider_sessionTwo.Value.ToString();
            }
        }

        private void slider_sessionTwo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var currentItem = comboBox_sessionTwo.SelectedItem;
            if(currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                List<SessionItem> currentSessionList = kvp.Value;
                if (currentSessionList != null)
                {
                    string currentSessionName = currentSessionList[0].name;
                    double volume = slider_sessionTwo.Value / 100.0d;
                    run_sessionTwo.Text = ((int)slider_sessionTwo.Value).ToString();

                    // Mute check BEGIN
                    if (volume == 0.0f)
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            session.controller.SimpleAudioVolume.Mute = true;
                        }
                    }
                    else
                    {
                        foreach (SessionItem session in currentSessionList)
                        {
                            if (session.controller.SimpleAudioVolume.Mute == true) session.controller.SimpleAudioVolume.Mute = false;
                        }
                    }
                    // Mute check END

                    foreach (SessionItem session in currentSessionList)
                    {
                        session.controller.SimpleAudioVolume.Volume = (float)volume;
                    }
                    updateSlidersByName(1, nAudioManager.currentSessions[currentSessionName][0].name);
                    updateLabels();
                }
            }
        }

        private void comboBox_microphones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_microphones.SelectedItem != null)
            {
                slider_microphone.Value = (int)((comboBox_microphones.SelectedItem as MicrophoneItem).device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                run_microphoneLevel.Text = slider_microphone.Value.ToString();
            }
        }

        private void slider_microphone_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MicrophoneItem microphoneItem = comboBox_microphones.SelectedItem as MicrophoneItem;
            if (microphoneItem != null)
            {
                double newValue = slider_microphone.Value / 100.0d;

                // Mute check BEGIN
                if (newValue == 0.0f)
                {
                    microphoneItem.device.AudioEndpointVolume.Mute = true;
                }
                else
                {
                    if (microphoneItem.device.AudioEndpointVolume.Mute == true) microphoneItem.device.AudioEndpointVolume.Mute = false;
                }
                // Mute check END

                microphoneItem.device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)newValue;
                run_microphoneLevel.Text = ((int)slider_microphone.Value).ToString();
            }
        }

        private BitmapImage IconToBitmapImage(Icon icon)
        {
            using (Bitmap bmp = icon.ToBitmap())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Bitmap'i memory stream'e PNG formatında kaydediyoruz
                    bmp.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Memory stream'den BitmapImage oluşturuyoruz
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            nAudioManager.InitializeDevices();

            string[] oldSessionNames = new string[2];
            int index = 0;
            foreach (var kvp in nAudioManager.currentSessions)
            {
                if (kvp.Value == null) oldSessionNames[index] = null;
                else
                {
                    oldSessionNames[index] = kvp.Value[0].name;
                }
                index++;
            }
            InitializeSessions();
            for (int i = 0; i < oldSessionNames.Length; i++)
            {
                string sessionName = oldSessionNames[i];
                bool didFind = false;
                index = 0;
                foreach (var kvp in nAudioManager.allSessions)
                {
                    if (kvp.Value[0].name == sessionName)
                    {
                        comboBoxes[i].SelectedIndex = index;
                        didFind = true;
                        break;
                    }
                    index++;
                }
                if (didFind == false) images[i].Source = null;
            }

            string oldDeviceID = comboBox_microphones.SelectedItem != null ? (comboBox_microphones.SelectedItem as MicrophoneItem).deviceID : "null";
            InitializeMicrophones();
            index = 0;
            foreach (MicrophoneItem microphone in nAudioManager.allMicrophones)
            { 
                if (microphone.deviceID == oldDeviceID) 
                { 
                    comboBox_microphones.SelectedIndex = index;
                    break;
                }
                index++;
            }
        }
    }
}