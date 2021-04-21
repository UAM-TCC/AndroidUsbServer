using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AndroidUsbServer.ViewModels
{
    public class UsbPortViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MainPage _page;
        private readonly IUsbService _usbService;
        private readonly IServer _server;

        private IPEndPoint _endpoint;

        private IEnumerable<UsbSerialPort> _usbSerialList { get; set; }
        public ObservableCollection<UsbPort> UsbList { get; set; } = new ObservableCollection<UsbPort>();
        public ObservableCollection<string> DeviceList { get; set; } = new ObservableCollection<string>();

        public UsbPortViewModel(MainPage page, IUsbService usbService, IServer server)
        {
            _page = page;
            _usbService = usbService;
            _server = server;
        }

        public async Task InitAsync()
        {
            var port = 12345;
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddressList = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            var ipAddress = ipAddressList.ElementAt(0); // Choose IP from available interfaces list
            _endpoint = new IPEndPoint(ipAddress, port);

            var devices = _usbService.GetFullDeviceList();
            foreach (var device in devices)
                DeviceList.Add(device);

            _usbSerialList = await _usbService.GetPortsListAsync();

            if (!_usbSerialList.Any())
                _page.ShowMessage("Nenhum dispositivo encontrado");

            var usbPortList = _usbService.Map(_usbSerialList);

            foreach (var usb in usbPortList)
                UsbList.Add(usb);
        }

        public async Task<AndroidServer> ConnectAsync(int portIndex)
        {
            Disconnect();

            var usbPort = _usbSerialList.ElementAt(portIndex);

            // Start USB

            _usbService.DataReceived += (sender, e) =>
            {
                try
                {
                    var message = "Read " + e.Data.Length + " bytes: \n" + HexDump.DumpHexString(e.Data) + "\n\n";
                    _server?.Send(message);
                }
                catch (Exception ex)
                {
                    _page.ShowError(ex);
                }
            };

            _usbService.ErrorReceived += (sender, e) =>
            {
                _page.ShowError(e.ExceptionObject as Exception);
            };

            await _usbService.OpenSerialAsync(usbPort, 9600, 8, StopBits.One, Parity.None);

            // Start Server

            _server.DataReceived += (sender, e) =>
            {
                try
                {
                    //if (!_serialManager.IsOpen)
                    //    throw new Exception("Serial is closed");
                    usbPort?.Write(Encoding.UTF8.GetBytes(e.Data), 0);
                }
                catch (Exception ex)
                {
                    _page.ShowError(ex);
                }
            };

            _server.ErrorReceived += (sender, e) =>
            {
                _page.ShowError(e.Exception);
            };

            _server.Listen(_endpoint);

            _server.Send("Connected");

            return new AndroidServer
            {
                Endpoint = _endpoint,
                Send = _server.Send
            };
        }

        public void Disconnect()
        {
            _usbService?.CloseSerial();
            _server?.Close();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
