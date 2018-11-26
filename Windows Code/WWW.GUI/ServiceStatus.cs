using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WildWildWifi.GUI
{
    public class ServiceStatus
    {
        public string Description { get; set; }
        public Brush Foreground { get; set; }
        public bool NeedsPermission { get; private set; }

        public ServiceControllerStatus RealStatus { get; set; }

        public ServiceStatus()
        {
            this.NeedsPermission = !WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
        }

        public bool CanStart
        {
            get
            {
                return !this.NeedsPermission && this.RealStatus == ServiceControllerStatus.Stopped;
            }
        }

        public bool CanStop
        {
            get
            {
                return !this.NeedsPermission && this.RealStatus == ServiceControllerStatus.Running;
            }
        }
    }
}
