using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WildWildWifi.Commons;

namespace WildWildWifi.GUI
{
    internal class TotpServiceClient
            : DuplexClientBase<ITotpDuplexService>, ITotpDuplexService
    {
        public TotpServiceClient(InstanceContext callbackInstance)
            : base(callbackInstance)
        {
        }

        public void Subscribe()
        {
            Channel.Subscribe();
        }

        public void UpdateSettings(TotpWifiSettings settings)
        {
            Channel.UpdateSettings(settings);
        }

        public void Unsubscribe()
        {
            Channel.Unsubscribe();
        }

        public TotpWifiSettings ReadSettings()
        {
            return Channel.ReadSettings();
        }
    }
}
