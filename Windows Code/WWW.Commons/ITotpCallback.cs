using System;
using System.ServiceModel;

namespace WildWildWifi.Commons
{
    public interface ITotpCallback
    {
        [OperationContract(IsOneWay = true)]
        void PasswordChanged(string newPassword, DateTime nextChange);
    }
}
