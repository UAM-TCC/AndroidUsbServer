using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidUsbServer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        static readonly string TAG = typeof(MainActivity).Name;

        EditText textView;

        UsbManager usbManager;
        List<UsbSerialPort> ports = new List<UsbSerialPort>();
        UsbSerialPort port;

        const int READ_WAIT_MILLIS = 200;
        const int WRITE_WAIT_MILLIS = 0;
        SerialInputOutputManager serialIoManager;

        Timer timer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            textView = FindViewById<EditText>(Resource.Id.textView);

            usbManager = GetSystemService(Context.UsbService) as UsbManager;

            Log.Info(TAG, "Log test ...");
        }

        protected override async void OnResume()
        {
            base.OnResume();

            try
            {
                var drivers = await FindAllDriversAsync(usbManager);
                foreach (var driver in drivers)
                    ports.AddRange(driver.Ports);

                textView.Text += string.Join("\n\n", drivers.Select(driver => $"{ driver.Device.DeviceName} - {driver.Device.ProductName}\nPorts:\n{string.Join("\n", driver.Ports.Select(x => x.PortNumber))}")) + "\n\n";

                var permissionGranted = await UsbPortPermission(0);

                if (permissionGranted)
                {
                    serialIoManager = new SerialInputOutputManager(port)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None,
                    };

                    serialIoManager.DataReceived += (sender, e) => {
                        RunOnUiThread(() => {
                            try
                            {
                                var message = "Read " + e.Data.Length + " bytes: \n" + HexDump.DumpHexString(e.Data) + "\n\n";
                                textView.Text += message;
                            }
                            catch (Exception ex)
                            {
                                textView.Text += ErrorString(ex);
                            }
                        });
                    };

                    serialIoManager.ErrorReceived += (sender, e) => {
                        RunOnUiThread(() => {
                            textView.Text += "ERROR\n\n";
                        });
                    };

                    textView.Text += $"Opening Serial...\n\n";

                    if (!serialIoManager.IsOpen)
                        serialIoManager.Open(usbManager);

                    timer = new Timer(state =>
                    {
                        RunOnUiThread(() => {
                            try
                            {
                                var stateCls = state as State;
                                textView.Text += $"Sending: {stateCls.Text}\n";
                                WriteData(Encoding.ASCII.GetBytes(stateCls.Text));
                                stateCls.Text = stateCls.Text == "ON" ? "OFF" : "ON";
                            }
                            catch (Exception ex)
                            {
                                textView.Text += ErrorString(ex);
                            }
                        });
                    }, new State { Text = "ON" }, 0, 10000);
                }
            }
            catch (Exception ex)
            {
                textView.Text += ErrorString(ex);
            }
        }

        class State
        {
            public string Text { get; set; }
        }

        protected override void OnPause()
        {
            base.OnPause();

            timer?.Dispose();

            if (serialIoManager != null && serialIoManager.IsOpen)
            {
                try
                {
                    serialIoManager.Close();
                }
                catch (Java.IO.IOException)
                {
                    // ignore
                }
            }
        }

        private Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG

            table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

            var prober = new UsbSerialProber(table);
            return prober.FindAllDriversAsync(usbManager);
        }

        private async Task<bool> UsbPortPermission(int position)
        {
            // request user permission to connect to device
            // NOTE: no request is shown to user if permission already granted
            port = ports.ElementAtOrDefault(position);
            if (port == null) throw new Exception($"Port {position} not found!");
            var permissionGranted = await usbManager.RequestPermissionAsync(port.Driver.Device, this);
            return permissionGranted;
        }

        void WriteData(byte[] data)
        {
            if (serialIoManager.IsOpen)
            {
                port.Write(data, WRITE_WAIT_MILLIS);
            }
            else
            {
                textView.Text += "Serial is closed\n";
            }
        }

        private string ErrorString(Exception ex) => ex.GetType().Name + ": " + ex.Message + "\nInner: " + ex.InnerException?.Message + "\nStack: " + ex.StackTrace + "\n";

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}
    }
}