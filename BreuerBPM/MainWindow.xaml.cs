using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Timers;
using System.Text.RegularExpressions;
using System.Data;

namespace BreuerBPM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //retrieves surveorInfo stored by sample manager
            initialiseSurveyorInfo();

            //This timer lets us retrieve the absolute final value. It needs to be set here in order for global varibales to act accordingly, otherwise
            //they are cleared after the final result data-check.
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            //This timer enforced the 1 minute delay/countdown between measurements
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer2_Tick);

            InitialiseUIThings();

            //starts looking for BPM BT device
            StartBleDeviceWatcher();

        }

        #region UI
        public void InitialiseUIThings()
        {
            //Features to be hidden/disabled on initialisation
            clear1.IsEnabled = false;
            clear2.IsEnabled = false;
            clear3.IsEnabled = false;
            save1.Visibility = Visibility.Hidden;
            save2.Visibility = Visibility.Hidden;
            save3.Visibility = Visibility.Hidden;
            NextMeasurementIn.Visibility = Visibility.Hidden;
            CounterLabel.Visibility = Visibility.Hidden;
            MinuteDelayPrompt.Visibility = Visibility.Hidden;
            SYS1_manual.Visibility = Visibility.Hidden;
            SYS2_manual.Visibility = Visibility.Hidden;
            SYS3_manual.Visibility = Visibility.Hidden;
            DIA1_manual.Visibility = Visibility.Hidden;
            DIA2_manual.Visibility = Visibility.Hidden;
            DIA3_manual.Visibility = Visibility.Hidden;
            PUL1_manual.Visibility = Visibility.Hidden;
            PUL2_manual.Visibility = Visibility.Hidden;
            PUL3_manual.Visibility = Visibility.Hidden;
        }


        //Update UI to display connection status
        public void updateConnectionStatus(string text)
        {
            if (text == "Ready For Measurement")
            {
                Application.Current.Dispatcher.Invoke(() => { Connectionstatus.Text = text; Connectionstatus.Foreground = Brushes.Green; });
            }
            if (text == "Disconnected")
            {
                Application.Current.Dispatcher.Invoke(() => { Connectionstatus.Text = text; Connectionstatus.Foreground = Brushes.Black; });
            }
            if (text == "Awaiting Countdown")
            {
                Application.Current.Dispatcher.Invoke(() => { Connectionstatus.Text = text; Connectionstatus.Foreground = Brushes.OrangeRed; });
            }
            if (text == "Manual Measurement")
            {
                Application.Current.Dispatcher.Invoke(() => { Connectionstatus.Text = text; Connectionstatus.Foreground = Brushes.Gray; });
            }

        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            //CSV conversion must go here with appropriate handling. Currently checking for decimal point in measurement

            try
            {
                if (true)
                {

                }

                else
                {
                    //A decimal point is not present. BT transmission always sends a decimal point.
                    MessageBox.Show("Incorrect weight format. \n\nPlease ensure you've collected results using Salter Scales.\n\n" +
                        "If entering manually, ensure the measurement is exactly what is shown on scales.\n\n" +
                        "The measurement expected is 1 decimal place. For example 70 kg must be input as 70.0");
                }
            }
            catch
            {   //array is indexed in appropriately, therefore null measurement or uncorrect measurement format
                MessageBox.Show("Please enter some measurements and ensure you've collected results using Salter Scales.\n\n" +
                   "If entering manually, ensure the measurement is exactly what is shown on scales.\n\n" +
                       "The measurement expected is 1 decimal place. For example 70 kg must be input as 70.0");
            }
        }


        bool manualMeasurement = false;
        bool regexOverride = false;//allows usage of text box clear operations to delte old results by not having regex applied to user input
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            regexOverride = true;
            manualMeasurement = true;
            updateConnectionStatus("Manual Measurement");
            MessageBox.Show("You are now entering measurements manually.\n\n" +
                "Please ensure measurements reflect that of the Blood Pressure Monitor.");
            //////
            RunCleanUp();
            ///////
            regexOverride = false;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            allMeasurements.Clear();
            initialiseSurveyorInfo(); //re-initialise array for a return to BT input.
            regexOverride = true;
            manualMeasurement = false;
            MessageBox.Show("You are now entering measurements with Bluetooth.");
            //////
            RunCleanUp();
            ////////
            regexOverride = false;
        }

        //Clearing measurements from indivudal fields
        bool clear1WasClicked = false;
        string measurementField = "";
        private void clear1_Click(object sender, RoutedEventArgs e)
        {
            clear1WasClicked = true; //setting this to true allows us to override the set up warnings when clearing first BT measurement. It must be set to false where measurement added to array.
            measurementField = "measurement1";
            regexOverride = true;
            if (manualMeasurement == true)
            {

            }
            else
            {
                clear2.IsEnabled = false;
                clear3.IsEnabled = false;
                ClearMeasurement(measurementField);
            }
            regexOverride = false;
        }

        bool clear2WasClicked = false;
        private void clear2_Click(object sender, RoutedEventArgs e)
        {
            clear2WasClicked = true;
            measurementField = "measurement2";
            regexOverride = true;
            if (manualMeasurement == true)
            {

            }
            else
            {
                //allMeasurements.Clear();
                clear1.IsEnabled = false;
                clear3.IsEnabled = false;
                ClearMeasurement(measurementField);

            }
            regexOverride = false;
        }

        bool clear3WasClicked = false;
        private void clear3_Click(object sender, RoutedEventArgs e)
        {
            clear3WasClicked = true;
            measurementField = "measurement3";
            regexOverride = true;
            if (manualMeasurement == true)
            {

            }
            else
            {
                clear1.IsEnabled = false;
                clear2.IsEnabled = false;
                ClearMeasurement(measurementField);
            }
            regexOverride = false;
        }

        public void ClearMeasurement(string measurementField)
        {
            switch (measurementField)
            {
                case "measurement1":
                    Application.Current.Dispatcher.Invoke(() => { SYS1.Text = "empty"; DIA1.Text = "empty"; PUL1.Text = "empty"; });
                    break;
                case "measurement2":
                    Application.Current.Dispatcher.Invoke(() => { SYS2.Text = "empty"; DIA2.Text = "empty"; PUL2.Text = "empty"; });
                    break;
                case "measurement3":
                    Application.Current.Dispatcher.Invoke(() => { SYS3.Text = "empty"; DIA3.Text = "empty"; PUL3.Text = "empty"; });
                    break;

            }
        }


        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            finalMeasurementsList.Clear();
            allMeasurements.Clear();
            ClearMeasurement("measurement1"); ClearMeasurement("measurement2"); ClearMeasurement("measurement3");
            clear1.IsEnabled = false;
            clear2.IsEnabled = false;
            clear3.IsEnabled = false;
            field1AtleastOneMeasurement = false;
            field2AtleastOneMeasurement = false;
            field3AtleastOneMeasurement = false;
        }

        private void save3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void save2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void save1_Click(object sender, RoutedEventArgs e)
        {

        }


        //Handles all manual measurement boxes with one event and one regex. Permits only backspaces and numeric.
        private void measurement_manual_input(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextAllowed(e.Text);
        }

        private static readonly Regex _regex = new Regex("[0-9\b]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }


        //This is run when setting manualMeasurement on or off via checkbox. Clears all fields and re-sets for taking 1st measurement.
        public void RunCleanUp()
        {
            //reset all measruements
            allMeasurements.Clear();


            if (manualMeasurement == true) //Enable the manualMeasurement == true button to perform submission calcs using the manually entered measurements and not timer entered measurements.
            {
                finalMeasurementsList.Clear();
                SYS1_manual.Visibility = Visibility.Visible;
                SYS2_manual.Visibility = Visibility.Visible;
                SYS3_manual.Visibility = Visibility.Visible;
                DIA1_manual.Visibility = Visibility.Visible;
                DIA2_manual.Visibility = Visibility.Visible;
                DIA3_manual.Visibility = Visibility.Visible;
                PUL1_manual.Visibility = Visibility.Visible;
                PUL2_manual.Visibility = Visibility.Visible;
                PUL3_manual.Visibility = Visibility.Visible;
                save1.Visibility = Visibility.Visible;
                save2.Visibility = Visibility.Visible;
                save3.Visibility = Visibility.Visible;

            }
            else //Bluetooth measuring so setting initial button again.
            {
                finalMeasurementsList.Clear();
                SYS1_manual.Visibility = Visibility.Hidden;
                SYS2_manual.Visibility = Visibility.Hidden;
                SYS3_manual.Visibility = Visibility.Hidden;
                DIA1_manual.Visibility = Visibility.Hidden;
                DIA2_manual.Visibility = Visibility.Hidden;
                DIA3_manual.Visibility = Visibility.Hidden;
                PUL1_manual.Visibility = Visibility.Hidden;
                PUL2_manual.Visibility = Visibility.Hidden;
                PUL3_manual.Visibility = Visibility.Hidden;
                save1.Visibility = Visibility.Hidden;
                save2.Visibility = Visibility.Hidden;
                save3.Visibility = Visibility.Hidden;

            }


            //Previous input used in Regex expressions for only allowing certain char input. Clearing these avoids duplication of previous inout values.
            /*previousInput = "";
            previousInput1 = "";
            previousInput2 = "";*/
        }

        #endregion

        #region DeviceDiscovery

        private ObservableCollection<BluetoothLEDeviceDisplay> KnownDevices = new ObservableCollection<BluetoothLEDeviceDisplay>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();

        private DeviceWatcher deviceWatcher;
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            //deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            //deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            KnownDevices.Clear();

            // Start the watcher. Active enumeration is limited to approximately 30 seconds.
            // This limits power usage and reduces interference with other Bluetooth activities.
            // To monitor for the presence of Bluetooth LE devices for an extended period,
            // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
            // sample for an example.
            deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                //deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                //deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in UnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            await Task.Run(async () =>
            {
                lock (this)
                {


                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure device isn't already present in the list.
                        if (FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != string.Empty)
                            {
                                // If device has a friendly name display it immediately.
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                            }
                            else
                            {
                                // Add it to a list in case the name gets updated later. 
                                UnknownDevices.Add(deviceInfo);
                            }
                        }

                    }
                }
            });

        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {

            //if contains salter and salter is connectable stop all other handlers and connect  
            await Task.Run(async () =>
            {
                lock (this)
                {


                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            DeviceInformation updatedDevice = bleDeviceDisplay.DeviceInformation;
                            //IsConnectable will be established once updated accordingly here. So function needs to be added that handles all devices.
                            ConnectToSALTERDevice(updatedDevice, bleDeviceDisplay);
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            deviceInfo.Update(deviceInfoUpdate);
                            // If device has been updated with a friendly name it's no longer unknown.
                            if (deviceInfo.Name != String.Empty)
                            {
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                                UnknownDevices.Remove(deviceInfo);
                            }
                        }
                    }
                }
            });

        }


        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Task.Run(async () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    StartBleDeviceWatcher();
                }
            });
        }



        public class BluetoothLEDeviceDisplay : INotifyPropertyChanged
        {
            public BluetoothLEDeviceDisplay(DeviceInformation deviceInfoIn)
            {
                DeviceInformation = deviceInfoIn;

            }

            public DeviceInformation DeviceInformation { get; private set; }

            public string Id => DeviceInformation.Id;
            public string Name => DeviceInformation.Name;
            public bool IsPaired => DeviceInformation.Pairing.IsPaired;
            public bool IsConnected => (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
            public bool IsConnectable => (bool?)DeviceInformation.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;

            public IReadOnlyDictionary<string, object> Properties => DeviceInformation.Properties;



            public event PropertyChangedEventHandler PropertyChanged;

            public void Update(DeviceInformationUpdate deviceInfoUpdate)
            {
                DeviceInformation.Update(deviceInfoUpdate);

                OnPropertyChanged("Id");
                OnPropertyChanged("Name");
                OnPropertyChanged("DeviceInformation");
                OnPropertyChanged("IsPaired");
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("Properties");
                OnPropertyChanged("IsConnectable");

            }


            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region DeviceConnection

        private BluetoothLEDevice bluetoothLeDevice;


        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    //selectedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            return true;
        }

        private bool subscribedForNotifications = false;

        private async void ConnectToSALTERDevice(DeviceInformation device, BluetoothLEDeviceDisplay devDisplay)
        {


            if (!await ClearBluetoothLEDeviceAsync())
            {
                //Error for unable to reset state;
                return;
            }
            if (device.Name.Contains("BM57") && devDisplay.IsConnectable == true)
            {
                StopBleDeviceWatcher(); //Device found and connectable so stop watching
                try
                {
                    // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                    bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(devDisplay.Id);

                    if (bluetoothLeDevice == null)
                    {

                    }
                }
                catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
                {
                    //Notify that device is not available
                }

                if (bluetoothLeDevice != null)
                {
                    // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                    // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                    // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                    GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////SPECIFIC TO SALTER DEVICE. NEEDS TO BE CUSTOMISED FOR EACH DEVICE SERVICE/CHARACTERISTIC
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        Guid serviceGUID;
                        var services = result.Services;
                        foreach (var service in services)
                        {
                            string servicename = DisplayHelpers.GetServiceName(service);

                            if (servicename == "BloodPressure")
                            {
                                var SALTERservice = service;
                                serviceGUID = service.Uuid;
                                ConnectToService(SALTERservice);
                                break;
                            }
                        }
                    }
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    else
                    {
                        StartBleDeviceWatcher();
                    }
                }
            }


        }

        #endregion

        #region Data Retrieval

        private GattCharacteristic selectedCharacteristic;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;


        private async void ConnectToService(GattDeviceService wantedservice)
        {
            var service = wantedservice;

            RemoveValueChangedHandler();

            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {

                    var result2 = await service.GetCharacteristicsAsync(BluetoothCacheMode.Cached);//Allows access to device even if not broadcasting signal, detects from cache.
                    if (result2.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result2.Characteristics;
                    }
                    else
                    {
                        //error accessing individual service

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    //access isn't granted

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                //another restricted service error
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // SPECIFIC TO BPM DEVICE. THESE NEED TO BE CUSTOMISED FOR EACH DEVICE
            foreach (GattCharacteristic c in characteristics)
            {
                Guid characteristicGUID;
                string characteristicname = DisplayHelpers.GetCharacteristicName(c);

                if (characteristicname == "BloodPressureMeasurement")//The characteristic used to transfer data from measurements from scales
                {
                    var SALTERcharacteristic = c;
                    characteristicGUID = c.Uuid;
                    selectedCharacteristic = SALTERcharacteristic;
                    SubscribeToCharacteristic();
                    break;
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        //Subscribe to the "BloodPressureMeasurement" characteristic
        private async void SubscribeToCharacteristic()
        {

            if (!subscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }

                else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                }

                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success || status == GattCommunicationStatus.Unreachable) //Because connecting via BT cached mode, unreachable is fine.
                    {


                        //success in subscribing for value change. show alert here to user
                        //((Window.Current.Content as Frame).Content as MainPage).SetConnectionStatus("Connected");
                        updateConnectionStatus("Ready For Measurement");

                        //This thread sleep is important, it lets the device connect without detecting a characteristic value changed event (blood pressure reading) in the connection advertisement.
                        Thread.Sleep(500);

                        AddValueChangedHandler();//user now knows successful connection, so can begin taking measurements which are detected by AddValueChangedHandler (Turns on characteristic_valuechanged event)

                    }
                    else
                    {
                        //error registering for value changes
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    //unexpected error
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        subscribedForNotifications = false;
                        RemoveValueChangedHandler();
                        //un-registered for BT notifications (value changes)
                    }
                    else
                    {
                        //error un-registering for BT notifications
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    //error handling here
                }
            }
        }


        //set up the csv for storing measurements. individual csv file per measurement, not a master file.
        string[,] arrayMeasurements = new string[4, 7];
        private void initialiseSurveyorInfo()
        {
            arrayMeasurements[0, 0] = "MeasureType";
            arrayMeasurements[0, 1] = "Measurement";
            arrayMeasurements[0, 2] = "Qtr";
            arrayMeasurements[0, 3] = "MB";
            arrayMeasurements[0, 4] = "HHID";
            arrayMeasurements[0, 5] = "RespondentID";
            arrayMeasurements[0, 6] = "MeasurementInputType";
            string[] respondentInfo = GetRespondentIdentifiers();
            arrayMeasurements[1, 2] = respondentInfo[0];
            arrayMeasurements[1, 3] = respondentInfo[1];
            arrayMeasurements[1, 4] = respondentInfo[2];
            arrayMeasurements[1, 5] = respondentInfo[3];
            arrayMeasurements[2, 2] = respondentInfo[0];
            arrayMeasurements[2, 3] = respondentInfo[1];
            arrayMeasurements[2, 4] = respondentInfo[2];
            arrayMeasurements[2, 5] = respondentInfo[3];
            //arrayMeasurements[3, 2] = respondentInfo[0];
            //arrayMeasurements[3, 3] = respondentInfo[1];
            //arrayMeasurements[3, 4] = respondentInfo[2];
            //arrayMeasurements[3, 5] = respondentInfo[3];


        }

        //Retrieves all respondent specific information generated from sample manager
        private string[] GetRespondentIdentifiers()
        {
            string respIDs = File.ReadLines(@"C:\NZHS\surveyinstructions\MeasurementInfo.txt").First();
            string[] respIDSplit = respIDs.Split('+');
            return respIDSplit;
        }


        static List<decimal> measurementList = new List<decimal>();//Obsolete
        static List<decimal> finalMeasurementList = new List<decimal>();//Obsolete

        List<string[]> allMeasurements = new List<string[]>();//This list concerns only the measurements recieved immediately and is cleared when logging to a master measurements list.

        //Event of value change detected from BPM machine
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //transfer characteristic value of Ibuffer type to a byte array. Byte array already returning decimal readings. Eases processing.
            byte[] array = args.CharacteristicValue.ToArray();
            string SYS = array[1].ToString();
            string DIA = array[3].ToString();
            string PUL = array[14].ToString();
            string[] measurement = { SYS, DIA, PUL };

            allMeasurements.Add(measurement);
            if (allMeasurements.Count == 1)
            {
                RunResultsTimer();//Only running timer to get final result on the first observed characteristic value change. i.e. BPM reading. List contunues to be added to and timer finds last reading.
            }

        }

        //This timer fires when the first characteristic value change has been detected. It allows setting of the final measurement observed. 
        //This timer is needed because the BPM machine sends up to 7 measurements via BT that were previously stored in memory.
        //After the timer is complete, we access the last observed measurement out of the potential 7 which is the actual real time reading (most recent).
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private void RunResultsTimer()
        {

            //Set up timespan of 1 seconds to await any other final results that may be transmitted                       
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);//Tick period of 1 second gives enough time for all measurements in memory to send and be added to allMeasurements list.
            dispatcherTimer.IsEnabled = true;
            dispatcherTimer.Start();

        }


        List<string[]> finalMeasurementsList = new List<string[]>();
        //These booleans declare what measurement field is enabled depending on incoming measurements and whether a re-measurement is being taken.
        bool field1enabled = false;
        bool field2enabled = false;
        bool field3enabled = false;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            dispatcherTimer.Stop();
            dispatcherTimer.IsEnabled = false;
            string[] finalMeasurements = allMeasurements[allMeasurements.Count - 1]; //the measurement we are concerned with is the last observed in all measurements list after timer has ticked 1 second.
            if (!clear1WasClicked && !clear2WasClicked && !clear3WasClicked)//Only concerned with the automatic iterative process when adding to finalmeasurements list.
            {
                finalMeasurementsList.Add(finalMeasurements);//This list is only added to in events of non-cleared measurements, so can iterate down the measurement fields each successive measurement.
            }

            string SYS = finalMeasurements[0];
            string DIA = finalMeasurements[1];
            string PUL = finalMeasurements[2];

            //If finalMeasurementsList.Count is equal to one then enable field set 1, if 2 enable field set 2, if 3 enable field set 3. These are the normal cases. must handle re-taking measurements
            switch (finalMeasurementsList.Count)
            {
                case 1:
                    field1enabled = true;
                    break;
                case 2:
                    field2enabled = true;
                    break;
                case 3:
                    field3enabled = true;
                    break;
            }

            allMeasurements.Clear();//This array is cleared on timer elapse to allow a fresh stream of values from BPM machine. finalMeasurementsList is not cleared as it allows to iterate through measurement fields.
            InputValues(SYS, DIA, PUL);

        }

        bool field1AtleastOneMeasurement = false;
        bool field2AtleastOneMeasurement = false;
        bool field3AtleastOneMeasurement = false;
        private void InputValues(string sys, string dia, string pul)
        {

            if (!clear1WasClicked && !clear2WasClicked && !clear3WasClicked) //This is the iterative method that only applies in cases where clear wasn't clicked. fields enabled by allmeasurementlist count.
            {
                if (field1enabled == true)
                {
                    clear1.IsEnabled = true;
                    Set1stMeasurement(sys, dia, pul);
                    field1AtleastOneMeasurement = true;//Tracking this allows to enable the clear button if another fields clear button has been pressed, but the field has value already and therefore can enable option to clear.
                                                       //Enabling each clear on each successful iterative measurement
                }
                else if (field2enabled == true)
                {
                    clear2.IsEnabled = true;
                    Set2ndMeasurement(sys, dia, pul);
                    field2AtleastOneMeasurement = true;

                }
                else if (field3enabled == true)
                {
                    clear3.IsEnabled = true;
                    Set3rdMeasurement(sys, dia, pul);
                    field3AtleastOneMeasurement = true;

                }
            }
            else
            {
                if (clear1WasClicked)
                {
                    setClears();
                    Set1stMeasurement(sys, dia, pul);

                }
                else if (clear2WasClicked)
                {
                    setClears();
                    Set2ndMeasurement(sys, dia, pul);

                }
                else if (clear3WasClicked)
                {
                    setClears();
                    Set3rdMeasurement(sys, dia, pul);

                }
            }
            field1enabled = false;
            field2enabled = false;
            field3enabled = false;
            clear1WasClicked = false;
            clear2WasClicked = false;
            clear3WasClicked = false;

        }

        private void setClears()
        {
            if (field1AtleastOneMeasurement)
            {
                clear1.IsEnabled = true;
            }
            if (field2AtleastOneMeasurement)
            {
                clear2.IsEnabled = true;
            }
            if (field3AtleastOneMeasurement)
            {
                clear3.IsEnabled = true;
            }
        }

        public void Set1stMeasurement(string sys, string dia, string pul)
        {
            Application.Current.Dispatcher.Invoke(() => { SYS1.Text = sys; DIA1.Text = dia; PUL1.Text = pul; });
            DisableEverythingAndStartCountdown();
        }

        public void Set2ndMeasurement(string sys, string dia, string pul)
        {
            Application.Current.Dispatcher.Invoke(() => { SYS2.Text = sys; DIA2.Text = dia; PUL2.Text = pul; });
            DisableEverythingAndStartCountdown();
        }

        public void Set3rdMeasurement(string sys, string dia, string pul)
        {
            Application.Current.Dispatcher.Invoke(() => { SYS3.Text = sys; DIA3.Text = dia; PUL3.Text = pul; });
            DisableEverythingAndStartCountdown();
        }


        System.Windows.Threading.DispatcherTimer dispatcherTimer2 = new System.Windows.Threading.DispatcherTimer();
        int counter = 60;
        private void DisableEverythingAndStartCountdown()
        {
            RemoveValueChangedHandler();//Turn off the Bluetooth stream, measurements attempted will not come through.
            updateConnectionStatus("Awaiting Countdown");
            clear1.IsEnabled = false;
            clear2.IsEnabled = false;
            clear3.IsEnabled = false;
            counter = 10;
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 1);//
            dispatcherTimer2.IsEnabled = true;
            dispatcherTimer2.Start();
        }

        private void dispatcherTimer2_Tick(object sender, EventArgs e)
        {
            NextMeasurementIn.Visibility = Visibility.Visible;
            CounterLabel.Visibility = Visibility.Visible;
            MinuteDelayPrompt.Visibility = Visibility.Visible;
            counter--;
            CounterLabel.Text = counter.ToString();
            if (counter == 0)
            {
                dispatcherTimer2.Stop();
                AddValueChangedHandler();//Turn the bluetooth stream back on
                NextMeasurementIn.Visibility = Visibility.Hidden;
                CounterLabel.Visibility = Visibility.Hidden;
                MinuteDelayPrompt.Visibility = Visibility.Hidden;
                updateConnectionStatus("Ready For Measurement");
                setClears();
            }


        }

        //Salter scales sepcific csv file write
        private async void WriteCSVFile(string csvMeasurements)
        {
            System.IO.Directory.CreateDirectory(@"C:\NZHS\BodyMeasurements\WeightMeasurements");
            string CSVFileName = @"C:\NZHS\BodyMeasurements\WeightMeasurements\" + "WeightMeasurements_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + ".csv";

            System.IO.File.WriteAllText(CSVFileName, csvMeasurements);
        }

        //Wrtie 2d array to format expected of csv
        static string ArrayToCsv(string[,] values)
        {
            // Get the bounds.
            int num_rows = values.GetUpperBound(0) + 1;
            int num_cols = values.GetUpperBound(1) + 1;

            // Convert the array into a CSV string.
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < num_rows; row++)
            {
                // Add the first field in this row.
                sb.Append(values[row, 0]);

                // Add the other fields in this row separated by commas.
                for (int col = 1; col < num_cols; col++)
                    sb.Append("," + values[row, col]);

                // Move to the next line.
                sb.AppendLine();
            }

            // Return the CSV format string.
            return sb.ToString();
        }

        //Generate a 2d array that can be sent through arraytocsv to store values
        static T[,] CreateRectangularArray<T>(IList<T[]> arrays)
        {
            // TODO: Validation and special-casing for arrays.Count == 0
            int minorLength = arrays[0].Length;
            T[,] ret = new T[arrays.Count, minorLength];
            for (int i = 0; i < arrays.Count; i++)
            {
                var array = arrays[i];
                if (array.Length != minorLength)
                {
                    throw new ArgumentException
                        ("All arrays must be the same length");
                }
                for (int j = 0; j < minorLength; j++)
                {
                    ret[i, j] = array[j];
                }
            }
            return ret;
        }



        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        //Characteristic value changed handler instantiation for handling event of new BT data stream
        private void AddValueChangedHandler()
        {
            //ValueChangedSubscribeToggle.Content = "Unsubscribe from value changes";
            if (!subscribedForNotifications)
            {
                registeredCharacteristic = selectedCharacteristic;
                registeredCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        //Get rid of characteristic value changed handler, i.e. the actual transmission of BT data event
        private void RemoveValueChangedHandler()
        {
            //ValueChangedSubscribeToggle.Content = "Subscribe to value changes";
            if (subscribedForNotifications)
            {
                registeredCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                registeredCharacteristic = null;
                subscribedForNotifications = false;
            }
        }

        private decimal ConvertStrToDec(string value)
        {
            decimal convert = Convert.ToDecimal(value);
            return convert;
        }

        #endregion

        //These regions are left here in case there is any further development. Do not worry about them. Display helpers only used in retrieving characteristic/service names
        //from UUID for BT device

        #region Display Helpers

        public static class DisplayHelpers
        {
            public static string GetServiceName(GattDeviceService service)
            {
                if (IsSigDefinedUuid(service.Uuid))
                {
                    GattNativeServiceUuid serviceName;
                    if (Enum.TryParse(Utilities.ConvertUuidToShortId(service.Uuid).ToString(), out serviceName))
                    {
                        return serviceName.ToString();
                    }
                }
                return "Custom Service: " + service.Uuid;
            }

            private static bool IsSigDefinedUuid(Guid uuid)
            {
                var bluetoothBaseUuid = new Guid("00000000-0000-1000-8000-00805F9B34FB");

                var bytes = uuid.ToByteArray();
                // Zero out the first and second bytes
                // Note how each byte gets flipped in a section - 1234 becomes 34 12
                // Example Guid: 35918bc9-1234-40ea-9779-889d79b753f0
                //                   ^^^^
                // bytes output = C9 8B 91 35 34 12 EA 40 97 79 88 9D 79 B7 53 F0
                //                ^^ ^^
                bytes[0] = 0;
                bytes[1] = 0;
                var baseUuid = new Guid(bytes);
                return baseUuid == bluetoothBaseUuid;
            }

            public static string GetCharacteristicName(GattCharacteristic characteristic)
            {
                if (IsSigDefinedUuid(characteristic.Uuid))
                {
                    GattNativeCharacteristicUuid characteristicName;
                    if (Enum.TryParse(Utilities.ConvertUuidToShortId(characteristic.Uuid).ToString(),
                        out characteristicName))
                    {
                        return characteristicName.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(characteristic.UserDescription))
                {
                    return characteristic.UserDescription;
                }

                else
                {
                    return "Custom Characteristic: " + characteristic.Uuid;
                }
            }

            public enum GattNativeServiceUuid : ushort
            {
                None = 0,
                AlertNotification = 0x1811,
                Battery = 0x180F,
                BloodPressure = 0x1810,
                CurrentTimeService = 0x1805,
                CyclingSpeedandCadence = 0x1816,
                DeviceInformation = 0x180A,
                GenericAccess = 0x1800,
                GenericAttribute = 0x1801,
                Glucose = 0x1808,
                HealthThermometer = 0x1809,
                HeartRate = 0x180D,
                HumanInterfaceDevice = 0x1812,
                ImmediateAlert = 0x1802,
                LinkLoss = 0x1803,
                NextDSTChange = 0x1807,
                PhoneAlertStatus = 0x180E,
                ReferenceTimeUpdateService = 0x1806,
                RunningSpeedandCadence = 0x1814,
                ScanParameters = 0x1813,
                TxPower = 0x1804,
                SimpleKeyService = 0xFFE0

            }

            /// <summary>
            ///     This enum is nice for finding a string representation of a BT SIG assigned value for Characteristic UUIDs
            ///     Reference: https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicsHome.aspx
            /// </summary>
            public enum GattNativeCharacteristicUuid : ushort
            {
                None = 0,
                AlertCategoryID = 0x2A43,
                AlertCategoryIDBitMask = 0x2A42,
                AlertLevel = 0x2A06,
                AlertNotificationControlPoint = 0x2A44,
                AlertStatus = 0x2A3F,
                Appearance = 0x2A01,
                BatteryLevel = 0x2A19,
                BloodPressureFeature = 0x2A49,
                BloodPressureMeasurement = 0x2A35,
                BodySensorLocation = 0x2A38,
                BootKeyboardInputReport = 0x2A22,
                BootKeyboardOutputReport = 0x2A32,
                BootMouseInputReport = 0x2A33,
                CSCFeature = 0x2A5C,
                CSCMeasurement = 0x2A5B,
                CurrentTime = 0x2A2B,
                DateTime = 0x2A08,
                DayDateTime = 0x2A0A,
                DayofWeek = 0x2A09,
                DeviceName = 0x2A00,
                DSTOffset = 0x2A0D,
                ExactTime256 = 0x2A0C,
                FirmwareRevisionString = 0x2A26,
                GlucoseFeature = 0x2A51,
                GlucoseMeasurement = 0x2A18,
                GlucoseMeasurementContext = 0x2A34,
                HardwareRevisionString = 0x2A27,
                HeartRateControlPoint = 0x2A39,
                HeartRateMeasurement = 0x2A37,
                HIDControlPoint = 0x2A4C,
                HIDInformation = 0x2A4A,
                IEEE11073_20601RegulatoryCertificationDataList = 0x2A2A,
                IntermediateCuffPressure = 0x2A36,
                IntermediateTemperature = 0x2A1E,
                LocalTimeInformation = 0x2A0F,
                ManufacturerNameString = 0x2A29,
                MeasurementInterval = 0x2A21,
                ModelNumberString = 0x2A24,
                NewAlert = 0x2A46,
                PeripheralPreferredConnectionParameters = 0x2A04,
                PeripheralPrivacyFlag = 0x2A02,
                PnPID = 0x2A50,
                ProtocolMode = 0x2A4E,
                ReconnectionAddress = 0x2A03,
                RecordAccessControlPoint = 0x2A52,
                ReferenceTimeInformation = 0x2A14,
                Report = 0x2A4D,
                ReportMap = 0x2A4B,
                RingerControlPoint = 0x2A40,
                RingerSetting = 0x2A41,
                RSCFeature = 0x2A54,
                RSCMeasurement = 0x2A53,
                SCControlPoint = 0x2A55,
                ScanIntervalWindow = 0x2A4F,
                ScanRefresh = 0x2A31,
                SensorLocation = 0x2A5D,
                SerialNumberString = 0x2A25,
                ServiceChanged = 0x2A05,
                SoftwareRevisionString = 0x2A28,
                SupportedNewAlertCategory = 0x2A47,
                SupportedUnreadAlertCategory = 0x2A48,
                SystemID = 0x2A23,
                TemperatureMeasurement = 0x2A1C,
                TemperatureType = 0x2A1D,
                TimeAccuracy = 0x2A12,
                TimeSource = 0x2A13,
                TimeUpdateControlPoint = 0x2A16,
                TimeUpdateState = 0x2A17,
                TimewithDST = 0x2A11,
                TimeZone = 0x2A0E,
                TxPowerLevel = 0x2A07,
                UnreadAlertStatus = 0x2A45,
                AggregateInput = 0x2A5A,
                AnalogInput = 0x2A58,
                AnalogOutput = 0x2A59,
                CyclingPowerControlPoint = 0x2A66,
                CyclingPowerFeature = 0x2A65,
                CyclingPowerMeasurement = 0x2A63,
                CyclingPowerVector = 0x2A64,
                DigitalInput = 0x2A56,
                DigitalOutput = 0x2A57,
                ExactTime100 = 0x2A0B,
                LNControlPoint = 0x2A6B,
                LNFeature = 0x2A6A,
                LocationandSpeed = 0x2A67,
                Navigation = 0x2A68,
                NetworkAvailability = 0x2A3E,
                PositionQuality = 0x2A69,
                ScientificTemperatureinCelsius = 0x2A3C,
                SecondaryTimeZone = 0x2A10,
                String = 0x2A3D,
                TemperatureinCelsius = 0x2A1F,
                TemperatureinFahrenheit = 0x2A20,
                TimeBroadcast = 0x2A15,
                BatteryLevelState = 0x2A1B,
                BatteryPowerState = 0x2A1A,
                PulseOximetryContinuousMeasurement = 0x2A5F,
                PulseOximetryControlPoint = 0x2A62,
                PulseOximetryFeatures = 0x2A61,
                PulseOximetryPulsatileEvent = 0x2A60,
                SimpleKeyState = 0xFFE1,
                Weight = 0x2A98,
                WeightMeasurement = 0x2A9D
            }



        }

        public static class Utilities
        {
            /// <summary>
            ///     Converts from standard 128bit UUID to the assigned 32bit UUIDs. Makes it easy to compare services
            ///     that devices expose to the standard list.
            /// </summary>
            /// <param name="uuid">UUID to convert to 32 bit</param>
            /// <returns></returns>
            public static ushort ConvertUuidToShortId(Guid uuid)
            {
                // Get the short Uuid
                var bytes = uuid.ToByteArray();
                var shortUuid = (ushort)(bytes[0] | (bytes[1] << 8));
                return shortUuid;
            }



            #endregion

            #region Constants

            public class Constants
            {
                // BT_Code: Initializes custom local parameters w/ properties, protection levels as well as common descriptors like User Description. 
                public static readonly GattLocalCharacteristicParameters gattOperandParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Write |
                                               GattCharacteristicProperties.WriteWithoutResponse,
                    WriteProtectionLevel = GattProtectionLevel.Plain,
                    UserDescription = "Operand Characteristic"
                };

                public static readonly GattLocalCharacteristicParameters gattOperatorParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Write |
                                               GattCharacteristicProperties.WriteWithoutResponse,
                    WriteProtectionLevel = GattProtectionLevel.Plain,
                    UserDescription = "Operator Characteristic"
                };

                public static readonly GattLocalCharacteristicParameters gattResultParameters = new GattLocalCharacteristicParameters
                {
                    CharacteristicProperties = GattCharacteristicProperties.Read |
                                               GattCharacteristicProperties.Notify,
                    WriteProtectionLevel = GattProtectionLevel.Plain,
                    UserDescription = "Result Characteristic"
                };

                public static readonly Guid CalcServiceUuid = Guid.Parse("caecface-e1d9-11e6-bf01-fe55135034f0");

                public static readonly Guid Op1CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f1");
                public static readonly Guid Op2CharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f2");
                public static readonly Guid OperatorCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f3");
                public static readonly Guid ResultCharacteristicUuid = Guid.Parse("caec2ebc-e1d9-11e6-bf01-fe55135034f4");
            };





            #endregion


        }



    }
}
