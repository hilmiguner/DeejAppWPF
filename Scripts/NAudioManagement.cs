﻿using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeejAppWPF.Scripts
{
    class NAudioManagement
    {
        private MMDeviceEnumerator deviceEnumerator;
        public MMDevice audioDevice;
        private MMDevice recordingDevice;

        private MainWindow mainWindowObject;

        public KeyValuePair<String, List<SessionItem>>[] currentSessions = new KeyValuePair<String, List<SessionItem>>[2];

        public Dictionary<String, List<SessionItem>> allSessions;
        public List<MicrophoneItem> allMicrophones;
        public NAudioManagement(MainWindow mainWindow)
        {
            mainWindowObject = mainWindow;

            InitializeDevices();

            allSessions = GetSessions();
            allMicrophones = GetAllMicrophones();
        }

        public Dictionary<String, List<SessionItem>> GetSessions()
        {
            Dictionary<String, List<SessionItem>> sessionDict = new Dictionary<String, List<SessionItem>>();

            audioDevice.AudioSessionManager.RefreshSessions();
            SessionCollection allSessions = audioDevice.AudioSessionManager.Sessions;
            for (int i = 0; i < allSessions.Count; i++)
            {
                AudioSessionControl sessionController = allSessions[i];
                var process = Process.GetProcessById((int)sessionController.GetProcessID);
                Icon? icon = null;
                string? name = null;
                uint processID = 0;
                if (sessionController.GetProcessID != 0)
                {
                    try
                    {
                        name = process.ProcessName;
                        processID = sessionController.GetProcessID;
                        icon = Icon.ExtractAssociatedIcon(filePath: process.MainModule.FileName);
                    }
                    catch (Exception ex)
                    {

                    }
                    if (!sessionDict.ContainsKey(name)) sessionDict[name] = new List<SessionItem>();
                    sessionDict[name].Add(new SessionItem { processID = processID, controller = sessionController, icon = icon, name = name });
                }
            }
            return sessionDict;
        }

        public void FetchSessions()
        {
            allSessions = GetSessions();
        }

        public List<MicrophoneItem> GetAllMicrophones()
        {
            List<MicrophoneItem> microphones = new List<MicrophoneItem>();
            MMDeviceCollection devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            foreach (MMDevice device in devices)
            {
                microphones.Add(new MicrophoneItem { deviceID = device.ID, device = device, name = device.DeviceFriendlyName });
            }

            return microphones;
        }

        public void FetchMicrophones()
        {
            allMicrophones = GetAllMicrophones();
        }

        public void InitializeDevices()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            recordingDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

            audioDevice.AudioSessionManager.OnSessionCreated += AudioSessionManager_OnSessionCreated;
        }

        private void AudioSessionManager_OnSessionCreated(object sender, IAudioSessionControl session)
        {
            AudioSessionControl temp = new AudioSessionControl(session);
            Debug.Print("New audio session created: " + temp.GetSessionIdentifier);
            mainWindowObject.InitializeSessions();
            mainWindowObject.SetCurrentPreset("presetOne");
        }
    }
}
