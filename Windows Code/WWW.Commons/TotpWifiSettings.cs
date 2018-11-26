using System;
using System.Text.RegularExpressions;

namespace WildWildWifi
{
    [Serializable]
    public class TotpWifiSettings
    {
        private static readonly Regex base32Regex = new Regex("^[A-Z2-7]+=?=?=?$", RegexOptions.Compiled);

        public string SecretKeyBase32 { get; set; }

        public string PSHK { get; set; }

        public string ESSID { get; set; }

        public int StepSeconds { get; set; }

        public int TotpDigitCount { get; private set; }

        public Guid WlanInterfaceId { get; set; }

        public bool AutoConnect { get; private set; }

        public TotpWifiSettings(string secretKey, string pshk, int seconds, string essid, Guid wlanId, bool autoconnect)
        {
            this.SecretKeyBase32 = secretKey;
            this.StepSeconds = seconds;
            this.ESSID = essid;
            this.WlanInterfaceId = wlanId;
            this.AutoConnect = autoconnect;
            this.TotpDigitCount = 8;
            this.PSHK = pshk;
        }

        public static TotpWifiSettings CreateDefault()
        {
            return new TotpWifiSettings("Insert secret here", "Your PSHK", 60, "SSID Name", Guid.Empty, true);
        }

        public override string ToString()
        {
            return String.Join("\r\n", String.Concat(nameof(this.ESSID), "=", this.ESSID),
                                       String.Concat(nameof(this.SecretKeyBase32), "=", this.SecretKeyBase32),
                                       String.Concat(nameof(this.PSHK), "=", this.PSHK),
                                       String.Concat(nameof(this.StepSeconds), "=", this.StepSeconds));
        }

        public bool IsValid()
        {
            return !String.IsNullOrWhiteSpace(this.PSHK) && this.PSHK.Length >= 10 &&
                    !String.IsNullOrWhiteSpace(this.SecretKeyBase32) && base32Regex.IsMatch(this.SecretKeyBase32) && this.SecretKeyBase32.Length % 4 == 0 &&
                    this.StepSeconds > 10 &&
                    !String.IsNullOrWhiteSpace(this.ESSID) &&
                    !this.WlanInterfaceId.Equals(Guid.Empty);
        }
    }
}
