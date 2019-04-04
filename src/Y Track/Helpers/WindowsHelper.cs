using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Helpers
{
    public class WindowsHelper
    {
        /// <summary>
        /// adds the application to startup list
        /// </summary>
        /// <param name="start"></param>
        public static void ToggleStartAtStartup(bool start)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (start)
                rk.SetValue("YTrack__", System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                rk.DeleteValue("YTrack__", false);

        }

    }
}
