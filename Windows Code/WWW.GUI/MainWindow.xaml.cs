using FirstFloor.ModernUI.Windows.Controls;
using NativeWifi;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WildWildWifi.Commons;

namespace WildWildWifi.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        private DispatcherTimer timer;
        private Regex onlyNumberRegex;

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(TotpWifiSettings), typeof(MainWindow), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ServiceStatusProperty = DependencyProperty.Register("ServiceStatus",
            typeof(ServiceStatus), typeof(MainWindow), new FrameworkPropertyMetadata(new ServiceStatus() { Description = "Searching service...", Foreground = new SolidColorBrush(Colors.Red) }));

        private TotpServiceClient client;


        public bool IsConnected
        {
            get
            {
                return this.client != null && this.client.State == CommunicationState.Opened;
            }
        }

        public TotpWifiSettings Settings
        {
            get { return (TotpWifiSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public ServiceStatus ServiceStatus
        {
            get { return (ServiceStatus)GetValue(ServiceStatusProperty); }
            set { SetValue(ServiceStatusProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.onlyNumberRegex = new Regex("[^0-9]+", RegexOptions.Compiled);
            this.timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            this.timer.Tick += Timer_Tick;
            this.Settings = TotpWifiSettings.CreateDefault();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.remainingSecondsProgress.Dispatcher.Invoke(() => { this.remainingSecondsProgress.Value--; });
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDownEvent,
                                                new MouseButtonEventHandler(SelectivelyHandleMouseButton), true);
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
                                                new RoutedEventHandler(SelectAllText), true);

            this.networkAdapterList.Dispatcher.Invoke(() =>
            {
                this.networkAdapterList.Items.Clear();
                this.networkAdapterList.DisplayMemberPath = "InterfaceDescription";
                this.networkAdapterList.SelectedValuePath = "InterfaceGuid";

                WlanClient client = new WlanClient();
                foreach (var item in client.Interfaces)
                {
                    this.networkAdapterList.Items.Add(item);
                }

                if (this.networkAdapterList.HasItems)
                {
                    this.networkAdapterList.SelectedIndex = 0;
                }

            });

            this.UpdateServiceStatus();
            if (this.ServiceStatus.RealStatus == ServiceControllerStatus.Running)
            {
                this.ConnectToService();
                this.LoadServiceSettings();
            }
        }

        private void ConnectToService()
        {
            try
            {
                if (this.client != null)
                {
                    this.client.Close();
                }
            }
            catch (Exception)
            {
            }

            try
            {
                TotpServiceCallback callback = new TotpServiceCallback();
                callback.ServiceCallbackEvent += Callback_ServiceCallbackEvent;
                InstanceContext instanceContext = new InstanceContext(callback);
                this.client = new TotpServiceClient(instanceContext);
                client.Subscribe();
            }
            catch (Exception ex)
            {
            }
        }

        private void OnlyNumbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = onlyNumberRegex.IsMatch(e.Text);
        }

        private void Callback_ServiceCallbackEvent(object sender, ChangePasswordEventArgs e)
        {
            this.timer.Stop();
            this.currentPass.Text = e.Password;
            this.remainingSecondsProgress.Dispatcher.Invoke(() => { this.remainingSecondsProgress.Value = this.remainingSecondsProgress.Maximum = e.NextChange.Subtract(DateTime.UtcNow).TotalSeconds; });
            this.timer.Start();
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (this.client != null && this.client.State == CommunicationState.Opened)
                {
                    this.client.Unsubscribe();
                    this.client.Close();
                }

            }
            catch (Exception)
            {
            }
            this.timer.Stop();

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Settings.IsValid())
            {

                if (!this.IsConnected)
                    this.ConnectToService();

                if (this.IsConnected)
                {
                    this.client.UpdateSettings(this.Settings);
                }
            }
            else
            {
                MessageBox.Show("The provided settings are invalid", "Invalid settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServiceSettings()
        {
            if (!this.IsConnected)
                this.ConnectToService();

            if (this.IsConnected)
            {
                var settings = this.client.ReadSettings();
                if (settings != null)
                {
                    this.Settings = settings;
                }
            }
        }

        private static void SelectivelyHandleMouseButton(object sender, MouseButtonEventArgs e)
        {
            var textbox = (sender as TextBox);
            if (textbox != null && !textbox.IsKeyboardFocusWithin)
            {
                if (e.OriginalSource.GetType().Name == "TextBoxView")
                {
                    e.Handled = true;
                    textbox.Focus();
                }
            }
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private void UpdateServiceStatus()
        {
            ServiceStatus currentStatus = new ServiceStatus();
            currentStatus.RealStatus = 0;
            try
            {
                using (ServiceController controller = new ServiceController(Constants.SERVICE_NAME, "."))
                {
                    currentStatus.RealStatus = controller.Status;
                    switch (currentStatus.RealStatus)
                    {
                        case ServiceControllerStatus.Running:
                        case ServiceControllerStatus.StartPending:
                            currentStatus.Description = "RUNNING";
                            currentStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1CA200"));
                            break;
                        case ServiceControllerStatus.Stopped:
                        case ServiceControllerStatus.StopPending:
                            currentStatus.Description = "STOPPED";
                            currentStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8D0009"));
                            break;
                        default:
                            currentStatus = new ServiceStatus() { Description = "UNDEFINED", Foreground = new SolidColorBrush(Colors.Yellow) };
                            break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException.GetType() == typeof(Win32Exception))
                {
                    Win32Exception exx = (Win32Exception)ex.InnerException;
                    if (exx.NativeErrorCode == 1060)
                    {
                        currentStatus = new ServiceStatus() { Description = "NOT INSTALLED", Foreground = new SolidColorBrush(Colors.Orange) };
                    }
                }
            }
            this.ServiceStatus = currentStatus;
        }

        private void StartService()
        {

            using (ServiceController controller = new ServiceController(Constants.SERVICE_NAME, "."))
            {
                ServiceControllerStatus status = controller.Status;
                if (status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                    this.ConnectToService();
                }
            }
            this.UpdateServiceStatus();
        }

        private void StopService()
        {
            using (ServiceController controller = new ServiceController(Constants.SERVICE_NAME, "."))
            {
                ServiceControllerStatus status = controller.Status;
                if (status == ServiceControllerStatus.Running)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    this.timer.Stop();
                }
            }
            this.UpdateServiceStatus();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.StopService();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartService();
        }
    }
}
