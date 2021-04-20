//using AndroidUsbServer.Services;
//using System;
//using System.Net;
//using System.Threading.Tasks;
//using WebSocket4Net;

//namespace AndroidUsbServer.Droid.Services
//{
//    public class ServerService1 : IServerService
//    {
//        private readonly IPEndPoint _endPoint;
//        private readonly WebSocket _websocket;

//        public event EventHandler Opened;
//        public event EventHandler<AndroidUsbServer.Services.MessageReceivedEventArgs> MessageReceived;
//        public event EventHandler<AndroidUsbServer.Services.DataReceivedEventArgs> DataReceived;
//        public event EventHandler<AndroidUsbServer.Services.ErrorEventArgs> Error;
//        public event EventHandler Closed;

//        public ServerService1()
//        {
//            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
//            IPAddress ipAddress = ipHostInfo.AddressList[0];
//            _endPoint = new IPEndPoint(ipAddress, 12345);

//            //_websocket = new WebSocket($"ws://{_endPoint.Address}:{_endPoint.Port}/");
//            _websocket = new WebSocket($"ws://localhost:2012/");

//            _websocket.Opened += new EventHandler(websocket_Opened);
//            _websocket.DataReceived += new EventHandler<WebSocket4Net.DataReceivedEventArgs>(websocket_DataReceived);
//            _websocket.MessageReceived += new EventHandler<WebSocket4Net.MessageReceivedEventArgs>(websocket_MessageReceived);
//            _websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(websocket_Error);
//            _websocket.Closed += new EventHandler(websocket_Closed);
//        }

//        public async Task<AndroidServer> ListenAsync()
//        {
//            if (_websocket.State != WebSocketState.Closed)
//                await _websocket.CloseAsync();

//            await _websocket.OpenAsync();

//            Action<string> sendAction = (string message) => _websocket.Send(message);

//            return new AndroidServer
//            {
//                Endpoint = _endPoint,
//                Send = sendAction
//            };
//        }

//        public async Task CloseAsync()
//        {
//            await _websocket?.CloseAsync();
//        }

//        public void Dispose()
//        {
//            _websocket?.Close();
//            _websocket?.Dispose();
//        }

//        private void websocket_Opened(object sender, EventArgs e)
//        {
//            Opened?.Invoke(sender, e);
//        }

//        private void websocket_DataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
//        {
//            DataReceived?.Invoke(sender, new AndroidUsbServer.Services.DataReceivedEventArgs(e.Data));
//        }

//        private void websocket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
//        {
//            MessageReceived?.Invoke(sender, new AndroidUsbServer.Services.MessageReceivedEventArgs(e.Message));
//        }

//        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
//        {
//            Error?.Invoke(sender, new AndroidUsbServer.Services.ErrorEventArgs(e.Exception));
//        }

//        private void websocket_Closed(object sender, EventArgs e)
//        {
//            Closed?.Invoke(sender, e);
//        }
//    }
//}