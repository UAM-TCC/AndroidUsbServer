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
        public event EventHandler<SerialDataReceivedArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> ErrorReceived;

        private readonly Activity _activity;
        private readonly UsbManager _usbManager;

        private UsbSerialPort _serialPort;
        private SerialInputOutputManager _serialManager;

        public bool IsOpen => _serialManager?.IsOpen ?? false;
        public IReadOnlyList<UsbDriver> Drivers => new List<UsbDriver>
        {
            new UsbDriver("CDC", typeof(CdcAcmSerialDriver)),
            new UsbDriver("CH34x", typeof(Ch34xSerialDriver)),
            new UsbDriver("CP21xx", typeof(Cp21xxSerialDriver)),
            new UsbDriver("FTDI", typeof(FtdiSerialDriver)),
            new UsbDriver("Prolific", typeof(ProlificSerialDriver)),
            new UsbDriver("STM32", typeof(STM32SerialDriver)),
        };

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

            var table = new ProbeTable();
            table.AddDriver(typeof(CdcAcmSerialDriver));

            table.AddProduct(0x16D0, 0x087E, typeof(CdcAcmSerialDriver)); // MSC ATtiny85

            table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG
            table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

            var prober = new UsbSerialProber(table);
            var drivers = await prober.FindAllDriversAsync(_usbManager);

            return drivers.ToList();
        }

        public IEnumerable<string> GetFullDeviceList()
        {
            return _usbManager.DeviceList.Values.Select(x => $"V {x.VendorId} / P {x.ProductId} / D {x.DeviceId}");
        }

        public async Task<IEnumerable<string>> GetDeviceListAsync()
        {
            var drivers = await GetDriversAsync();
            return drivers.Select(x => x.GetDevice()?.DeviceId).Select(x => x.GetValueOrDefault().ToString());
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

        public IEnumerable<UsbPort> Map(IEnumerable<UsbSerialPort> usbPorts, bool full = true)
        {
            return usbPorts.Select(x => new UsbPort
            {
                Type = x.GetType().Name,
                PortNumber = x.PortNumber, // 0, 1 ... incremental ?
                VendorId = x.Driver?.Device?.VendorId ?? 0, // 5840 (0x16D0) = MSC
                ProductId = x.Driver?.Device?.ProductId ?? 0, // 2174 (0x087E) = ATtiny85
                DeviceId = x.Driver?.Device?.DeviceId ?? 0, // 1002, 1003 ... incremental ?
                SerialNumber = full ? x.Driver?.Device?.SerialNumber : default,
                DeviceName = full ? x.Driver?.Device?.DeviceName : default,
                ProductName = full ? x.Driver?.Device?.ProductName : default,
                ManufacturerName = full ? x.Driver?.Device?.ManufacturerName : default,
                DeviceProtocol = full ? x.Driver?.Device?.DeviceProtocol ?? 0 : default
            });
        }

        public async Task OpenSerialAsync(UsbSerialPort port, int baudRate = 9600, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None)
        {
            _serialPort = port;

            var canUsb = await AskPortPermission(_serialPort);
            if (!canUsb) throw new Exception("Permissão USB rejeitada");

            _serialManager = new SerialInputOutputManager(_serialPort)
            {
                BaudRate = baudRate,
                DataBits = dataBits,
                StopBits = stopBits,
                Parity = parity,
            };

            _serialManager.DataReceived += DataReceived;
            _serialManager.ErrorReceived += ErrorReceived;

            if (!IsOpen) _serialManager.Open(_usbManager);
        }

        public void CloseSerial()
        {
            if (IsOpen) _serialManager?.Close();
            _serialManager = null;
            _serialPort = null;
        }

        public void Dispose()
        {
            CloseSerial();
        }

        public IEnumerable<string> MockDevices()
        {
            return new int[3].Select(x => $"V 5840 / P 2174 / D 1002");
        }

        public IEnumerable<UsbPort> MockPorts()
        {
            return new int[3].Select(x => new UsbPort
            {
                Type = "CdcAcmSerialDriver",
                VendorId = 5840, // 5840 (0x16D0) = MSC
                ProductId = 2174, // 2174 (0x087E) = ATtiny85
                DeviceId = 1002, // 1002, 1003 ... incremental ?
                PortNumber = 0, // 0, 1 ... incremental ?
                SerialNumber = "SerialNumber",
                DeviceName = "DeviceName",
                ProductName = "ProductName",
                ManufacturerName = "ManufacturerName",
                DeviceProtocol = 0
            });
        }
    }
}