using CommandLine;

namespace WildWildWifi.Service
{
    internal class ServiceOptions
    {
        [Option('i', "install", Required = false, HelpText = "Install service.", SetName = "install")]
        public bool serviceInstall { get; set; }

        [Option('u', "uninstall", Required = false, HelpText = "Uninstall service.", SetName = "uninstall")]
        public bool serviceUninstall { get; set; }

        [Option('d', "debug", Required = false, HelpText = "Debug service.", SetName = "debug")]
        public bool serviceDebug { get; set; }
    }
}
