//using AndroidUsbServer.Services;
//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.Threading.Tasks;

//namespace AndroidUsbServer.Droid.Services
//{
//    public class ServerService : IServerService
//    {
//        private readonly IPEndPoint _endPoint;
//        private readonly TcpListener _server;

//        private Thread _thread;

//        private string _output = "";

//        public event EventHandler Opened;
//        public event EventHandler<DataReceivedEventArgs> DataReceived;
//        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
//        public event EventHandler Closed;

//        public ServerService()
//        {
//            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
//            IPAddress ipAddress = ipHostInfo.AddressList[0];
//            _endPoint = new IPEndPoint(ipAddress, 12345);

//            _server = new TcpListener(_endPoint);

//            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
//            //IPAddress ipAddress = ipHostInfo.AddressList[0];
//            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
//            //System.Diagnostics.Debug.WriteLine(ipAddress.ToString());
//            //// Create a TCP/IP socket.
//            //Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//        }

//        public Task<AndroidServer> StartAsync()
//        {
//            try
//            {
//                _thread = new Thread(Listen);
//                _thread.IsBackground = true;
//                _thread.Start();

//                Action<string> sendAction = (string message) => _output += message;

//                var server = new AndroidServer
//                {
//                    Endpoint = _endPoint,
//                    Send = sendAction
//                };

//                return Task.FromResult(server);
//            }
//            catch (Exception ex)
//            {
//                ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex));
//                throw ex;
//            }
//        }

//        public Task StopAsync()
//        {
//            try
//            {
//                _server?.Stop();
//                if (_thread != null && _thread.IsAlive) _thread.Abort();
//                Closed?.Invoke(this, null);
//            }
//            catch (Exception ex)
//            {
//                ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex));
//                throw ex;
//            }

//            return Task.CompletedTask;
//        }

//        private void Listen()
//        {
//            try
//            {
//                _server.Start();

//                Opened?.Invoke(this, null);

//                // Buffer for reading data
//                Byte[] bytes = new Byte[256];
//                String data = null;

//                // Enter the listening loop.
//                while (true)
//                {
//                    Console.Write("Waiting for a connection... ");

//                    // Perform a blocking call to accept requests.
//                    // You could also use server.AcceptSocket() here.
//                    TcpClient client = _server.AcceptTcpClient();
//                    Console.WriteLine("Connected!");

//                    data = null;

//                    // Get a stream object for reading and writing
//                    NetworkStream stream = client.GetStream();

//                    int i;

//                    // Loop to receive all the data sent by the client.
//                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
//                    {
//                        // Translate data bytes to a ASCII string.
//                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

//                        // Process the data sent by the client.
//                        DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
//                        var output = _output;
//                        _output = "";

//                        // Send back a response.

//                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(output);
//                        stream.Write(msg, 0, msg.Length);
//                    }

//                    // Shutdown and end connection
//                    client.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex));
//                throw ex;
//            }
//        }

//        public void Dispose()
//        {
//            StopAsync().Wait();
//        }
//    }
//}