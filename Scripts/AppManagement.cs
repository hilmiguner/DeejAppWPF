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
using System.Runtime.InteropServices;

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
            AllocConsole();
            HelperFunctions.Log("Uygulama açıldı (AppManagemenet Constructor)", HelperFunctions.LogForm.Log);
            InitializeNotifyIcon();

            loadingWindow = new LoadingWindow();
            Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    nAudioManager = new NAudioManagement();
                    nAudioManager.audioDevice.AudioSessionManager.OnSessionCreated += AudioSessionManager_OnSessionCreated;
                    HelperFunctions.Log("Audio device on session created eventi bağlandı (AppManagement Constructor)", HelperFunctions.LogForm.Log);
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

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

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
            HelperFunctions.Log("Seri bağlantı iletişimi başlatılıyor (AppManagement.InitializeSerialCommunication)", HelperFunctions.LogForm.Log);
            string FindArduinoPort()
            {
                string ScanPorts()
                {
                    HelperFunctions.Log("Doğru portu bulmak için bilgisayardaki tüm portlar taranıyor (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);
                    string[] ports = SerialPort.GetPortNames();
                    foreach (string port in ports)
                    {
                        HelperFunctions.Log("Denenen port numarası: "+port+ " (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);
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
                                HelperFunctions.Log("Denenen port açıldı (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);

                                Thread.Sleep(2000);

                                string response;
                                //string response = serialPort.ReadTo("\n");
                                for (int i = 0; i < 3; i++)
                                {
                                    response = serialPort.ReadLine();
                                    HelperFunctions.Log("Denenen porttan gelen yanıt: " + response + " (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);
                                    if (response.Contains("|") && response.Split("|")[0] == "DeejApp")
                                    {
                                        serialPort.Close();
                                        settingsManager.SetSettings("serialPort", port);
                                        HelperFunctions.Log("Port bulundu ve önbelleğe kaydedildi: "+port+" (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);
                                        return port;
                                    }
                                }
                                serialPort.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            HelperFunctions.Log(ex.Message+" (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Error);
                            continue;
                        }
                    }
                    HelperFunctions.Log("Denenen portlardan hiç biri uyuşmadı (AppManagement.InitializeSerialCommunication.FindArduinoPort.ScanPorts)", HelperFunctions.LogForm.Log);
                    return null;
                }

                if (settingsManager.serialPort == "none")
                {
                    HelperFunctions.Log("Önbellekte port bulunamadı. (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Log);
                    return ScanPorts();
                }
                else
                {
                    HelperFunctions.Log("Önbellektei mevcut port numarası deneniyor. (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Log);
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
                            HelperFunctions.Log("Test Port açıldı (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Log);
                            for (int i = 0; i < 3; i++)
                            {
                                response = serialPort.ReadLine();
                                HelperFunctions.Log("Gelen yanıt: "+response+" (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Log);
                                //Debug.Print("Gelen yanıt: " + response);
                                if (response.Contains("|") && response.Split("|")[0] == "DeejApp")
                                {
                                    serialPort.Close();
                                    return settingsManager.serialPort;
                                }
                            }
                            serialPort.Close();
                        }
                        HelperFunctions.Log("Önbellekteki port numarası hatalı ya da yanlış (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Log);
                        settingsManager.SetSettings("serialPort", "none");
                    }
                    catch (Exception ex)
                    {
                        HelperFunctions.Log(ex.Message + " (AppManagement.InitializeSerialCommunication.FindArduinoPort)", HelperFunctions.LogForm.Error);
                        settingsManager.SetSettings("serialPort", "none");
                        Console.WriteLine("Hata: " + ex.Message);
                    }
                    return ScanPorts();
                }
            }

            string arduinoPort = FindArduinoPort();

            if (arduinoPort != null)
            {
                HelperFunctions.Log("Arduino portu bulundu: "+arduinoPort+" (AppManagement.InitializeSerialCommunication)", HelperFunctions.LogForm.Log);
                serialPort = new SerialPort(arduinoPort, 19200);
                serialPort.Open();
                Thread.Sleep(2000);
                serialPort.ReadLine();
                return true;
            }
            else
            {
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
            HelperFunctions.Log("Seri port iletişim kontrolcüsü başladı (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
            while (true)
            {
                HelperFunctions.Log("Seri port iletişim kontrolcüsü döngü (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                if (serialPort.IsOpen) {
                    HelperFunctions.Log("Seri port açık (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
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
                    HelperFunctions.Log("Seri port kapalı (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                    if (mainWindow.IsControlsEnabled)
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
                    HelperFunctions.Log("Hali hazırdaki port ile diğer portların isimleri karşılaştırılıyor (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
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
                        HelperFunctions.Log("Karşılaştırma sonucunda uyuşan port bulundu (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                        serialPort.Open();
                    }
                    else
                    {
                        HelperFunctions.Log("Karşılaştırma sonucunda uyuşan port bulunamadı (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                        bool didInitialized = InitializeSerialCommunication();
                        if (didInitialized)
                        {
                            mainWindow.serialPort = serialPort;
                            mainWindow.serialPort.DataReceived += mainWindow.DataReceivedHandler;
                            HelperFunctions.Log("Cihaza tekrardan bağlantı sağlandı (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                        }
                    }
                }
                HelperFunctions.Log("Seri port iletişim kontrolcüsü döngü çıkışı (AppManagemenet.AsyncSerialCommunicationChecker)", HelperFunctions.LogForm.Log);
                Thread.Sleep(2000);
            }
        }
    
        private void AsyncSessionStateChecker()
        {
            HelperFunctions.Log("Async session state checker başladı (AppManagemenet.AsyncSessionStateChecker)", HelperFunctions.LogForm.Log);
            while (true)
            {
                HelperFunctions.Log("Async session state checker döngü (AppManagemenet.AsyncSessionStateChecker)", HelperFunctions.LogForm.Log);
                foreach (var sessionDictionary in nAudioManager.allSessions)
                {
                    foreach (var session in sessionDictionary.Value)
                    {
                        if (session.controller.State == AudioSessionState.AudioSessionStateExpired)
                        {
                            HelperFunctions.Log("Programı kapanmış session bulundu: "+session.name+" (AppManagemenet.AsyncSessionStateChecker)", HelperFunctions.LogForm.Log);
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                mainWindow.InitializeSessions();
                                mainWindow.SetCurrentPreset("presetOne");
                            });
                        }
                    }
                }
                HelperFunctions.Log("Async session state checker döngü çıkışı (AppManagemenet.AsyncSessionStateChecker)", HelperFunctions.LogForm.Log);
                Thread.Sleep(2000);
            }
        }

        private void AsyncDefaultAudioDeviceChecker()
        {
            HelperFunctions.Log("Async Default Audio Device Checker başladı (AppManagement.AsyncDefaultAudioDeviceChecker)", HelperFunctions.LogForm.Log);
            MMDevice tempDevice;
            MMDevice currentDevice;
            while (true)
            {
                HelperFunctions.Log("Async Default Audio Device Checker Döngü girildi (AppManagement.AsyncDefaultAudioDeviceChecker)", HelperFunctions.LogForm.Log);
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
                catch (Exception e) 
                {
                    HelperFunctions.Log(e.Message+ " (AsyncDefaultAudioDeviceChecker)", HelperFunctions.LogForm.Error);
                }
                finally
                {
                    HelperFunctions.Log("Async Default Audio Device Checker Döngü çıkıldı (AppManagement.AsyncDefaultAudioDeviceChecker)", HelperFunctions.LogForm.Log);
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
