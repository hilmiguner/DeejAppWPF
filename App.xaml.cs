using DeejAppWPF.Scripts;
using System.Configuration;
using System.Data;
using System.Windows;

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

            // Başlangıçta açılacak pencereyi manuel olarak belirleyin
            AppManagement appManagement = new AppManagement();
        }
    }

}
