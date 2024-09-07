using NAudio.CoreAudioApi.Interfaces;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace DeejAppWPF.Scripts
{
    public class AppManagement
    {
        private NotifyIcon notifyIcon;
        private NAudioManagement nAudioManager;
        private SerialPort serialPort;
        private SettingsManager settingsManager;
        private MainWindow mainWindow;
        private Task serialPortChecker;
        private LoadingWindow loadingWindow;
        public AppManagement() 
        {
            InitializeNotifyIcon();

            loadingWindow = new LoadingWindow();
            Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    nAudioManager = new NAudioManagement();
                    nAudioManager.audioDevice.AudioSessionManager.OnSessionCreated += AudioSessionManager_OnSessionCreated;
                    Task.Run(AsyncDefaultAudioDeviceChecker);
                });

                settingsManager = new SettingsManager();

                bool isSerialPortInitialized = InitializeSerialCommunication();
                
                System.Windows.Application.Current.Dispatcher.Invoke(() => mainWindow = new MainWindow(nAudioManager, serialPort));
                
                if (isSerialPortInitialized)
                {
                    serialPortChecker = Task.Run(AsyncSerialCommunicationChecker);
                    if (mainWindow != null) System.Windows.Application.Current.Dispatcher.Invoke(() => mainWindow.Show());
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    loadingWindow.Hide();
                    Task.Run(AsyncSessionStateChecker);
                });
            });
            loadingWindow.ShowDialog();

            
        }

        private void InitializeNotifyIcon()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open Deej App", null, showApp);
            contextMenu.Items.Add("Close App", null, closeApp);
            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/assets/image/deejApp.ico")).Stream);
            notifyIcon.MouseClick += notifyIconMouseDoubleClick;
            notifyIcon.MouseDoubleClick += notifyIconMouseDoubleClick;
            notifyIcon.Visible = true;
        }

        private void showApp(object sender, EventArgs e)
        {
            if(mainWindow != null && mainWindow.IsLoaded) mainWindow.Show();
        }

        private void closeApp(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void notifyIconMouseDoubleClick(object sender, EventArgs e)
        {
            if (mainWindow != null) System.Windows.Application.Current.Dispatcher.Invoke(() => mainWindow.Show());
        }

        public bool InitializeSerialCommunication()
        {
            string FindArduinoPort()
            {
                string ScanPorts()
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
                                        settingsManager.SetSettings("serialPort", port);
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

                if (settingsManager.serialPort == "none")
                {
                    return ScanPorts();
                }
                else
                {
                    try
                    {
                        using (SerialPort serialPort = new SerialPort(settingsManager.serialPort, 19200))
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
                                    return settingsManager.serialPort;
                                }
                            }
                            serialPort.Close();
                        }
                        settingsManager.SetSettings("serialPort", "none");
                    }
                    catch (Exception ex)
                    {
                        settingsManager.SetSettings("serialPort", "none");
                        Console.WriteLine("Hata: " + ex.Message);
                    }
                    return ScanPorts();
                }
            }

            string arduinoPort = FindArduinoPort();

            if (arduinoPort != null)
            {
                Debug.Print("ARDUINO BULUNDU. " + arduinoPort);
                serialPort = new SerialPort(arduinoPort, 19200);
                serialPort.Open();
                Thread.Sleep(2000);
                serialPort.ReadLine();
                return true;
            }
            else
            {
                Debug.Print("ARDUINO BULUNAMADI.");
                return false;
            }
        }

        public void AudioSessionManager_OnSessionCreated(object sender, IAudioSessionControl session)
        {
            mainWindow.InitializeSessions();
            mainWindow.SetCurrentPreset("presetOne");
        }

        private void AsyncSerialCommunicationChecker()
        {
            while (true)
            {
                if (serialPort.IsOpen) {
                    if (!mainWindow.IsControlsEnabled)
                    {
                        mainWindow.ToggleControls();
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            loadingWindow.Hide();
                        });
                    }
                }
                else
                {
                    if(mainWindow.IsControlsEnabled)
                    {
                        mainWindow.ToggleControls();

                        Task.Run(() => 
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                loadingWindow.SetLoadingText("Trying reconnect to device...");
                                loadingWindow.ShowDialog();
                            });
                        });
                        
                    }
                    bool isComFound = false;
                    foreach (string portName in SerialPort.GetPortNames())
                    {
                        if (portName == serialPort.PortName)
                        {
                            isComFound = true;
                            break;
                        }
                    }
                    if (isComFound)
                    {
                        serialPort.Open();
                    }
                    else
                    {
                        bool didInitialized = InitializeSerialCommunication();
                        if (didInitialized)
                        {
                            mainWindow.serialPort = serialPort;
                            mainWindow.serialPort.DataReceived += mainWindow.DataReceivedHandler;
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }
    
        private void AsyncSessionStateChecker()
        {
            while (true)
            {
                foreach (var sessionDictionary in nAudioManager.allSessions)
                {
                    foreach (var session in sessionDictionary.Value)
                    {
                        if (session.controller.State == AudioSessionState.AudioSessionStateExpired)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                mainWindow.InitializeSessions();
                                mainWindow.SetCurrentPreset("presetOne");
                            });
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }

        private void AsyncDefaultAudioDeviceChecker()
        {
            MMDevice tempDevice;
            MMDevice currentDevice;
            while (true)
            {
                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        tempDevice = nAudioManager.deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                        currentDevice = nAudioManager.audioDevice;
                        if (currentDevice.DeviceFriendlyName != tempDevice.DeviceFriendlyName)
                        {
                            nAudioManager.InitializeDevices();
                            nAudioManager.audioDevice.AudioSessionManager.OnSessionCreated += AudioSessionManager_OnSessionCreated;
                            mainWindow.InitializeSessions();
                            mainWindow.SetCurrentPreset("presetOne");
                        }
                    });
                }
                catch (Exception e) { }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
