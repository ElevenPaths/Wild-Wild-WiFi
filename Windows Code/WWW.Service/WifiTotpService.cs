using NativeWifi;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WildWildWifi;

namespace WildWildWifi.Service
{
    internal partial class WifiTotpService : ServiceBase
    {
        private TotpWifiSettings settings;
        private CancellationTokenSource serviceCancelSource;
        private CancellationTokenSource currentConfigCancelSource;
        private readonly Regex profileRegex = new Regex("<protected>.*</protected>\r\n.*<keyMaterial>.*</keyMaterial>", RegexOptions.Compiled);

        public WifiTotpService()
        {
            InitializeComponent();
            this.serviceCancelSource = new CancellationTokenSource();
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(p => Run(), TaskCreationOptions.LongRunning);
        }

        protected override void OnStop()
        {
            this.serviceCancelSource.Cancel();
            this.currentConfigCancelSource.Cancel();
        }

        public void Debug()
        {
            Run();
        }

        private void LoadConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string wlanId = config.AppSettings.Settings.ReadOrDefault<string>(nameof(TotpWifiSettings.WlanInterfaceId));
            TotpWifiSettings savedSettings = new TotpWifiSettings(
             config.AppSettings.Settings.ReadOrDefault<string>(nameof(TotpWifiSettings.SecretKeyBase32)),
             config.AppSettings.Settings.ReadOrDefault<string>(nameof(TotpWifiSettings.PSHK)),
             config.AppSettings.Settings.ReadOrDefault<int>(nameof(TotpWifiSettings.StepSeconds)),
             config.AppSettings.Settings.ReadOrDefault<string>(nameof(TotpWifiSettings.ESSID)),
             String.IsNullOrWhiteSpace(wlanId) ? Guid.Empty : Guid.Parse(wlanId),
             config.AppSettings.Settings.ReadOrDefault<bool>(nameof(TotpWifiSettings.AutoConnect)));

            if (savedSettings.IsValid())
            {
                this.settings = savedSettings;
            }
        }

        private void Run()
        {
            this.LoadConfig();
            this.currentConfigCancelSource = new CancellationTokenSource();

            ServiceProxy coreService;
            try
            {
                coreService = new ServiceProxy() { CanSendCallbacks = false };
                ServiceHost host = new ServiceHost(coreService);
                host.Open();
                coreService.SettingsUpdatedEvent += CoreService_SettingsUpdatedEvent;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }

            do
            {
                if (this.settings != null)
                {
                    coreService.Settings = this.settings;
                    coreService.CanSendCallbacks = true;
                    Trace.TraceInformation(this.settings.ToString());
                    this.currentConfigCancelSource = new CancellationTokenSource();
                    WlanClient client = new WlanClient();
                    WlanClient.WlanInterface wlanIface = client.Interfaces.SingleOrDefault(p => p.InterfaceGuid.Equals(this.settings.WlanInterfaceId));
                    if (wlanIface == null)
                    {
                        throw new ArgumentException("The provided wlan interface id does not exist.");
                    }

                    byte[] otpKey = Base32.Base32Encoder.Decode(this.settings.SecretKeyBase32);
                    OtpSharp.Totp otpProvider = new OtpSharp.Totp(otpKey, this.settings.StepSeconds, totpSize: this.settings.TotpDigitCount);

                    WLANProfile defaultProfile = WLANProfile.Default(this.settings.ESSID);
                    if (wlanIface.GetProfiles().Any(p => p.profileName.Equals(this.settings.ESSID)))
                    {
                        wlanIface.DeleteProfile(this.settings.ESSID);
                    }

                    XmlSerializer xmlSer = new XmlSerializer(typeof(WLANProfile));
                    string textProfile = String.Empty;
                    using (StringWriter writer = new StringWriter())
                    {
                        xmlSer.Serialize(writer, defaultProfile);
                        textProfile = writer.ToString();
                    }

                    DateTime currentDate;
                    DateTime nextChange;
                    nextChange = currentDate = DateTime.UtcNow;

                    SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
                    string pskhash = BitConverter.ToString(sha1Provider.ComputeHash(Encoding.ASCII.GetBytes(this.settings.PSHK))).Replace("-", "").ToLower();

                    do
                    {
                        try
                        {
                            double sleepSeconds = 0.1;
                            if (currentDate >= nextChange)
                            {
                                try
                                {
                                    //Generate key
                                    string otp = otpProvider.ComputeTotp(currentDate);
                                    string totphash = BitConverter.ToString(sha1Provider.ComputeHash(Encoding.ASCII.GetBytes(otp))).Replace("-", "").ToLower();
                                    string newKey = BitConverter.ToString(sha1Provider.ComputeHash(Encoding.ASCII.GetBytes(totphash + pskhash))).Replace("-", "").ToLower();
                                    Trace.TraceInformation(otp + " - " + newKey);
                                    //if (wlanIface.CurrentConnection.profileName.Equals(networkName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        string newProf = profileRegex.Replace(textProfile, $"<protected>false</protected><keyMaterial>{newKey}</keyMaterial>");
                                        wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, newProf, true);
                                        if (this.settings.AutoConnect)
                                        {
                                            //wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, defaultProfile.Name);
                                            wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, defaultProfile.Name);
                                        }
                                    }

                                    int desync = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds % this.settings.StepSeconds;
                                    nextChange = DateTime.UtcNow.AddSeconds(this.settings.StepSeconds - desync);

                                    Task.Factory.StartNew(() => coreService.UpdateValues(newKey, nextChange));
                                    sleepSeconds = this.settings.StepSeconds - desync - 1;
                                    Trace.TraceInformation("Next change: " + nextChange.ToString("T"));
                                }
                                catch (Exception e)
                                {
                                    Trace.TraceError(e.ToString());
                                }
                            }

                            //Task.Delay(TimeSpan.FromSeconds(sleepSeconds), this.currentConfigCancelSource.Token).Wait();
                            this.currentConfigCancelSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(sleepSeconds));
                            currentDate = DateTime.UtcNow;
                        }
                        catch (AggregateException)
                        { }
                    } while (!this.currentConfigCancelSource.IsCancellationRequested);
                    sha1Provider.Dispose();
                }
                else
                {
                    coreService.CanSendCallbacks = false;
                    Trace.TraceInformation("Waiting for a valid settings");
                    //Task.Delay(TimeSpan.FromSeconds(10), this.currentConfigCancelSource.Token).Wait();
                    this.currentConfigCancelSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(60));
                }
            } while (!this.serviceCancelSource.IsCancellationRequested);
        }

        private void CoreService_SettingsUpdatedEvent(object sender, TotpWifiSettings e)
        {
            try
            {
                //var x = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                if (e.IsValid())
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.AutoConnect), e.AutoConnect.ToString());
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.ESSID), e.ESSID);
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.PSHK), e.PSHK);
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.SecretKeyBase32), e.SecretKeyBase32);
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.StepSeconds), e.StepSeconds.ToString());
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.TotpDigitCount), e.TotpDigitCount.ToString());
                    config.AppSettings.Settings.AddOrUpdate(nameof(TotpWifiSettings.WlanInterfaceId), e.WlanInterfaceId.ToString());

                    config.Save(ConfigurationSaveMode.Full);


                    this.settings = e;
                    this.currentConfigCancelSource.Cancel(false);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }
}
