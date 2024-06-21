using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeejAppWPF.Scripts
{    
    class PresetManager
    {
        public string microphone = "";
        public string sessionOne = "";
        public string sessionTwo = "";

        public PresetManager() 
        {
            FetchPreset("presetOne");
        }

        public void FetchPreset(string presetName)
        {
            using var document = JsonDocument.Parse(GetJsonString());

            JsonElement root = document.RootElement;

            JsonElement preset = root.GetProperty(presetName);
            microphone = preset.GetProperty("microphone").GetString();
            sessionOne = preset.GetProperty("sessionOne").GetString();
            sessionTwo = preset.GetProperty("sessionTwo").GetString();  
        }

        private String GetJsonString()
        {
            return File.ReadAllText("presets.json");
        }

        public Dictionary<String, String> GetPreset()
        {
            Dictionary<String, String> preset = new Dictionary<String, String>();
            preset["microphone"] = microphone;
            preset["sessionOne"] = sessionOne;
            preset["sessionTwo"] = sessionTwo;
            return preset;
        }

        public void SetPreset(string[] data)
        {
            string jsonContent = GetJsonString();
            JObject jsonObject = JObject.Parse(jsonContent);

            if (data[1] != "") jsonObject[data[0]]["microphone"] = data[1];
            if (data[2] != "") jsonObject[data[0]]["sessionOne"] = data[2];
            if (data[3] != "") jsonObject[data[0]]["sessionTwo"] = data[3];

            File.WriteAllText("presets.json", jsonObject.ToString());

            microphone = data[1];
            sessionOne = data[2];
            sessionTwo = data[3];
        }
    }
}
