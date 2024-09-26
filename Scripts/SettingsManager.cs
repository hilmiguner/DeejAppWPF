using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace DeejAppWPF.Scripts
{
    class SettingsManager
    {
        public string serialPort;
        public bool runOnStartUp;
        public SettingsManager() 
        {
            ReadSettingsFile();
        }
        private String GetJsonContent()
        {
            return File.ReadAllText("assets/settings.json");
        }

        private void ReadSettingsFile()
        {
            using var document = JsonDocument.Parse(GetJsonContent());

            JsonElement root = document.RootElement;

            serialPort = root.GetProperty("serialPort").GetString();
            runOnStartUp = bool.Parse(root.GetProperty("runOnStartUp").GetString());
        }

        public void SetSettings(string key, string value)
        {
            string jsonContent = GetJsonContent();
            JObject jsonObject = JObject.Parse(jsonContent);

            jsonObject[key] = value;

            File.WriteAllText("assets/settings.json", jsonObject.ToString());

            if (key == "serialPort") serialPort = value;
            if (key == "runOnStartUp") runOnStartUp = bool.Parse(value);
        }

        public void AddToStartup()
        {
            string applicationPath = '"' + Process.GetCurrentProcess().MainModule.FileName + '"';

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (registryKey == null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            }

            registryKey.SetValue("DeejApp", applicationPath);
        }

        public void RemoveFromStartup()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (registryKey != null)
            {
                registryKey.DeleteValue("DeejApp", false);
            }
        }
    }
}
