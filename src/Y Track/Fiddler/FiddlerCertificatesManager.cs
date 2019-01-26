using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Y_Track.Fiddler
{
    public static class FiddlerCertificatesManager
    {


        public static bool InstallCertificate()
        {
            CertMaker.createRootCert();
            X509Certificate2 cert = CertMaker.GetRootCertificate();
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, "DO_NOT_TRUST_FiddlerRoot", false);
                foreach (X509Certificate2 i in col)
                {
                    store.Remove(i);
                }
                store.Add(cert);
                CertMaker.trustRootCert();
            }
            catch 
            {
                // TODO :: Show Notification To User that the certificate cannot install
                return false;
            }
            finally
            {
                store.Close();
            }
            return true;
        }

        public static bool UninstallCertificate()
        {
            CertMaker.createRootCert();
            X509Certificate2 cert = CertMaker.GetRootCertificate();
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            X509Store store2 = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, "DO_NOT_TRUST_FiddlerRoot", false);
                foreach (X509Certificate2 i in col)
                {
                    store.Remove(i);
                }
                store2.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);
                X509Certificate2Collection col2 = store2.Certificates.Find(X509FindType.FindByIssuerName, "DO_NOT_TRUST_FiddlerRoot", false);
                foreach (X509Certificate2 i in col2)
                {
                    store2.Remove(i);
                }
            }
            catch 
            {

                return false;
            }
            finally
            {
                store.Close();
                store2.Close();
            }

            return true;
        }


        public static bool IsInstalled()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, "DO_NOT_TRUST_FiddlerRoot", false);
                return col.Count > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                store.Close();
            }
        }


    }
}
