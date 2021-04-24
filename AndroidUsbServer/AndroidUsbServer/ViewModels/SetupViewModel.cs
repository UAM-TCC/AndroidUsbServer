using AndroidUsbServer.Common;
using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using Hoho.Android.UsbSerial.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace AndroidUsbServer.ViewModels
{
    public class SetupViewModel : BaseViewModel
    {
        // Services

        private readonly IUsbService _usbService;

        // Observables

        public ObservableCollection<string> Drivers { get; set; }

        private UsbDriver _selectedDriver;
        public string SelectedDriver
        {
            get => _selectedDriver.Alias;
            set
            {
                if (_selectedDriver.Alias != value && _usbService.Drivers.Any(d => d.Alias == value))
                {
                    _selectedDriver = _usbService.Drivers.Single(d => d.Alias == value);
                    OnPropertyChanged();
                }
            }
        }

        private string _endpoint;
        public string Endpoint
        {
            get => _endpoint;
            set
            {
                if (_endpoint != value)
                {
                    _endpoint = value;
                    OnPropertyChanged();
                }
            }
        }

        // Properties

        private UsbSerialPort _usbPort;

        // Constructors

        public SetupViewModel(IUsbService usbService, UsbSerialPort usbPort)
        {
            _usbService = usbService;
            Drivers = new ObservableCollection<string>(_usbService.Drivers.Select(d => d.Alias));

            _usbPort = usbPort;
            _selectedDriver = _usbService.Drivers.FirstOrDefault(d => d.Driver == _usbPort.Driver.GetType());

            if (_selectedDriver == null)
                throw new Exception("Invalid driver");

            var port = 8000;
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddressList = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            var ipAddress = ipAddressList.ElementAt(0); // Choose IP from available interfaces list
            _endpoint = new IPEndPoint(ipAddress, port).ToString();
        }

        // Methods

        public AndroidServer CreateSetup()
        {
            IPEndPoint endpoint;

            try
            {
                var (address, port, _) = _endpoint.Split(":");
                endpoint = new IPEndPoint(IPAddress.Parse(address), int.Parse(port));
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid endpoint: {_endpoint}", ex);
            }

            return new AndroidServer
            {
                Port = _usbPort,
                Driver = _selectedDriver,
                Endpoint = endpoint
            };
        }
    }
}
