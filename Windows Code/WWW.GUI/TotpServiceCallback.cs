using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WildWildWifi.Commons;

namespace WildWildWifi.GUI
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class TotpServiceCallback : ITotpCallback
    {
        private SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;
        public event EventHandler<ChangePasswordEventArgs> ServiceCallbackEvent;

        public void PasswordChanged(string newPassword, DateTime nextChange)
        {
            _syncContext.Post(new SendOrPostCallback(OnServiceCallbackEvent),
                new ChangePasswordEventArgs(newPassword, nextChange));
        }

        private void OnServiceCallbackEvent(object state)
        {
            EventHandler<ChangePasswordEventArgs> handler = ServiceCallbackEvent;
            ChangePasswordEventArgs e = state as ChangePasswordEventArgs;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    internal class ChangePasswordEventArgs : EventArgs
    {
        public string Password
        {
            get;
            set;
        }

        public DateTime NextChange
        {
            get;
            set;
        }

        public ChangePasswordEventArgs(string password, DateTime nextChange)
        {
            this.Password = password;
            this.NextChange = nextChange;
        }
    }
}
