using AndroidUsbServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidUsbServer.Droid.Services
{
    public class WebSocketServer : IServer, IDisposable
    {
        // https://github.com/Tpessia/WebSocketServer

        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
        // Protocol: https://datatracker.ietf.org/doc/html/rfc6455
        // Frame Packets: https://tools.ietf.org/html/rfc6455#section-5.2
        // Opcodes: https://tools.ietf.org/html/rfc6455#section-11.8
        // Closing Codes: https://tools.ietf.org/html/rfc6455#section-7.4.1 / https://github.com/Luka967/websocket-close-codes
        // https://stackoverflow.com/questions/8125507/how-can-i-send-and-receive-websocket-messages-on-the-server-side
        // https://lucumr.pocoo.org/2012/9/24/websockets-101/

        // Handle client disconnect (ping/pong)
        // Handle wrong connection type (that's not WebSocket)
        // X Handle closing
        // X Handle multiple clients

        public event EventHandler<OpenedEventArgs> Opened;
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        public event EventHandler<ClosedEventArgs> Closed;

        private IPEndPoint _endpoint;
        private TcpListener _server;
        private Task _serverTask;
        private CancellationTokenSource _serverCancellationToken = new CancellationTokenSource();
        private List<(TcpClient Client, Task Task, CancellationTokenSource Cancellation)> _clients = new List<(TcpClient, Task, CancellationTokenSource)>();

        public Action<string> Send => (string message) => _clients.ForEach(c => WriteMessage(c.Client, message));
        //public Action<TcpClient, string> Send => (TcpClient client, string message) => WriteMessage(client, message);
        //public Action<string> SendAll => (string message) => _clients.ForEach(c => WriteMessage(c.Client, message));

        public bool IsOpen { get; protected set; } = false;

        public void Listen(IPEndPoint endpoint)
        {
            //var port = 12345;
            //var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //var ipAddressList = ipHostInfo.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            //var ipAddress = ipAddressList.ElementAt(0); // Choose IP from available interfaces list
            //var endpoint = new IPEndPoint(ipAddress, port);

            _endpoint = endpoint;
            _server = new TcpListener(_endpoint);

            _server.Start();
            _serverTask = Task.Run(() => HandleClients(), _serverCancellationToken.Token);

            IsOpen = true;

            Opened?.Invoke(this, new OpenedEventArgs(_endpoint, null));
        }

        public void Close()
        {
            try { _clients.ForEach(c => CloseClient(c.Client)); }
            catch (Exception ex) { ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex, null)); }

            _clients.Clear();

            _server.Stop();

            try { _serverCancellationToken.Cancel(); }
            catch (Exception ex) { ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex, null)); }

            _serverCancellationToken = new CancellationTokenSource();

            IsOpen = false;

            Closed?.Invoke(this, new ClosedEventArgs(true, null));
        }

        public void CloseClient(TcpClient client)
        {
            var clientTuple = _clients.SingleOrDefault(c => c.Client == client);

            try
            {
                _clients.Remove(clientTuple);

                if (client != null && client.Connected)
                {
                    WriteMessage(client, null, Opcode.ConnectionCloseFrame);
                    client.Close();
                }

                Closed?.Invoke(this, new ClosedEventArgs(false, client));
                clientTuple.Cancellation.Cancel();
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex, client));
                clientTuple.Cancellation.Cancel();
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void HandleClients()
        {
            while (true)
            {
                if (_serverCancellationToken.IsCancellationRequested) break;

                TcpClient client = _server.AcceptTcpClient();

                var clientCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_serverCancellationToken.Token);

                var clientTask = Task.Run(() =>
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();

                        while (true)
                        {
                            if (_serverCancellationToken.IsCancellationRequested) break;

                            var bytes = ReadClient(client, stream);
                            var str = Encoding.UTF8.GetString(bytes);

                            var isHandshake = Regex.IsMatch(str, "^GET", RegexOptions.IgnoreCase);

                            if (isHandshake)
                            {
                                Handshake(stream, str);
                                Opened?.Invoke(this, new OpenedEventArgs(_endpoint, client));
                            }
                            else
                            {
                                var (opcode, message) = ReadMessage(bytes);

                                if (opcode == Opcode.TextFrame)
                                    DataReceived?.Invoke(this, new DataReceivedEventArgs(message, client));
                                else if (opcode == Opcode.ConnectionCloseFrame)
                                    CloseClient(client);
                                //else if (opcode == Opcode.PongFrame)
                            }
                        }

                        if (client != null && client.Connected)
                            client.Close();
                    }
                    catch (Exception ex)
                    {
                        ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex, client));
                        try { CloseClient(client); }
                        catch { ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(ex, client)); }
                    }
                }, clientCancellationToken.Token);

                _clients.Add((client, clientTask, clientCancellationToken));
            }
        }

        private byte[] ReadClient(TcpClient client, NetworkStream stream)
        {
            while (!stream.CanRead || !stream.DataAvailable) ;
            while (client.Available < 3) ; // match against "get"

            byte[] bytes = new byte[client.Available];
            stream.Read(bytes, 0, client.Available);

            return bytes;
        }

        private void Handshake(NetworkStream stream, string str)
        {
            //Console.WriteLine("=====Handshaking from client=====\n{0}", str);

            // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
            // 3. Compute SHA-1 and Base64 hash of the new value
            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
            string swk = Regex.Match(str, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
            string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
            string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

            // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
            byte[] response = Encoding.UTF8.GetBytes(
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Connection: Upgrade\r\n" +
                "Upgrade: websocket\r\n" +
                "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

            stream.Write(response, 0, response.Length);
        }

        private (Opcode, string) ReadMessage(byte[] bytes)
        {
            bool fin = (bytes[0] & 0b10000000) != 0,
                mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"

            if (!mask) throw new Exception("Mask bit not set");

            int opcodeInt = bytes[0] & 0b00001111;

            if (!Enum.IsDefined(typeof(Opcode), opcodeInt))
                throw new Exception("Invalid opcode");

            var opcode = (Opcode)opcodeInt;

            return (opcode, DecodeMessage(bytes));
        }

        private void WriteMessage(TcpClient client, string message, Opcode opcode = Opcode.TextFrame)
        {
            var stream = client.GetStream();
            var bytes = EncodeMessageToSend(message, opcode);
            stream.Write(bytes);
        }

        private string DecodeMessage(byte[] bytes)
        {
            byte secondByte = bytes[1];
            int dataLength = secondByte & 127;
            int indexFirstMask = 2;

            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            var keys = bytes.Skip(indexFirstMask).Take(4);
            var indexFirstDataByte = indexFirstMask + 4;

            var decoded = new byte[bytes.Length - indexFirstDataByte];
            for (int i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
                decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));

            return Encoding.UTF8.GetString(decoded, 0, decoded.Length);
        }

        private byte[] EncodeMessageToSend(string message, Opcode opcode)
        {
            byte[] response;
            byte[] bytesRaw = message != null ? Encoding.UTF8.GetBytes(message) : new byte[0];
            byte[] frame = new byte[10];

            int indexStartRawData;
            int length = bytesRaw.Length;

            frame[0] = (byte)(0b10000000 | (int)opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            // Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            // Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        private enum Opcode
        {
            ContinuationFrame = 0,
            TextFrame = 1,
            BinaryFrame = 2,
            ConnectionCloseFrame = 8,
            PingFrame = 9,
            PongFrame = 10
        }

        #region Read Example

        //static string ReadMessage(byte[] bytes)
        //{
        //    string message = null;

        //    bool fin = (bytes[0] & 0b10000000) != 0,
        //        mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"

        //    int opcode = bytes[0] & 0b00001111, // expecting 1 - text message
        //        msglen = bytes[1] - 128, // & 0111 1111
        //        offset = 2;

        //    if (msglen == 126)
        //    {
        //        // was ToUInt16(bytes, offset) but the result is incorrect
        //        msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
        //        offset = 4;
        //    }
        //    else if (msglen == 127)
        //    {
        //        Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
        //        // i don't really know the byte order, please edit this
        //        // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
        //        // offset = 10;
        //    }

        //    if ((Opcode)opcode == Opcode.TextFrame)
        //    {
        //        if (msglen == 0)
        //        {
        //            throw new Exception("Received empty message");
        //        }
        //        else if (mask)
        //        {
        //            byte[] decoded = new byte[msglen];
        //            byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
        //            offset += 4;

        //            for (int i = 0; i < msglen; ++i)
        //                decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

        //            message = Encoding.UTF8.GetString(decoded);
        //        }
        //        else
        //        {
        //            throw new Exception("Mask bit not set");
        //        }
        //    }
        //    else if ((Opcode)opcode == Opcode.ConnectionCloseFrame)
        //    {
        //        //CloseConnection();
        //    }
        //    else
        //    {
        //        throw new Exception("Invalid opcode");
        //    }

        //    return message;
        //}

        #endregion
    }
}
