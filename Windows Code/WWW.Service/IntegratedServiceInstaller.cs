using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace WildWildWifi.Service
{
    internal class IntegratedServiceInstaller
    {
        #region Public Methods

        public void Install(string ServiceName, string DisplayName, string Description,
            string InstanceID, System.ServiceProcess.ServiceAccount Account, System.ServiceProcess.ServiceStartMode StartMode)
        {
            System.ServiceProcess.ServiceProcessInstaller ProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            ProcessInstaller.Account = Account;

            System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

            System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext();
            string processPath = Process.GetCurrentProcess().MainModule.FileName;
            if (processPath != null && processPath.Length > 0)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(processPath);

                string path = string.Format("/assemblypath={0}", fi.FullName);
                string[] cmdline = { path };
                Context = new System.Configuration.Install.InstallContext("", cmdline);
            }

            SINST.Context = Context;
            SINST.DisplayName = string.Format("{0} - {1}", DisplayName, InstanceID);
            SINST.Description = string.Format("{0} - {1}", Description, InstanceID);
            SINST.ServiceName = string.Format("{0}_{1}", ServiceName, InstanceID);
            SINST.StartType = StartMode;
            SINST.Parent = ProcessInstaller;

            SINST.ServicesDependedOn = new string[] { "RpcSs" };

            System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
            SINST.Install(state);

            using (RegistryKey oKey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\Services\{0}_{1}", ServiceName, InstanceID), true))
            {
                try
                {
                    Object sValue = oKey.GetValue("ImagePath");
                    oKey.SetValue("ImagePath", sValue);
                }
                catch (Exception Ex)
                {
                    Trace.TraceError(Ex.Message);
                }
            }
        }

        public void Install(string ServiceName, string DisplayName, string Description, System.ServiceProcess.ServiceAccount Account, System.ServiceProcess.ServiceStartMode StartMode)
        {
            System.ServiceProcess.ServiceProcessInstaller ProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            ProcessInstaller.Account = Account;

            System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

            System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext();
            string processPath = Process.GetCurrentProcess().MainModule.FileName;
            if (processPath != null && processPath.Length > 0)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(processPath);

                string path = string.Format("/assemblypath={0}", fi.FullName);
                string[] cmdline = { path };
                Context = new System.Configuration.Install.InstallContext("", cmdline);
            }

            SINST.Context = Context;
            SINST.DisplayName = DisplayName;
            SINST.Description = Description;
            SINST.ServiceName = ServiceName;
            SINST.StartType = StartMode;
            SINST.Parent = ProcessInstaller;
            SINST.ServicesDependedOn = new string[] { "RpcSs" };

            System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
            SINST.Install(state);

            using (RegistryKey oKey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\Services\{0}", ServiceName), true))
            {
                try
                {
                    Object sValue = oKey.GetValue("ImagePath");
                    oKey.SetValue("ImagePath", sValue);
                }
                catch (Exception Ex)
                {
                    Trace.TraceError(Ex.Message);
                }
            }
        }

        public void Uninstall(string ServiceName, string InstanceID)
        {
            System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

            System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext();
            SINST.Context = Context;
            SINST.ServiceName = string.Format("{0}_{1}", ServiceName, InstanceID);
            SINST.Uninstall(null);
        }

        public void Uninstall(string ServiceName)
        {
            System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

            System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext();
            SINST.Context = Context;
            SINST.ServiceName = string.Format("{0}", ServiceName);
            SINST.Uninstall(null);
        }

        #endregion Public Methods
    }
}
