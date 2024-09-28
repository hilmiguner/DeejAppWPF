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

        public List<MicrophoneItem> allMicrophones;
        public NAudioManagement()
        {
            InitializeDevices();
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
                Process process = Process.GetProcessById((int)sessionController.GetProcessID);
                Icon? icon = null;
                string? name = null;
                if (sessionController.GetProcessID != 0)
                {
                    try
                    {
                        name = process.MainModule.FileVersionInfo.ProductName;
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (name == null || name == "") name = process.ProcessName;
                    }

                    try
                    {
                        icon = Icon.ExtractAssociatedIcon(filePath: process.MainModule.FileName);
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (icon == null) icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/DeejAppWPF;component/assets/image/unknown.ico")).Stream);
                    }

                    if (!sessionDict.ContainsKey(name)) sessionDict[name] = new List<SessionItem>();
                    
                    sessionDict[name].Add(new SessionItem { controller = sessionController, icon = icon, name = name });
                }
            }
            return sessionDict;
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
        }

        public void SetRenderDeviceByID(string deviceID)
        {
            audioDevice = deviceEnumerator.GetDevice(deviceID);
        }
    }
}
