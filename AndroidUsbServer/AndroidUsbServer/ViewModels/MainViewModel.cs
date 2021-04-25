using AndroidUsbServer.Models;
using AndroidUsbServer.Services;
using Hoho.Android.UsbSerial.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace AndroidUsbServer.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        // Services

        private readonly IUsbService _usbService;
        private readonly IServer _server;

        // Observables

        public ObservableCollection<UsbPort> UsbList { get; set; } = new ObservableCollection<UsbPort>();

        // Properties

        private IEnumerable<UsbSerialPort> _usbSerialList;

        private AndroidServer _serverSetup;
        public AndroidServer ServerSetup
        {
            get => _serverSetup;
            set
            {
                if (_serverSetup != value)
                {
                    _serverSetup = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsIdle => !IsRunning;
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        // Constructors

        public MainViewModel(IUsbService usbService, IServer server)
        {
            _usbService = usbService;
            _server = server;
        }

        // Methods

        public async Task RefreshUsbList()
        {
            _usbSerialList = await _usbService.GetPortsListAsync();

            if (!_usbSerialList.Any())
                ShowMessage("Nenhum dispositivo encontrado");

            var usbPortList = _usbService.Map(_usbSerialList, full: false);
            //var usbPortList = _usbService.MockPorts();

            UsbList.Clear();
            foreach (var usb in usbPortList)
                UsbList.Add(usb);

            //UsbSerialProber:ProbeDevice
            //driver = (IUsbSerialDriver)Activator.CreateInstance(driverClass, new System.Object[] { usbDevice });
        }

        public UsbSerialPort GetUsbPort(int portIndex)
        {
            return _usbSerialList.ElementAt(portIndex);
        }

        public async Task<AndroidServer> ConnectAsync(AndroidServer serverSetup)
        {
            Disconnect();

            // Start USB

            _usbService.DataReceived += (sender, e) =>
            {
                try
                {
                    if (e?.Data == null) return;
                    var message = Encoding.UTF8.GetString(e.Data);
                    _server?.Send(message);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            };

            _usbService.ErrorReceived += (sender, e) =>
            {
                ShowError(e.ExceptionObject as Exception);
            };

            await _usbService.OpenSerialAsync(serverSetup.Port, 9600, 8, StopBits.One, Parity.None);

            // Start Server

            _server.DataReceived += (sender, e) =>
            {
                try
                {
                    if (e?.Data == null || !_usbService.IsOpen) return;
                    serverSetup?.Port?.Write(Encoding.UTF8.GetBytes(e.Data), 0);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            };

            _server.ErrorReceived += (sender, e) =>
            {
                ShowError(e.Exception);
            };

            _server.Listen(serverSetup.Endpoint);

            IsRunning = true;

            //_server.Send("Connected");

            serverSetup.Send = _server.Send;
            ServerSetup = serverSetup;

            return serverSetup;
        }

        public ICommand DisconnectCommand => new Command(Disconnect);
        public void Disconnect()
        {
            IsRunning = false;
            _usbService?.CloseSerial();
            _server?.Close();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
