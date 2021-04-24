using AndroidUsbServer.Models;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AndroidUsbServer.Services
{
    public interface IUsbService
    {

        public event EventHandler<SerialDataReceivedArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> ErrorReceived;

        IReadOnlyList<UsbDriver> Drivers { get; }

        Task<IEnumerable<IUsbSerialDriver>> GetDriversAsync();
        IEnumerable<string> GetFullDeviceList();
        Task<IEnumerable<string>> GetDeviceListAsync();
        Task<IEnumerable<UsbSerialPort>> GetPortsListAsync();
        Task<bool> AskPortPermission(UsbSerialPort port);
        IEnumerable<UsbPort> Map(IEnumerable<UsbSerialPort> usbPorts, bool full = true);
        Task OpenSerialAsync(UsbSerialPort port, int baudRate = 9600, int dataBits = 8, StopBits stopBits = StopBits.One, Parity parity = Parity.None);
        void CloseSerial();
        IEnumerable<UsbPort> MockPorts();
        IEnumerable<string> MockDevices();
    }
}
