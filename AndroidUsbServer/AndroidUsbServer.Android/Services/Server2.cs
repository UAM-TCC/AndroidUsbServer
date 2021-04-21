//using AndroidUsbServer.Services;
//using System;
//using System.IO;
//using System.Net;
//using System.Threading.Tasks;
//using Xamarin.Essentials;

//namespace AndroidUsbServer.Droid.Services
//{
//    public class ServerService2 : IServerService
//    {
//        private readonly IPEndPoint _endPoint;

//        public event EventHandler Opened;
//        public event EventHandler<AndroidUsbServer.Services.MessageReceivedEventArgs> MessageReceived;
//        public event EventHandler<AndroidUsbServer.Services.DataReceivedEventArgs> DataReceived;
//        public event EventHandler<AndroidUsbServer.Services.ErrorEventArgs> Error;
//        public event EventHandler Closed;

//        public ServerService2()
//        {
//            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
//            IPAddress ipAddress = ipHostInfo.AddressList[0];
//            _endPoint = new IPEndPoint(ipAddress, 12345);
//        }

//        public async Task<AndroidServer> ListenAsync()
//        {
//            //var canRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
//            //var canWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

//            // READ_EXTERNAL_STORAGE
//            // WRITE_EXTERNAL_STORAGE

//            var canRead = await Permissions.RequestAsync<Permissions.StorageRead>();
//            var canWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();

//            if (canRead != PermissionStatus.Granted || canWrite != PermissionStatus.Granted)
//                throw new Exception("Permissão de leitura/escrita rejeitada");

//            Action<string> sendAction = (string message) =>
//            {
//                //string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp.txt");
//                var externalStorage = Android.OS.Environment.ExternalStorageDirectory;
//                //var externalStorage = Android.App.Application.Context.GetExternalFilesDir(null);
//                //var externalStorage = Android.OS.Environment.GetExternalStoragePublicDirectory(null);
//                var fileName = Path.Combine(externalStorage.Path, "temp.txt");
//                File.WriteAllText(fileName, message);
//            };

//            await Task.CompletedTask;

//            return new AndroidServer
//            {
//                Endpoint = _endPoint,
//                Send = sendAction
//            };
//        }

//        public async Task CloseAsync()
//        {
//        }

//        public void Dispose()
//        {
//        }
//    }
//}