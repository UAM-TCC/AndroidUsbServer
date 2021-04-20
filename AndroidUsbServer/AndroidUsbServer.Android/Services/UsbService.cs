using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidUsbServer.Droid.Services
{
    public class UsbService : IUsbService, IDisposable
    {
        private readonly Activity _activity;
        private readonly UsbManager _usbManager;
        private SerialInputOutputManager _serialManager;

        public event EventHandler<SerialDataReceivedArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> ErrorReceived;

        public UsbService(Activity activity)
        {
            _activity = activity;
            _usbManager = _activity.GetSystemService(Context.UsbService) as UsbManager;
        }

        public async Task<IEnumerable<IUsbSerialDriver>> GetDriversAsync()
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            //var table = UsbSerialProber.DefaultProbeTable;
            //table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG

            //table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4


            // add CDC (ATtiny85)
            var table = new ProbeTable();
            table.AddDriver(typeof(CdcAcmSerialDriver));


            var prober = new UsbSerialProber(table);
            var drivers = await prober.FindAllDriversAsync(_usbManager);

            return drivers.ToList();
        }

        public async Task<IEnumerable<string>> GetDeviceListAsync()
        {
            return _usbManager.DeviceList.Values.Select(x => $"V {x.VendorId} / P {x.ProductId} / D {x.DeviceId}");
            //var drivers = await GetDriversAsync();
            //return drivers.Select(x => x.GetDevice()?.DeviceId).Select(x => x.GetValueOrDefault().ToString());
        }

        public async Task<IEnumerable<UsbSerialPort>> GetPortsListAsync()
        {
            var drivers = await GetDriversAsync();
            return drivers.Select(x => x.Ports).SelectMany(x => x);
        }

        public async Task<bool> AskPortPermission(UsbSerialPort port)
        {
            // request user permission to connect to device
            // NOTE: no request is shown to user if permission already granted
            var permissionGranted = await _usbManager.RequestPermissionAsync(port.Driver.Device, _activity);
            return permissionGranted;
        }

        public IEnumerable<UsbPort> Map(IEnumerable<UsbSerialPort> usbPorts)
        {
            return usbPorts.Select(x => new UsbPort
            {
                Type = x.GetType().Name,
                DeviceName = x.Driver.Device.DeviceName,
                DeviceId = x.Driver.Device.DeviceId,
                PortNumber = x.PortNumber,
                //ProductId = x.Driver.Device.ProductId,
                //VendorId = x.Driver.Device.VendorId,
                //SerialNumber = x.Driver.Device.SerialNumber,
                //ProductName = x.Driver.Device.ProductName,
                //ManufacturerName = x.Driver.Device.ManufacturerName,
                //DeviceProtocol = x.Driver.Device.DeviceProtocol
            });
        }

        public async Task OpenSerialAsync(UsbSerialPort port, int baudRate = 9600, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None)
        {
            var canUsb = await AskPortPermission(port);
            if (!canUsb) throw new Exception("Permissão USB rejeitada");

            _serialManager = new SerialInputOutputManager(port)
            {
                BaudRate = baudRate,
                DataBits = dataBits,
                StopBits = stopBits,
                Parity = parity,
            };

            _serialManager.DataReceived += DataReceived;
            _serialManager.ErrorReceived += ErrorReceived;

            if (!_serialManager.IsOpen)
                _serialManager.Open(_usbManager);
        }

        public void CloseSerial()
        {
            _serialManager?.Close();
        }

        public void Dispose()
        {
            CloseSerial();
        }
    }
}