using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Y_Track
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {



        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => _handleUnhandledExceptions((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) => _handleUnhandledExceptions(e.Exception);

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                _handleUnhandledExceptions(e.Exception);
            };

        }


        public static bool SameProcessRunning()
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            return (Process.GetProcessesByName(thisProc.ProcessName).Length > 1);
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            if (SameProcessRunning())
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("Application is already running.");
                Application.Current.Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        private void _handleUnhandledExceptions(Exception e)
        {
            Fiddler.SystemProxyManager.StopSystemProxy();
            Helpers.YTrackLogger.Log("Unhandled Exception Occured : " + e.Message + "\n\n" + e.StackTrace);
        }

   
    }
}
