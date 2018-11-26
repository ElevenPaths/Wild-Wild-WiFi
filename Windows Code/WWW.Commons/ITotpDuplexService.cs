using System.ServiceModel;

namespace WildWildWifi.Commons
{
    [ServiceContract(CallbackContract = typeof(ITotpCallback))]
    public interface ITotpDuplexService
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe();

        [OperationContract(IsOneWay = true)]
        void UpdateSettings(TotpWifiSettings settings);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe();

        [OperationContract(IsOneWay = false)]
        TotpWifiSettings ReadSettings();
    }
}
