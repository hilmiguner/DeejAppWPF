using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DeejAppWPF.Scripts
{
    class SettingsManager
    {
        public string serialPort;
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
        }

        public void SetSettings(string key, string value)
        {
            string jsonContent = GetJsonContent();
            JObject jsonObject = JObject.Parse(jsonContent);

            jsonObject[key] = value;

            File.WriteAllText("assets/settings.json", jsonObject.ToString());

            if (key == "serialPort") serialPort = value;
        }
    }
}
