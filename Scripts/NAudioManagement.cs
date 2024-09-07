using NAudio.CoreAudioApi;
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
    public class NAudioManagement
    {
        public MMDeviceEnumerator deviceEnumerator;
        public MMDevice audioDevice;
        private MMDevice recordingDevice;

        public KeyValuePair<String, List<SessionItem>>[] currentSessions = new KeyValuePair<String, List<SessionItem>>[2];

        public Dictionary<String, List<SessionItem>> allSessions;
        public List<MicrophoneItem> allMicrophones;
        public NAudioManagement()
        {
            HelperFunctions.Log("NAudioManagement objesi oluşturuluyor (NAudioManagement Constructor)", HelperFunctions.LogForm.Log);
            InitializeDevices();

            allSessions = GetSessions();
            allMicrophones = GetAllMicrophones();
            HelperFunctions.Log("NAudioManagement objesi oluşturuldu (NAudioManagement Constructor)", HelperFunctions.LogForm.Log);
        }

        public Dictionary<String, List<SessionItem>> GetSessions()
        {
            HelperFunctions.Log("Session dictionary oluşturuluyor (NAudioManagement.GetSessions)", HelperFunctions.LogForm.Log);
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
                        HelperFunctions.Log(ex.Message+" (NAudioManagement.GetSessions)", HelperFunctions.LogForm.Error);
                    }
                    if (!sessionDict.ContainsKey(name)) sessionDict[name] = new List<SessionItem>();
                    sessionDict[name].Add(new SessionItem { processID = processID, controller = sessionController, icon = icon, name = name });
                }
            }
            HelperFunctions.Log("Session dictionary oluşturuldu (NAudioManagement.GetSessions)", HelperFunctions.LogForm.Log);
            return sessionDict;
        }

        public void FetchSessions()
        {
            allSessions = GetSessions();
        }

        public List<MicrophoneItem> GetAllMicrophones()
        {
            HelperFunctions.Log("Mikrofon listesi oluşturuluyor (NAudioManagement.GetAllMicrophones)", HelperFunctions.LogForm.Log);
            List<MicrophoneItem> microphones = new List<MicrophoneItem>();
            MMDeviceCollection devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            foreach (MMDevice device in devices)
            {
                microphones.Add(new MicrophoneItem { deviceID = device.ID, device = device, name = device.DeviceFriendlyName });
            }
            HelperFunctions.Log("Mikrofon listesi oluşturuldu (NAudioManagement.GetAllMicrophones)", HelperFunctions.LogForm.Log);
            return microphones;
        }

        public void FetchMicrophones()
        {
            allMicrophones = GetAllMicrophones();
        }

        public void InitializeDevices()
        {
            HelperFunctions.Log("NAudio cihazları oluşturuluyor (NAudioManagement.InitializeDevices)", HelperFunctions.LogForm.Log);
            deviceEnumerator = new MMDeviceEnumerator();
            audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            recordingDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
            HelperFunctions.Log("NAudio cihazları oluşturdu (NAudioManagement.InitializeDevices)", HelperFunctions.LogForm.Log);
        }
 
    }
}
