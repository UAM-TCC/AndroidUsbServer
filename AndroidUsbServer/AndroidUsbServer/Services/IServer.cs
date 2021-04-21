using System;
using System.Net;
using System.Net.Sockets;

namespace AndroidUsbServer.Services
{
    public interface IServer : IDisposable
    {
        event EventHandler<OpenedEventArgs> Opened;
        event EventHandler<DataReceivedEventArgs> DataReceived;
        event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        event EventHandler<ClosedEventArgs> Closed;

        Action<string> Send { get; }

        void Listen(IPEndPoint endpoint);
        void Close();
    }

    public class AndroidServer
    {
        public IPEndPoint Endpoint { get; set; }
        public Action<string> Send { get; set; }
    }

    public class OpenedEventArgs : EventArgs
    {
        public OpenedEventArgs(EndPoint endPoint, TcpClient client)
        {
            EndPoint = endPoint;
            Client = client;
        }

        public EndPoint EndPoint { get; }
        public TcpClient Client { get; }
    }

    public class ClosedEventArgs : EventArgs
    {
        public ClosedEventArgs(bool serverClosed, TcpClient client)
        {
            ServerClosed = serverClosed;
            Client = client;
        }

        public bool ServerClosed { get; }
        public TcpClient Client { get; }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(string data, TcpClient client)
        {
            Data = data;
            Client = client;
        }

        public string Data { get; }
        public TcpClient Client { get; }
    }

    public class ErrorReceivedEventArgs : EventArgs
    {
        public ErrorReceivedEventArgs(Exception exception, TcpClient client)
        {
            Exception = exception;
            Client = client;
        }

        public Exception Exception { get; }
        public TcpClient Client { get; }
    }
}
