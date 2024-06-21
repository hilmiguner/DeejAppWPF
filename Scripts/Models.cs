using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace DeejAppWPF.Scripts
{
    public class SessionItem
    {
        public uint processID { get; set; }
        public string name { get; set; }
        public AudioSessionControl controller { get; set; }
        public Icon icon { get; set; }
    }
    public class MicrophoneItem
    {
        public string deviceID { get; set; }
        public string name { get; set; }
        public MMDevice device { get; set; }
    }
}
