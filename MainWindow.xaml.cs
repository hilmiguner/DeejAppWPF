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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private NotifyIcon notifyIcon;

        private NAudioManagement nAudioManager;

        private System.Windows.Controls.Image[] images = new System.Windows.Controls.Image[2];
        private System.Windows.Controls.ComboBox[] comboBoxes = new System.Windows.Controls.ComboBox[2];
        private Slider[] sliders = new Slider[2];

        private SerialPort serialPort;

        private PresetManager presetManager;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            nAudioManager = new NAudioManagement(this);
            presetManager = new PresetManager();

            InitializeImages();
            InitializeComboBoxes();
            InitializeSessions();
            InitializeMicrophones();
            InitializeSerialCommunication();

            SetCurrentPreset("presetOne");
        }

        private float Normalize(float value)
        {
            float newValue = (value / 1021);
            return newValue > 1 ? 1 : newValue;
        }

        private void UpdateValues(float[] pins)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    UpdateMasterLevel(pins[0]);

                    UpdateMicrophoneLevel(pins[1]);

                    UpdateSessionOne(pins[2]);

                    UpdateSessionTwo(pins[3]);
                });
            }
            catch (Exception ex) { }
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
            progressBar_MasterAudio.Value = (int)(newValue * 100);
            run_masterVolumeLevel.Text = ((int)(newValue * 100)).ToString();
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
            }
            progressBar_microphone.Value = (int)(newValue * 100);
            run_microphoneLevel.Text = ((int)(newValue * 100)).ToString();
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
                }
            }
            progressBar_sessionOne.Value = (int)(newValue * 100);
            run_sessionOne.Text = ((int)(newValue * 100)).ToString();
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
                }
            }
            progressBar_sessionTwo.Value = (int)(newValue * 100);
            run_sessionTwo.Text = ((int)(newValue * 100)).ToString();
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

        public void InitializeSessions()
        {
            nAudioManager.FetchSessions();
            foreach (System.Windows.Controls.ComboBox comboBox in comboBoxes)
            {
                Dictionary<String, List<SessionItem>> sessionList = nAudioManager.GetSessions();
                this.Dispatcher.Invoke(() =>
                {
                    comboBox.ItemsSource = sessionList;
                    comboBox.DisplayMemberPath = "Key";
                });
            }
        }

        private void InitializeMicrophones()
        {
            nAudioManager.FetchMicrophones();

            comboBox_microphones.ItemsSource = nAudioManager.GetAllMicrophones();
            comboBox_microphones.DisplayMemberPath = "name";
        }

        public void InitializeSerialCommunication()
        {
            string FindArduinoPort()
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    try
                    {
                        using (SerialPort serialPort = new SerialPort(port, 19200))
                        {
                            serialPort.ReadTimeout = 2000;

                            if (serialPort.IsOpen)
                            {
                                serialPort.Close();
                            }

                            serialPort.Open();

                            Thread.Sleep(2000);

                            string response;
                            //string response = serialPort.ReadTo("\n");
                            for (int i = 0; i < 3; i++)
                            {
                                response = serialPort.ReadLine();
                                Debug.Print("Gelen yanıt: " + response);
                                if (response.Contains("|") && response.Split("|")[0] == "DeejApp")
                                {
                                    serialPort.Close();
                                    return port;
                                }
                            }
                            serialPort.Close();
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

            string arduinoPort = FindArduinoPort();

            if (arduinoPort != null)
            {
                while (serialPort == null)
                {
                    Debug.Print("ARDUINO BULUNDU. " + arduinoPort);
                    serialPort = new SerialPort(arduinoPort, 19200);
                    serialPort.Open();
                    Thread.Sleep(2000);
                    serialPort.ReadLine();
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    serialPort.ReadLine();
                }
            }
            else
            {
                Debug.Print("ARDUINO BULUNAMADI.");
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                string[] dataList = data.Split("|");
                float[] pins = new float[4];
                pins[0] = Normalize(float.Parse(dataList[1]));
                pins[1] = Normalize(float.Parse(dataList[2]));
                pins[2] = Normalize(float.Parse(dataList[3]));
                pins[3] = Normalize(float.Parse(dataList[4]));
                int tempCount = 0;
                foreach (var item in pins)
                {
                    tempCount++;
                    Debug.Print(tempCount.ToString() + ". pin değeri: " + item.ToString());
                }
                UpdateValues(pins);
            }
            catch (Exception ex) { }
        }

        public void SetCurrentPreset(string presetName)
        {
            presetManager.FetchPreset(presetName);
            Dictionary<String, String> presets = presetManager.GetPreset();

            if (presets["microphone"] != "")
            {
                foreach (MicrophoneItem session in comboBox_microphones.ItemsSource)
                {
                    if (session.name.ToLower() == presets["microphone"].ToLower()) 
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            comboBox_microphones.SelectedItem = session;
                        });
                        break;
                    }
                }
            }

            if (presets["sessionOne"] != "")
            {
                foreach (KeyValuePair<String, List<SessionItem>> kvp in comboBox_sessionOne.ItemsSource)
                {
                    if (kvp.Key.ToLower() == presets["sessionOne"].ToLower())
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            comboBox_sessionOne.SelectedItem = kvp;
                        });
                        break;
                    }
                }
            }

            if (presets["sessionTwo"] != "")
            {
                foreach (KeyValuePair<String, List<SessionItem>> kvp in comboBox_sessionTwo.ItemsSource)
                {
                    if (kvp.Key.ToLower() == presets["sessionTwo"].ToLower())
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            comboBox_sessionTwo.SelectedItem = kvp;
                        });
                        break;
                    }
                }
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

        private void comboBox_microphones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = comboBox_microphones.SelectedItem;
            if (currentItem != null)
            {
                presetManager.SetPreset(GetComboBoxStrings());
            }
        }

        private void comboBox_sessionOne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = comboBox_sessionOne.SelectedItem;
            if (currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                bool didFind = false;

                for (int i = 0; i < nAudioManager.currentSessions.Length; i++)
                {
                    if (i != 0 && nAudioManager.currentSessions[i].Key == kvp.Key)
                    {
                        didFind = true;
                        break;
                    }
                }

                if (!didFind)
                {
                    List<SessionItem> currentItemList = kvp.Value;

                    string currentSessionName = currentItemList[0].name;
                    nAudioManager.currentSessions[0] = new KeyValuePair<String, List<SessionItem>>(kvp.Key, currentItemList) ;
                    image_sessionOne.Source = IconToBitmapImage(nAudioManager.currentSessions[0].Value[0].icon); ;

                    presetManager.SetPreset(GetComboBoxStrings());
                }
                else
                {
                    comboBox_sessionOne.SelectedItem = nAudioManager.currentSessions[0];
                }
            }
        }

        private void comboBox_sessionTwo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentItem = comboBox_sessionTwo.SelectedItem;
            if (currentItem != null)
            {
                KeyValuePair<string, List<SessionItem>> kvp = (KeyValuePair<string, List<SessionItem>>)currentItem;

                bool didFind = false;

                for (int i = 0; i < nAudioManager.currentSessions.Length; i++)
                {
                    if (i != 1 && nAudioManager.currentSessions[i].Key == kvp.Key)
                    {
                        didFind = true;
                        break;
                    }
                }

                if (!didFind)
                {
                    List<SessionItem> currentItemList = kvp.Value;

                    string currentSessionName = currentItemList[0].name;
                    nAudioManager.currentSessions[1] = new KeyValuePair<String, List<SessionItem>>(kvp.Key, currentItemList);
                    image_sessionTwo.Source = IconToBitmapImage(nAudioManager.currentSessions[1].Value[0].icon); ;

                    presetManager.SetPreset(GetComboBoxStrings());
                }
                else
                {
                    comboBox_sessionTwo.SelectedItem = nAudioManager.currentSessions[1];
                }
            }
        }
    
        private String[] GetComboBoxStrings()
        {
            String[] strings = new String[4];

            strings[0] = "presetOne";

            for (int i = 1; i < strings.Length; i++)
            {
                strings[i] = "";
            }

            MicrophoneItem currentMicrophone = comboBox_microphones.SelectedItem as MicrophoneItem;
            // Debug.Print("KONTROL 0: " + (currentMicrophone != null).ToString());
            if (currentMicrophone != null) strings[1] = currentMicrophone.name;

            var currentSessionOne = comboBox_sessionOne.SelectedItem;
            // Debug.Print("KONTROL 1: " + (currentSessionOne != null).ToString());
            if (currentSessionOne != null) strings[2] = ((KeyValuePair<string, List<SessionItem>>)currentSessionOne).Key;

            var currentSessionTwo = comboBox_sessionTwo.SelectedItem;
            // Debug.Print("KONTROL 2: " + (currentSessionTwo != null).ToString());
            if (currentSessionTwo != null) strings[3] = ((KeyValuePair<string, List<SessionItem>>)currentSessionTwo).Key;

            return strings;
        }  
    }
}