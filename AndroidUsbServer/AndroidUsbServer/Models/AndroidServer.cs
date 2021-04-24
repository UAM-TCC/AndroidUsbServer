using Hoho.Android.UsbSerial.Driver;
using System;
using System.Net;

namespace AndroidUsbServer.Models
{
    public class AndroidServer
    {
        public UsbSerialPort Port { get; set; }
        public UsbDriver Driver { get; set; }
        public IPEndPoint Endpoint { get; set; }
        public Action<string> Send { get; set; }
    }
}
