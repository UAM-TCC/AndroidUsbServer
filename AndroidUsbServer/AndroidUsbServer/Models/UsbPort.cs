namespace AndroidUsbServer.Models
{
    public class UsbPort
    {
        public string Type { get; set; }
        public string DeviceName { get; set; }
        public int DeviceId { get; set; }
        public int PortNumber { get; set; }
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public string SerialNumber { get; set; }
        public string ProductName { get; set; }
        public string ManufacturerName { get; set; }
        public int DeviceProtocol { get; set; }
    }
}