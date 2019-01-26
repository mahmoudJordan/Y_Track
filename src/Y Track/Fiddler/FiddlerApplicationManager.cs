using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Fiddler
{
    public class FiddlerApplicationManager
    {
        public delegate void FiddlerStatusChanged(object sender, FiddlerStatusEventArguments e);
        public event FiddlerStatusChanged OnUpdateStatus;

        public async Task<bool> StartFiddler(int runningPort)
        {
            return await Task.Run(() =>
            {
                _checkCertificate();
                
                if (!FiddlerApplication.IsStarted())
                {
                    FiddlerApplication.Startup(this._getConfiguration(runningPort));
                    if (OnUpdateStatus != null)
                        OnUpdateStatus(this, new FiddlerStatusEventArguments() { Status = FiddlerStatus.Started });
                    return true;
                }
                return false;
            });
        }


        public async Task<bool> StopFiddler()
        {
            return await Task.Run(() =>
            {
                if (FiddlerApplication.IsStarted())
                {
                    FiddlerApplication.Shutdown();
                    if (OnUpdateStatus != null)
                        OnUpdateStatus(this, new FiddlerStatusEventArguments() { Status = FiddlerStatus.Stopped });
                    return true;
                }
                return false;
            });
        }



        private void _checkCertificate()
        {
            //FiddlerCertificatesManager.UninstallCertificate();
            //FiddlerCertificatesManager.InstallCertificate();
            if (!FiddlerCertificatesManager.IsInstalled())
                FiddlerCertificatesManager.InstallCertificate();
        }

        private FiddlerCoreStartupSettings _getConfiguration(int runningPort)
        {
            FiddlerCoreStartupSettingsBuilder builder = new FiddlerCoreStartupSettingsBuilder();
            builder.DecryptSSL();
            builder.OptimizeThreadPool();
            builder.RegisterAsSystemProxy();
            builder.ListenOnPort((ushort)runningPort);
            builder.AllowRemoteClients();
            return builder.Build();
        }
    }
}
