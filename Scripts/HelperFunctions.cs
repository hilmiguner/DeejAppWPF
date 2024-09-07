using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeejAppWPF.Scripts
{
    class HelperFunctions
    {
        public static float Normalize(float value)
        {
            float newValue = (value / 1021);
            return newValue > 1 ? 1 : newValue;
        }

        public static void Log(string text, LogForm logForm)
        {
            string logFormText = "";
            if (logForm == LogForm.Log) logFormText = "[LOG]";
            else if (logForm == LogForm.Error) logFormText = "[ERROR]";
            var newText = logFormText + "[" + DateTime.Now.ToString("h:mm:ss tt") + "]" + "[" + text + "]";
            Console.WriteLine(newText);

            string filePath = "assets/log.txt";
            bool success = false;
            int retryCount = 5;
            int delay = 1000;

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(newText);
                        success = true;
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(delay); // Bekle ve tekrar dene
                }
            }
        }

        public enum LogForm
        {
            Log,
            Error
        }
    }
}
