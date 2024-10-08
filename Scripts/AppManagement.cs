﻿using NAudio.CoreAudioApi.Interfaces;
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
using System.Management;
using System.IO;

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
                    Task.Run(AsyncSessionInitializer);
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

        private Dictionary<string, string> ScanAllPorts()
        {
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'";
            string driverNameWithCom = "";
            Dictionary<string, string> resultDict = new Dictionary<string, string>();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    driverNameWithCom = obj["Caption"].ToString();
                    if (driverNameWithCom.Contains("(COM"))
                    {
                        if(driverNameWithCom.Split("(COM")[0].Trim() == "USB-SERIAL CH340")
                        {
                            resultDict["driverName"] = driverNameWithCom.Split("(COM")[0].Trim();
                            resultDict["portName"] = driverNameWithCom.Split("(")[1].Split(")")[0];
                            settingsManager.SetSettings("serialPort", resultDict["portName"]);
                            return resultDict;
                        }
                    }
                }
            }
            return null;
        }

        private bool CheckDeviceName(string serialPort)
        {
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%"+serialPort+"%'";
            string driverNameWithCom = "";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    driverNameWithCom = obj["Caption"].ToString();
                    if (driverNameWithCom.Contains("(COM"))
                    {
                        if (driverNameWithCom.Split("(COM")[0].Trim() == "USB-SERIAL CH340") return true;
                    }
                }
            }
            return false;
        }

        private void ConnectSerialPort(string portName, int baudRate)
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Thread.Sleep(2000);
            serialPort.ReadLine();
        }

        public bool InitializeSerialCommunication()
        {
            bool isInitialized = false;
            while (!isInitialized)
            {
                if (settingsManager.serialPort == "none")
                {
                    Dictionary<string, string> dict = ScanAllPorts();
                    if (dict != null)
                    {
                        ConnectSerialPort(dict["portName"], 9600);
                        isInitialized = true;
                    }
                }
                else
                {
                    bool isCachedPortRight = CheckDeviceName(settingsManager.serialPort);
                    if (isCachedPortRight)
                    {
                        ConnectSerialPort(settingsManager.serialPort, 9600);
                        isInitialized = true;
                    }
                    else
                    {
                        settingsManager.SetSettings("serialPort", "none");
                        Dictionary<string, string> dict = ScanAllPorts();
                        if (dict != null)
                        {
                            ConnectSerialPort(dict["portName"], 9600);
                            isInitialized = true;
                        }
                    }
                }
            }
            return isInitialized;
        }

        private void AsyncSessionInitializer()
        {
            while (true)
            {
                bool willChange = false;

                Dictionary<String, List<SessionItem>> sessions = nAudioManager.GetSessions();

                foreach (KeyValuePair<String, List<SessionItem>> kvp in sessions)
                {
                    if(mainWindow.mainPage.currentSessions.ContainsKey(kvp.Key))
                    {
                        foreach (SessionItem item in kvp.Value)
                        {
                            bool identifierFound = false;
                            foreach (SessionItem currentItem in mainWindow.mainPage.currentSessions[kvp.Key])
                            {
                                if(item.controller.GetSessionInstanceIdentifier == currentItem.controller.GetSessionInstanceIdentifier)
                                {
                                    identifierFound = true;
                                    break;
                                }
                            }
                            if(!identifierFound)
                            {
                                willChange = true;
                                break;
                            }
                        }
                    }
                    else willChange = true;
                }

                foreach (string key in mainWindow.mainPage.currentSessions.Keys)
                {
                    if (!sessions.ContainsKey(key))
                    {
                        willChange = true;
                        break;
                    }
                }

                if (willChange)
                {
                    mainWindow.mainPage.InitializeSessions();
                    mainWindow.mainPage.SetCurrentPreset("presetOne");
                }
                Thread.Sleep(2000);
            }
        }

        private void AsyncSerialCommunicationChecker()
        {
            while (true)
            {
                if (serialPort.IsOpen) {
                    try
                    {
                        serialPort.ReadLine();
                    }
                    catch (Exception ex)
                    {
                    }

                    if (!mainWindow.mainPage.IsControlsEnabled)
                    {
                        mainWindow.mainPage.ToggleControls();
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            loadingWindow.Hide();
                        });
                    }
                }
                else
                {
                    if (mainWindow.mainPage.IsControlsEnabled)
                    {
                        mainWindow.mainPage.ToggleControls();

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
                            mainWindow.mainPage.serialPort = serialPort;
                            mainWindow.mainPage.serialPort.DataReceived += mainWindow.mainPage.DataReceivedHandler;
                        }
                    }
                }
                Thread.Sleep(2000);
            }
        }
    }
}
