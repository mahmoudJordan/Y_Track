using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

namespace Y_Track.Titanium
{
    public class TitaniumManager
    {
        /// <summary>
        /// reports proxy status chaning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ProxyStatusChanged(object sender, ProxyStatusEventsArguments e);
        public event ProxyStatusChanged OnUpdateStatus;

        /// <summary>
        /// singiliton holder
        /// </summary>
        private static TitaniumManager _instance;

        /// <summary>
        /// proxy instance
        /// </summary>
        public ProxyServer Proxy { get; private set; }
        private ExplicitProxyEndPoint explicitProxyEndPoint;
        public static TitaniumManager Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new TitaniumManager();
                    _instance.Proxy = new ProxyServer(true);
                    _instance.explicitProxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 0, true);
                    _instance.Proxy.AddEndPoint(_instance.explicitProxyEndPoint);
                    _instance.Proxy.CertificateManager.TrustRootCertificate(true);
                 
                }
                return _instance;
            }
        }


        /// <summary>
        /// protected constructor to prevent initialize a new instance from current class
        /// </summary>
        protected TitaniumManager() { }

        /// <summary>
        /// Start the proxy and free up resources
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start(int runningPort)
        {
            return await Task.Run(() =>
            {

                try
                {
                    Proxy.Start();
                    Proxy.SetAsSystemHttpProxy(_instance.explicitProxyEndPoint);
                    Proxy.SetAsSystemHttpsProxy(_instance.explicitProxyEndPoint);
                    OnUpdateStatus?.Invoke(this, new ProxyStatusEventsArguments()
                    {
                        Status = ProxyStatus.Started
                    });
                    return true;
                }
                catch (Exception e)
                {
                    Helpers.YTrackLogger.Log("Could not start Titanium : " + e.Message + "\n\n" + e.StackTrace);
                    return false;
                }
            });
        }

        /// <summary>
        /// stop the proxy and free up resources
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Proxy.Stop();
                    OnUpdateStatus?.Invoke(this, new ProxyStatusEventsArguments()
                    {
                        Status = ProxyStatus.Stopped
                    });
                    return true;
                }
                catch (Exception e)
                {
                    Helpers.YTrackLogger.Log("Could not stop Titanium : " + e.Message + "\n\n" + e.StackTrace);
                    return false;
                }
            });
        }



    }
}
