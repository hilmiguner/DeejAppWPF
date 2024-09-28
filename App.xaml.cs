using DeejAppWPF.Scripts;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DeejAppWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Thread veya Task kaynaklı hatalar için
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task-based hatalar için
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Başlangıçta açılacak pencereyi manuel olarak belirleyin
            AppManagement appManagement = new AppManagement();
        }

        // UI Thread hataları için
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception); // Hata raporu oluşturma
            e.Handled = true; // Programın kapanmaması için
        }

        // Task veya Thread-based hatalar için
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception); // Hata raporu oluşturma
        }

        // Task-based hatalar için
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception); // Hata raporu oluşturma
            e.SetObserved(); // Hata göz ardı edilirse crash olmasını önlemek için
        }

        private void LogException(Exception ex)
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crashes");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string logFileName = $"CrashLog_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
            string logFilePath = Path.Combine(logDirectory, logFileName);
           
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("----Hata Raporu----");
                writer.WriteLine("Tarih: " + DateTime.Now.ToString());
                writer.WriteLine("Hata Mesajı: " + ex.Message);
                writer.WriteLine("Stack Trace: " + ex.StackTrace);
                writer.WriteLine("Inner Exception: " + (ex.InnerException != null ? ex.InnerException.Message : "Yok"));
                writer.WriteLine("--------------------\n");
            }
        }
    }

}
