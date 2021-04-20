using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidUsbServer.ViewModels
{
    public class UsbPortViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MainPage _page;
        private readonly IUsbService _usbService;
        private readonly IServerService _serverService;

        private UsbSerialPort _usbPort;
        private AndroidServer _server;

        private IEnumerable<UsbSerialPort> _usbSerialList { get; set; }
        public ObservableCollection<UsbPort> UsbList { get; set; } = new ObservableCollection<UsbPort>();
        public ObservableCollection<string> DeviceList { get; set; } = new ObservableCollection<string>();

        public UsbPortViewModel(MainPage page, IUsbService usbService, IServerService serverService)
        {
            _page = page;
            _usbService = usbService;
            _serverService = serverService;
        }

        public async Task InitAsync()
        {
            var devices = await _usbService.GetDeviceListAsync();
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

            _usbPort = _usbSerialList.ElementAt(portIndex);

            // Start USB

            _usbService.DataReceived += (sender, e) =>
            {
                try
                {
                    var message = "Read " + e.Data.Length + " bytes: \n" + HexDump.DumpHexString(e.Data) + "\n\n";
                    _server.Send(message);
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

            await _usbService.OpenSerialAsync(_usbPort, 9600, 8, StopBits.One, Parity.None);

            // Start Server

            _serverService.DataReceived += (sender, e) =>
            {
                try
                {
                    //if (!_serialManager.IsOpen)
                    //    throw new Exception("Serial is closed");
                    _usbPort?.Write(Encoding.ASCII.GetBytes(e.Data), 0);
                }
                catch (Exception ex)
                {
                    _page.ShowError(ex);
                }
            };

            _serverService.ErrorReceived += (sender, e) =>
            {
                _page.ShowError(e.Exception);
            };

            _server = await _serverService.StartAsync();

            _server.Send("START");

            return _server;
        }

        public void Disconnect()
        {
            _usbService?.CloseSerial();
            _serverService?.StopAsync().Wait();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
