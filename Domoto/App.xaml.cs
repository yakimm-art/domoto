using System;
using System.IO;
using System.Windows;

namespace Domoto
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogError(e.Exception);
            MessageBox.Show(
                "An unexpected error occurred. Please check the error log for details.\n\n" + e.Exception.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                LogError(ex);
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TaskManager");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, "error.log");
                string entry = string.Format("[{0}] {1}\n{2}\n---\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ex.Message,
                    ex.StackTrace);
                File.AppendAllText(logFile, entry);
            }
            catch { }
        }
    }
}
