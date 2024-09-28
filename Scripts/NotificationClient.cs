using NAudio.CoreAudioApi.Interfaces;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DeejAppWPF.Scripts
{
    public class NotificationClient : IMMNotificationClient
    {
        private NAudioManagement audioManagement;
        private MainPage mainPage;
        public NotificationClient(NAudioManagement audioManagement, MainPage mainPage)
        {
            this.audioManagement = audioManagement;
            this.mainPage = mainPage;
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            // Debug.Print($"Device state changed: {deviceId}, New State: {newState}");
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            // Debug.Print($"Device added: {pwstrDeviceId}");
        }

        public void OnDeviceRemoved(string deviceId)
        {
            // Debug.Print($"Device removed: {deviceId}");
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if(flow == DataFlow.Render)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    audioManagement.SetRenderDeviceByID(defaultDeviceId);
                    mainPage.InitializeSessions();
                    mainPage.SetCurrentPreset("presetOne");
                }); 
            }
            // Debug.Print($"Default device changed: {defaultDeviceId}, DataFlow: {flow}, Role: {role}");
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            // Debug.Print($"Property value changed: {pwstrDeviceId}, fmtid: {key.formatId}, pid: {key.propertyId}");
        }
    }
}
