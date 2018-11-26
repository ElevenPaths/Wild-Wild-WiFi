using CommandLine;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using WildWildWifi;

namespace WildWildWifi.Service
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Parser parser = new Parser((e) =>
            {
                e.CaseSensitive = false;
                e.CaseInsensitiveEnumValues = true;
                e.EnableDashDash = true;
                e.HelpWriter = Console.Out;
            });

            try
            {
                parser.ParseArguments<ServiceOptions>(args).MapResult(
                     (ServiceOptions opts) => Run(opts),
                      errs => 1);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private static int Run(ServiceOptions options)
        {
            WifiTotpService service = new WifiTotpService();
            if (options.serviceInstall)
            {
                ServiceController controller = null;
                try
                {
                    controller = new ServiceController(service.ServiceName, ".");
                    ServiceControllerStatus status = controller.Status;
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.InnerException.GetType() == typeof(Win32Exception))
                    {
                        Win32Exception exx = (Win32Exception)ex.InnerException;
                        if (exx.NativeErrorCode != 1060)
                        {
                            Trace.TraceError(ex.Message);
                        }
                    }
                    else
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }

                try
                {
                    IntegratedServiceInstaller Inst = new IntegratedServiceInstaller();
                    Inst.Install(service.ServiceName, service.ServiceName, service.ServiceName, ServiceAccount.LocalSystem, ServiceStartMode.Automatic);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    return 1;
                }

                try
                {
                    controller = new ServiceController(service.ServiceName, ".");
                    ServiceControllerStatus status = controller.Status;
                    if (status == ServiceControllerStatus.Stopped)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.InnerException.GetType() == typeof(Win32Exception))
                    {
                        Win32Exception w32ex = (Win32Exception)ex.InnerException;
                        if (w32ex.NativeErrorCode == 1060)
                        {
                            Trace.TraceError("The service cannot be installed");
                            return 1;
                        }
                    }
                    else
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }

                Trace.TraceInformation(service.ServiceName + " installed.");

                return 0;
            }
            else if (options.serviceUninstall)
            {
                IntegratedServiceInstaller Inst = new IntegratedServiceInstaller();
                Inst.Uninstall(service.ServiceName);
                Trace.TraceInformation(service.ServiceName + " uninstalled.");
                return 0;
            }
            else if (options.serviceDebug)
            {
                service.Debug();
                return 0;
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    service
                };
                ServiceBase.Run(ServicesToRun);

                return 0;
            }
        }
    }
}
