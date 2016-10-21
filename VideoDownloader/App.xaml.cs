using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VideoDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length > 0)
            {
                //http://stackoverflow.com/questions/25371737/what-is-the-entry-point-of-a-wpf-application?noredirect=1&lq=1
                MessageBox.Show(String.Join(", ", e.Args.Length), "REM Out in App.xaml.cs", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.WindowState = WindowState.Minimized;
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
                //WindowState.Minimized;
            }
        }
    }
}
