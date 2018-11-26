using System;
using System.Collections.Generic;
using System.ServiceModel;
using WildWildWifi;
using WildWildWifi.Commons;

namespace WildWildWifi.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServiceProxy : ITotpDuplexService
    {
        private List<ITotpCallback> _callbackChannels;
        private object _sycnRoot;

        public event EventHandler<TotpWifiSettings> SettingsUpdatedEvent;

        public bool CanSendCallbacks { get; set; }

        internal string CurrentPassword
        {
            get;
            private set;
        }

        internal DateTime NextChange
        {
            get;
            private set;
        }

        internal TotpWifiSettings Settings
        {
            get;
            set;
        }

        public ServiceProxy()
        {
            this._sycnRoot = new object();
            this._callbackChannels = new List<ITotpCallback>();
            this.CanSendCallbacks = true;
        }

        public void UpdateSettings(TotpWifiSettings settings)
        {
            this.OnSettingsUpdatedEvent(settings);
        }

        public TotpWifiSettings ReadSettings()
        {
            return this.Settings;
        }

        public void Subscribe()
        {
            try
            {
                ITotpCallback callbackChannel = OperationContext.Current.GetCallbackChannel<ITotpCallback>();

                lock (this._sycnRoot)
                {
                    if (!_callbackChannels.Contains(callbackChannel))
                    {
                        _callbackChannels.Add(callbackChannel);
                        if (this.CanSendCallbacks)
                        {
                            callbackChannel.PasswordChanged(this.CurrentPassword, this.NextChange);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void Unsubscribe()
        {
            ITotpCallback callbackChannel = OperationContext.Current.GetCallbackChannel<ITotpCallback>();

            try
            {
                lock (this._sycnRoot)
                {
                    _callbackChannels.Remove(callbackChannel);
                }
            }
            catch
            {
            }
        }

        private void SendCallback()
        {
            if (this.CanSendCallbacks)
            {
                lock (this._sycnRoot)
                {
                    foreach (var item in this._callbackChannels)
                    {
                        try
                        {
                            item.PasswordChanged(this.CurrentPassword, this.NextChange);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void OnSettingsUpdatedEvent(TotpWifiSettings state)
        {
            EventHandler<TotpWifiSettings> handler = this.SettingsUpdatedEvent;

            if (handler != null)
            {
                handler(this, state);
            }
        }

        public void UpdateValues(string newPassword, DateTime nextChange)
        {
            this.CurrentPassword = newPassword;
            this.NextChange = nextChange;
            this.SendCallback();
        }
    }
}
