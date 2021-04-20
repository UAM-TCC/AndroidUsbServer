using System;
using System.Net;
using System.Threading.Tasks;

namespace AndroidUsbServer.Services
{
    public interface IServerService : IDisposable
    {
        event EventHandler Opened;
        event EventHandler<DataReceivedEventArgs> DataReceived;
        event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        event EventHandler Closed;

        Task<AndroidServer> StartAsync();
        Task StopAsync();
    }

    public class AndroidServer
    {
        public IPEndPoint Endpoint { get; set; }
        public Action<string> Send { get; set; }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }

    public class ErrorReceivedEventArgs : EventArgs
    {
        public ErrorReceivedEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
