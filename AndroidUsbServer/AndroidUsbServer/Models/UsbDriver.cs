using System;

namespace AndroidUsbServer.Models
{
    public class UsbDriver
    {
        public UsbDriver(string alias, Type driver)
        {
            Alias = alias;
            Driver = driver;
        }

        public string Alias { get; set; }
        public Type Driver { get; set; }
    }
}
