using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using AndroidUsbServer.Droid.Services;
using AndroidUsbServer.Services;
using Xamarin.Forms;

namespace AndroidUsbServer.Droid
{
    [Activity(Label = "AndroidUsbServer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    //[IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    //[IntentFilter(new[] { UsbManager.ActionUsbDeviceDetached })]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // Xamarin Forms (XAML - PCL)
        // DependencyService (registration via attribute vs method)
        // MessagingCenter (Application.Current)
        // BindingContext (MVVM) (http://www.macoratti.net/17/09/xf_dblv1.htm)
        // Intent (https://stackoverflow.com/questions/49351547/xamarin-forms-processing-a-notification-click)

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var usbService = new UsbService(this);
            Xamarin.Forms.DependencyService.RegisterSingleton<IUsbService>(usbService);

            var serverService = new ServerService();
            Xamarin.Forms.DependencyService.RegisterSingleton<IServerService>(serverService);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();

            //register the broadcast receivers
            BroadcastReceiver detachedReceiver = new UsbDeviceDetachedReceiver(this);
            RegisterReceiver(detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
        }

        //protected override void OnNewIntent(Intent intent)
        //{
        //    base.OnNewIntent(intent);
        //    MessagingCenter.Send(Xamarin.Forms.Application.Current, "Intent", intent.Action);
        //}

        class UsbDeviceDetachedReceiver : BroadcastReceiver
        {
            private readonly MainActivity _activity;

            public UsbDeviceDetachedReceiver(MainActivity activity)
            {
                _activity = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                MessagingCenter.Send(Xamarin.Forms.Application.Current, "Intent", intent.Action);
            }
        }
    }
}